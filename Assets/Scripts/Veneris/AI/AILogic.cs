/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluentBehaviourTree;
using Veneris.Vehicle;
using System;

namespace Veneris
{
	public class AILogic: MonoBehaviour
	{

		//Keep public to debug on editor, otherwise can be made protected

		#region LogBT

		//Uncomment this if you want to Debug behaviours trees
		//public List<String> logSteps = null;
		public bool showLog=false; //Only used if uncommented above and below

		#endregion

		public BaseCarInputController controller = null;
		public AIBehaviour defaultBehaviour = null;
		public AIBehaviour currentBehaviour = null;
	
		public List<AIBehaviour> taskList = null;
		public bool taskListChanged = false;
		public VenerisRoad currentRoad = null;
		public VenerisLane currentLane = null;
		public IntersectionInfo currentIntersection = null;
		public AgentRouteManager routeManager = null;
		public VehicleManager vehicleManager = null;


		public List<LaneChangeQueueEntry> laneChangesQueue = null;
		public List<LaneChangeQueueEntry> cancelledLaneChangesQueue = null;
		//May have crossed another road and cancelled a lane change inadvertendly



		public VehicleInfo vehicleInfo = null;
		public VehicleVisionPerceptionModel vision = null;
	
		public TriggerEventPublisher vehicleTrigger = null;
		public Vector3 vehicleTriggerColliderHalfSize;
		public Collider vehicleSignalTrigger = null;
		

		//public delegates
		public Action GetPath = null;
		public Action Steer = null;
		public Action<Vector3> DisplaceSteerTo = null;
		public Action Throttle = null;

		public Action<ThrottleMode, ThrottleGoalForPoint > SetThrottleGoal = null;

		protected Collider[] currentLaneBuffer;
		protected int laneMask;
		protected int laneLayer;
		//CarController

		public float throttle {
			set { controller.throttle = value; }
			get { return controller.throttle; }
		}

		public float brake {
			set { controller.brake = value; }
			get { return controller.brake; }
		}

		public float steeringWheelRotation {
			set { controller.steeringWheelRotation = value; }
			get { return controller.steeringWheelRotation; }
		}

		public bool gearUp {
			set { controller.gearUp = value; }
			get { return controller.gearUp; }
		}

		public bool gearDown {
			set { controller.gearDown = value; }
			get { return controller.gearDown; }
		}

		public bool requestReverseGear {
			set { controller.requestReverseGear = value; }
			get { return controller.requestReverseGear; }
		}



		private BoxCollider endOfRoute = null;
		private bool disableAtEndOfRoute = false;
		


		protected virtual void Awake ()
		{


			if (vehicleInfo == null) {
				vehicleInfo = transform.parent.GetComponent<VehicleInfo> ();
			}
			if (controller == null) {
				controller = gameObject.GetComponent<BaseCarInputController> ();

			}
			if (vision == null) {
				vision = GetComponentInChildren<VehicleVisionPerceptionModel> ();
			
			}
			if (vehicleTrigger == null) {

				vehicleTrigger = transform.Find ("VehicleTrigger").GetComponent<TriggerEventPublisher> ();
				vehicleTriggerColliderHalfSize = 0.5f * (vehicleTrigger.GetComponent<BoxCollider> ().size);
				vehicleTrigger.gameObject.layer = LayerMask.NameToLayer ("VehicleTrigger");

			}
			if (vehicleSignalTrigger == null) {
				vehicleSignalTrigger = transform.Find ("VehicleSignalTrigger").GetComponent<Collider> ();
				vehicleSignalTrigger.gameObject.SetActive (false);

				//vehicleSignalTrigger.gameObject.layer = defaultLayer;
			
			}



			if (routeManager == null) {
				routeManager = GetComponent<AgentRouteManager> ();
			}
			taskList = new List<AIBehaviour> ();







		}
		// Use this for initialization
		protected virtual void Start ()
		{
			controller.Init ();

			if (vision != null) {
				vision.AddEnterListener (HandleVisionTriggerEnter);
				vision.AddExitListener (HandleVisionTriggerExit);
			}
			if (vehicleTrigger != null) {
				vehicleTrigger.AddEnterListener (HandleVehicleTriggerEnter);
				vehicleTrigger.AddExitListener (HandleVehicleTriggerExit);
			}

			currentLaneBuffer = new Collider[1];
			laneLayer = LayerMask.NameToLayer ("Lane");
			laneMask = 1 << LayerMask.NameToLayer ("Lane");

			SetDefaultBehaviour (GetComponent<AIBehaviour> ());
			if (currentBehaviour == null) {

				currentBehaviour = defaultBehaviour;
				currentBehaviour.ActivateBehaviour ();
			}

			InitialEnvironmentCheck ();
	

			//Uncomment this if you want to Debug behaviours trees
			//logSteps = new List<string> ();
		}

		#region Log
		//Multiple log functions
		public void Log (string mess)
		{
	
			Debug.Log (Time.time + ": Vehicle=" + vehicleInfo.vehicleId + ". " + mess);
				
		}

		[System.Diagnostics.Conditional ("VENERIS_DEBUG")]
		public void Log (string mess, bool log)
		{

			if (log) {
				Debug.Log (Time.time + ": Vehicle=" + vehicleInfo.vehicleId + ". " + mess);
			}
			


		}

		public void LogError (string mess)
		{
			Debug.LogError (Time.time + ": Vehicle=" + vehicleInfo.vehicleId + ". " + mess);
		}

		public void Log (int id, string mess)
		{
			
			if (vehicleInfo.vehicleId == id) {
				Debug.Log (Time.time + ": Vehicle=" + vehicleInfo.vehicleId + ". " + mess);
			}

		}

		public void LogError (int id, string mess)
		{
			if (vehicleInfo.vehicleId == id) {
				Debug.LogError (Time.time + ": Vehicle=" + vehicleInfo.vehicleId + ". " + mess);
			}
		}

		#endregion

		#region VehicleCapabilities

		public float ComputeTurningRadius (float steerWheelRotation)
		{
			return vehicleInfo.carController.steerControl.ComputeTurningRadius (steeringWheelRotation);
		}

		public float GetMaxGripForce ()
		{

			return vehicleInfo.gripForce;
			//return 800f;

		}


		public float GetSqrBrakingDistanceAtMaxDeceleration ()
		{
			//Basic estimate: assume maxDeceleration  then to stop we need d=v*v/2a, so d*d=v*v*v*v/4*a*a
			//(2*3.4)^2=46.24

			return (vehicleInfo.sqrSpeed * vehicleInfo.sqrSpeed / 4 * vehicleInfo.maxDeceleration * vehicleInfo.maxDeceleration);


		}

		public float GetBrakingDistanceAtMaxDeceleration ()
		{
			//Basic estimate: assume maxDeceleration, then to stop we need d=v*v/2a, 
			//(2*3.4)^2=46.24


			return (vehicleInfo.sqrSpeed / 2 * vehicleInfo.maxDeceleration);


		}

		public float GetEstimatedSqrBrakingDistance (float dec)
		{
			//Basic estimate: use the "real" deceleration

			//Deceleration depends on the time interval, it should be computed by the caller
			//Assuming this is actually a deceleration

			return (vehicleInfo.sqrSpeed * vehicleInfo.sqrSpeed) / (dec * dec * 4f);


		}

		public float GetEstimatedBrakingDistance (float dec, float desiredSpeed)
		{
			//Assuming constant acc/dece, s=(v^2-vo^2)/2a
			return ((vehicleInfo.sqrSpeed) - (desiredSpeed * desiredSpeed) / (2f * dec));
		}

		public float GetEstimatedSqrBrakingDistance (float dec, float desiredSpeed)
		{
			float s = GetEstimatedBrakingDistance (dec, desiredSpeed);
			return (s * s);
		}

		public float GetEstimatedBrakingDistance (float dec)
		{
			//Basic estimate: use the "real" deceleration

			//Deceleration depends on the time interval, it should be computed by the caller
			return (vehicleInfo.sqrSpeed / (Mathf.Abs (dec) * 2f));


		}

		#endregion

		public void SetVehicleManager (VehicleManager man)
		{
			vehicleManager = man;
		}

		public void SetDefaultBehaviour (AIBehaviour b)
		{
			if (b == null) {
				Debug.Log ("No default behaviour");
			} else {
				b.Prepare ();
				defaultBehaviour = b;
			}
		}

		public void ActivateSignalTrigger (Vector3 position)
		{
			vehicleSignalTrigger.transform.position = position;
			ActivateSignalTrigger ();


		}

		public void TurnOnSignalLaneChange (LaneChangeDirection direction, VenerisLane targetLane)
		{
			vehicleInfo.SetWantToChangeLane (direction, targetLane);
		}

		public void TurnOffSignalLaneChange ()
		{

			vehicleInfo.UnsetWantToChangeLane ();
		}

		public void ActivateSignalTrigger ()
		{
			
			vehicleSignalTrigger.gameObject.SetActive (true);

		}

		public void SetSignalTriggerPosition (Vector3 position)
		{
			vehicleSignalTrigger.transform.position = position;

		}

		public void DeactivateSignalTrigger ()
		{
			
			vehicleSignalTrigger.gameObject.SetActive (false);
			vehicleSignalTrigger.transform.localPosition = Vector3.zero;
		}

		public void RecoverDefaultBehaviour ()
		{
			SetCurrentBehaviour (defaultBehaviour);
		}

		public virtual bool RemoveAndReinsert (string reason)
		{

			//Remove and reinsert
			List<VenerisRoad> forwardRoads = routeManager.GetForwardRoads (routeManager.GetNextRoad ());
			if (currentLane != null) {
				currentLane.UnRegisterVehicleWithLane (vehicleInfo);
			}
			//TODO: Internal lanes are not tracked at the moment, but we should update the density
			if (currentIntersection != null) {
				for (int i = 0; i < currentIntersection.internalPaths.Count; i++) {
					VenerisLane lane = currentIntersection.internalPaths [i].GetComponent<VenerisLane> ();
					lane.UnRegisterVehicleWithLane (vehicleInfo);
				}
			}
			if (forwardRoads != null) {
				if (forwardRoads.Count > 0) {
					Log ("Removing and reinserting vehicle");
					vehicleManager.RemoveAndReinsert (vehicleInfo, forwardRoads);
					SimulationManager.Instance.RecordVariableWithTimestamp (vehicleInfo.vehicleId + ".Teleported. Reason=" + reason, forwardRoads [0].sumoId);
					DestroyVehicle ();
					return true;
				} 
					

				
			}
			//End of route. Remove and destroy
			RemoveVehicleFromSimulation ();
			return false;

		}



		public void SetCurrentBehaviour (AIBehaviour b)
		{
			//Disable previous behaviour


			currentBehaviour.DeactivateBehaviour ();

			//Enable new behaviour
			currentBehaviour = b;
			currentBehaviour.ActivateBehaviour ();
		

		}

	

		//Add and execute if first in list
		public virtual void AddBehaviourToTaskList (AIBehaviour ab)
		{
			
			//Preemptive behaviour

			taskList.Add (ab);
			taskList.Sort ();
			taskListChanged = true;
		

		}

	

		public virtual void RemoveBehaviour (AIBehaviour b)
		{
			taskList.Remove (b);
			if (b == currentBehaviour) {
				//Should resort again to check updated priorities
				taskList.Sort ();
			
				Log ("taskList.Count=" + taskList.Count);
				if (taskList.Count > 0) {
					SetCurrentBehaviour (taskList [0]);
				} else {
					RecoverDefaultBehaviour ();
				}
			}
			Destroy (b, 0.1f);

		}

		public virtual  void EndRunningBehaviour (AIBehaviour b)
		{
			
			RemoveBehaviour (b);



		}

		public VenerisLane CheckCurrentLane ()
		{
			//Check the lane we are on in currently

			//Try to avoid allocationss first
			if (currentRoad != null) {
				for (int i = 0; i < currentRoad.lanes.Length; i++) {
					if (currentRoad.lanes [i].IsOnLane (vehicleInfo.backBumper) && currentRoad.lanes [i].IsOnLane (vehicleInfo.frontBumper)) {
						if (currentLane != currentRoad.lanes [i]) {
							currentLane = currentRoad.lanes [i];
						}
						return currentRoad.lanes [i];
					}
				}

			} 





			//Collider[] col=Physics.OverlapBox(transform.position,vehicleTriggerColliderHalfSize, transform.rotation);
			int hits = Physics.OverlapBoxNonAlloc (transform.position, vehicleTriggerColliderHalfSize, currentLaneBuffer, transform.rotation, laneMask);
		

			if (hits > 1) {
				LogError ("Buffer short. Multiple lanes detected on overlapbox");
			} else if (hits == 1) {
				
				VenerisLane lane = currentLaneBuffer [0].GetComponent<VenerisLane> ();
				if (lane.IsOnLane (vehicleInfo.backBumper) && lane.IsOnLane (vehicleInfo.frontBumper)) {
					currentLane = lane;
					currentRoad = lane.GetComponentInParent<VenerisRoad> ();
					//Debug.Log ("AILogic::CheckCurrentLane we are on lane " + lane.sumoId);
					return lane;
				}
			}

			return currentLane;
		}


		private void InitialEnvironmentCheck ()
		{




			Collider[] col = Physics.OverlapBox (transform.position, vehicleTriggerColliderHalfSize, transform.rotation);
		
			for (int i = 0; i < col.Length; i++) {
				
//				Debug.Log (vehicleInfo.vehicleId+ "AIlogic: hits=" + h.collider.name);
				if (col [i].CompareTag ("Road")) {
					

					VenerisRoad road = col [i].GetComponent<VenerisRoad> ();

					currentRoad = road;
					currentLane = road.GetMyLane (vehicleInfo.frontBumper);
				

				

				}
				if (col [i].CompareTag ("Lane")) {
					HandleLaneTag (col [i]);

					/*currentRoad = h.collider.GetComponentInParent<VenerisRoad> ();
						
					VenerisLane lane = h.collider.GetComponent<VenerisLane> ();
					if (lane.IsOnLane (frontBumper)) {
						currentLane = lane;
					}
					
					*/

				}
				if (col [i].CompareTag ("Junction")) {
					HandleJunctionTag (col [i]);
				}

			}
			if (currentLane == null) {
				Log ("AILogic::InitialEnvironmentCheck : not on a lane section");
			}
		}

		// Update is called once per frame
		protected virtual void Update ()
		{
			if (currentBehaviour == null) {
				LogError ("No current behaviour");
			} else {
				
				currentBehaviour.Run ();
				//currentBehaviour.Run (vehicleInfo.vehicleId);



			}
		}

		protected virtual void FixedUpdate ()
		{
			if (Time.timeScale == 0) {
				return;
			}


			if (currentBehaviour == null) {
				LogError ("No current behaviour");
			} else {

				//Safety check for environment variables
				if (currentLane.paths [0] != routeManager.trackedPath) {
					CheckCurrentLane ();
				}
				currentBehaviour.Run ();

				//Uncomment this if you want to Debug behaviours trees
				/*	logSteps.Clear ();

				currentBehaviour.Run (logSteps);

				//currentBehaviour.Run (vehicleInfo.vehicleId);

				if (showLog) {
					for (int i = 0; i < logSteps.Count; i++) {
						Log (logSteps [i]);
					}
				}
				*/

			}
		}


		protected virtual void HandleVisionTriggerEnter (Collider other)
		{
			if (other.tag == "Intersection") {
				//				Debug.Log ("Intersection seen " +other.transform.parent.name + " "+other.transform.name);
				if (other.GetComponent<AIBehaviourProvider> () != null) {
					HandleBehaviour (other.gameObject);
				}

			}
		}

		protected virtual void HandleVisionTriggerExit (Collider other)
		{
			if (other.tag == "Intersection") {
				AIBehaviourProvider provider = other.GetComponent<AIBehaviourProvider> ();
				if (provider != null) {
					//Log (10,"Checking validity of " + other.transform.root.name);
					provider.CheckBehaviourValidity (gameObject);
				}
			}
		}

		public void HandleBehaviour (GameObject provider)
		{
			//Debug.Log (provider.GetComponent<AIBehaviourProvider> ());
			AIBehaviour newBehaviour;
			provider.GetComponent<AIBehaviourProvider> ().SetBehaviour (gameObject, out newBehaviour);
			//Check if taskList have been modified
		
			if (taskList [0] != currentBehaviour) {
				if (taskList [0].GetPriority () < currentBehaviour.GetPriority ()) {
					SetCurrentBehaviour (taskList [0]);
				}
			}
				
		}

		protected virtual void  HandleRoadTag (Collider other)
		{
			//There is a road under us, but if we are on an elevated position, there may be others...
			//TODO: check that this is actually our road
			//Debug.Log (vehicleInfo.vehicleId+ "--AIlogic:hevt:checking road");

			VenerisRoad road = other.GetComponent<VenerisRoad> ();

			currentRoad = road;
			currentLane = road.GetMyLane (vehicleInfo.frontBumper);
		}

		protected virtual void  HandleLaneTag (Collider other)
		{
			//TODO: we should define unambiguosly WHEN a vehicle is on a road/lane, or at least something like enterLane, fullOnLane or something
			currentRoad = other.GetComponentInParent<VenerisRoad> ();
			VenerisLane lane = other.GetComponent<VenerisLane> ();

			//if (lane.IsOnLane (frontBumper)) {
			//In turns, the vehicle trigger enters the lane before the front bumper, just assume that we are in the lane and let components check if bumper is on lane when necessary

			currentLane = lane;

			//Update Path
			Path p = routeManager.GetPathInLane (lane);
			if (p != null) {

				routeManager.SetCurrentPath (p);

			}

			//}
		}

		protected virtual void  HandleJunctionTag (Collider other)
		{

			IntersectionInfo info = other.GetComponentInParent<IntersectionInfo> ();
			currentIntersection = info;
			if (info == null) {
				//Log (other.name + " junction has no IntersectionInfo");
			} else {
				//Update Path
				Path p = routeManager.GetFirstPathInJunction (info);
				if (p != null) {
					//if (p!=routeManager.GetTrackedPath()) {
					routeManager.SetCurrentPath (p);
					//}
				}
			
			}


		}

		protected virtual void HandleEndOfRoute (Collider other)
		{
			if (disableAtEndOfRoute) {
				//Debug.Log (vehicleInfo.vehicleId + " -- Finishing route and disabling");
				//Debug.Log (vehicleInfo.vehicleId + " -- Finishing route and destroying");

				if (currentLane != null) {
					currentLane.UnRegisterVehicleWithLane (vehicleInfo);
				}
				if (vehicleManager != null) {
					vehicleManager.EndOfRouteReached (vehicleInfo);
				}
				//endOfRoute.gameObject.SetActive (false);
				//transform.parent.gameObject.SetActive (false);
				DestroyVehicle ();
				return;
			}
		}

		public void RemoveVehicleFromSimulation (bool destroy = true)
		{
			if (vehicleManager != null) {
				vehicleManager.RemovedVehicle (vehicleInfo);
			}
			if (destroy) {
				DestroyVehicle ();
			}

		}

		public void DestroyVehicle ()
		{
			Destroy (endOfRoute.gameObject);
			Destroy (transform.parent.gameObject);
		}

		protected virtual void  HandleBehaviourTriggerTag (Collider other)
		{
			

			HandleBehaviour (other.gameObject);
		
		}

		protected virtual void HandleVehicleTriggerEnter (Collider other)
		{
		
			//Debug.Log ("AI, other collider=" + other.name);
			//Debug.Log ("AI, other collider tag=" + other.tag);

			if (endOfRoute == other) {
				HandleEndOfRoute (other);
				return;
			}


			if (other.gameObject.layer == laneLayer) { 
				
			
				HandleLaneTag (other);
				return;

			}

			if (other.CompareTag ("Road")) {

				HandleRoadTag (other);
				return;


			}
			if (other.CompareTag ("BehaviourTrigger")) {
				
				HandleBehaviourTriggerTag (other);
				return;
			}
			if (other.CompareTag ("Junction")) {

				HandleJunctionTag (other);
				return;
			}

		}

		protected virtual void HandleVehicleTriggerExit (Collider other)
		{
			if (other.CompareTag ("Junction")) {
				IntersectionInfo info = other.GetComponentInParent<IntersectionInfo> ();
				if (currentIntersection == info) {
					currentIntersection = null;
				}
				return;
			}
		}

		public void SetDisableOnArrivingEndOfRoute ()
		{
			//Create a trigger to inform us of the end of path
			GameObject go = new GameObject ("End of route for vehicle " + vehicleInfo.vehicleId); 
			endOfRoute = go.AddComponent<BoxCollider> ();

			go.transform.position = routeManager.GetLastPathInRoute ().GetLastNode ().transform.position;
			go.transform.rotation = Quaternion.LookRotation (routeManager.GetLastPathInRoute ().interpolatedPath [routeManager.GetLastPathInRoute ().interpolatedPath.Length - 1].tangent);
			endOfRoute.size = new Vector3 (5f, 2f, 5f);
			endOfRoute.isTrigger = true;
			disableAtEndOfRoute = true;
		}

		public virtual void AddCancelledLaneChange (LaneChangeQueueEntry entry)
		{
			if (cancelledLaneChangesQueue == null) {
				cancelledLaneChangesQueue = new List<LaneChangeQueueEntry> ();
			}
			if (!cancelledLaneChangesQueue.Contains (entry)) { //Check if we already have it
				//Log("Adding lane cahnge for "+entry.targetPId);
				cancelledLaneChangesQueue.Add (entry);
			}
		}

		public virtual void AddStrategicLaneChange (LaneChangeQueueEntry entry)
		{
			if (laneChangesQueue == null) {
				laneChangesQueue = new List<LaneChangeQueueEntry> ();
			}
			if (!laneChangesQueue.Contains (entry)) { //Check if we already have it
				//Log("Adding lane cahnge for "+entry.targetPId);
				laneChangesQueue.Add (entry);
			}
		}

		public virtual void RemoveStrategicLaneChange (LaneChangeQueueEntry entry)
		{
			laneChangesQueue.Remove (entry);
		}

		public virtual void RemoveCancelledLaneChange (LaneChangeQueueEntry entry)
		{
			cancelledLaneChangesQueue.Remove (entry);
		}

		public virtual void RemoveStrategicLaneChange (Path startPath)
		{
			for (int i = 0; i < laneChangesQueue.Count; i++) {
				if (startPath.pathId == laneChangesQueue [i].startPid) {
					RemoveStrategicLaneChange (laneChangesQueue [i]);
					return;
				}
			}
			
		}

		public virtual bool CheckForLaneChangeInPath (Path path, out LaneChangeQueueEntry entry)
		{
			//Log ("Checking for lane change in path");
			entry = GetStrategicLaneChange (path.pathId);
			if (entry == null) {
				//Check cancelled
				entry = GetCancelledLaneChange (path.pathId);
				if (entry == null) {
					return false;
				} else {
					return true;
				}

					
			} else {
				return true;
			}
		

		}

		public virtual LaneChangeQueueEntry GetStrategicLaneChange (VenerisLane lane)
		{
			for (int i = 0; i < laneChangesQueue.Count; i++) {
				if (lane == laneChangesQueue [i].startLane) {
					return laneChangesQueue [i];
				}
			}
			return null;
		}

		public virtual LaneChangeQueueEntry GetStrategicLaneChange (long startid)
		{
			for (int i = 0; i < laneChangesQueue.Count; i++) {
				if (startid == laneChangesQueue [i].startPid) {
					return laneChangesQueue [i];
				}
			}
			return null;
		}

		public virtual LaneChangeQueueEntry GetCancelledLaneChange (long startid)
		{
			for (int i = 0; i < cancelledLaneChangesQueue.Count; i++) {
				if (startid == cancelledLaneChangesQueue [i].startPid) {
					return cancelledLaneChangesQueue [i];
				}
			}
			return null;
		}

		public virtual LaneChangeQueueEntry GetFirstStrategicLaneChange ()
		{
			return laneChangesQueue [0];
		}

		public virtual void LaneChangeCancelled (LaneChangeQueueEntry origin)
		{
			RemoveStrategicLaneChange (origin);
			//Move to cancelled lane changes
			AddCancelledLaneChange (origin);

		}

		public  virtual void LaneChangeCompleted (LaneChangeQueueEntry origin)
		{


			if (origin.laneChangeCompletedListeners != null) {
				//Call delegates

				origin.laneChangeCompletedListeners ();
			}
			//routeManager.FinishLaneChange (origin.targetPId);
			RemoveStrategicLaneChange (origin);
			RemoveCancelledLaneChange (origin);

		}



	}
}
