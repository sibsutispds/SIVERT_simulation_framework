/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluentBehaviourTree;
namespace Veneris
{
	public class IntersectionBehaviour :   AIBehaviour
	{
		public enum IntersectionAction
		{
			Undefined,
			StraightWithoutBraking,
			PrepareToTurnWithPriority,
			PrepareToStop,
			StopAtInternalJunction}

		;

		public enum IntersectionApproachAction
		{
			Undefined,
			TrackTrafficLight,
			GivenByIntersectionAction,
			TrafficLightPassed,
		}

		//To quickly compare triggers. Set up by provider
		public BoxCollider stopLineCollider=null;
		public MeshCollider junctionCollider = null;

		public AILogic ailogic=null;
		protected ThrottleProportionalControllerActionBTHelper throttleHelper = null; //TODO: change to delegates of ailogic or proper interface
		public IntersectionInfo intersection=null;
		public Transform stopLinePosition = null;


		public PathConnector connector = null;
		public VenerisRoad fromRoad = null; //Road from which we are approaching the intersection



		public Path internalPath = null;
		public long pathIdForPriority=-1;
		public ConnectionInfo.PathDirectionInfo plannedPath=null;
		public IntersectionAction action;
		public IntersectionApproachAction approachAction;
		public ConnectionInfo.ConnectionDirection currentConnectionDirection;
		//public IntersectionStop.TrafficLightStateTrack tlTrack=null;

		public bool intersectionStopLineReached=false; //True when we reach the stopline
		public bool internalLaneEndReached = false; //We have reached the end of the internal lane

		public  ThrottleGoalForPoint throttleGoal=null;

		public TrafficLightTracker tlBehaviour = null;
		public IBehaviourTreeNode intersectionBehaviour = null;

		public float intersectionTimerStart=-1f;
		public float maxTimeAtIntersection = 120f;
		public float jammedDensity=0.9f;
		public float minimumPathLengthToWaitForJam=12f;
		public bool waitingForJam = false;
		public void Awake ()
		{
			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();
			}
			if (throttleHelper == null) {
				throttleHelper = GetComponent<ThrottleProportionalControllerActionBTHelper> ();
			}

		}



		public bool HasReachedIntersectionStop() {
			return  (throttleHelper.HasStoppedAtGoalPoint () && intersectionStopLineReached == true);
		}

		public bool OnIntersection() {
			return (ailogic.currentIntersection == intersection);
		}

		public void SetThrottleGoal(ThrottleGoalForPoint goal) {
			throttleGoal = goal;
		}

		public override void Prepare ()
		{
			fromRoad = ailogic.routeManager.GetPathFromId (pathIdForPriority).GetComponentInParent<VenerisRoad> ();
		}

	

		public void CheckColliders() {
			//Every time we set an action that requires a trigger check, we should check if we are already on the trigger, since ontriggerenter will not be called
			ExtDebug.DrawBox (ailogic.vehicleInfo.carBody.position, ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation, Color.white);
			Collider[] cols=Physics.OverlapBox(ailogic.vehicleInfo.carBody.position,ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation);
			for (int i = 0; i < cols.Length; i++) {
				HandleEnterVehicleTrigger (cols [i]);
			}
		}

		public override void ActivateBehaviour ()
		{
			base.ActivateBehaviour ();
			//Check current lane, just in case
			ailogic.CheckCurrentLane();
			CheckColliders ();
		}
		public void SetApproachActionAndPriority() {
			GetPriority = GetPriorityByDistanceInRoute;
			ailogic.vehicleTrigger.AddEnterListener (HandleEnterVehicleTrigger);
			if (approachAction == IntersectionApproachAction.TrackTrafficLight && tlBehaviour != null) {
				//cache intersection behaviour

				intersectionBehaviour = mainBehaviour;
				mainBehaviour = TrackTrafficLightTree ();

			}
		}

		public IBehaviourTreeNode TrackTrafficLightTree () {
			//Merge traffic light tracker with driving behaviour
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Parallel("drive and check traffic light",2,1).
				Splice(tlBehaviour.mainBehaviour).
				Splice(ailogic.defaultBehaviour.mainBehaviour).
				End(). //Parallel
			Build();
		}

		public void EndTrafficLightTrack() {
			
			//Recover intersection Behaviour
			mainBehaviour=intersectionBehaviour;
			//tlBehaviour = null;
			approachAction = IntersectionApproachAction.TrafficLightPassed;


		}

		public void SetAILogic(AILogic l) {
			ailogic = l;
		}

		public void SetThrottleHelper(ThrottleProportionalControllerActionBTHelper t) {
			throttleHelper = t;
		}

		public void SetIntersection(IntersectionInfo i) {
			intersection = i;
			junctionCollider = i.junction.GetComponent<MeshCollider> ();
		}


		public void SetStopLinePosition(Transform t, BoxCollider  bc) {
			stopLinePosition = t;
			stopLineCollider = bc;
		}
		public void SetPathConnector(PathConnector c) {
			connector = c;
		
		}
		public void SetPlannedPath(ConnectionInfo.PathDirectionInfo pair) {
			plannedPath = pair;
		}

		public bool SetStopAtStopLine(float distance=-1f) {
			if (distance < 0) {

				if (ailogic.currentLane == null) {
					//Wait until we have a lane
					return  false;
				}
				float distanceToStopLine;
				if (ComputeDistanceToStopLine (out distanceToStopLine)) {
				
					distance = distanceToStopLine;
				

				} else {
					//ComputeDistanceToStopLine (stopLinePosition,pair);
					throw new UnityException ();
				}
			} 	

			throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, 0.5f, ailogic.currentLane.speed, distance, ailogic.vehicleInfo.totalDistanceTraveled, stopLineCollider, true);

			throttleHelper.SetStopAtPoint (throttleGoal);
			throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);
			CheckColliders ();
			return true;
		}

		public void ReduceSpeedToStopLine (float speedLimit) {
			if (ailogic.currentLane.speed > speedLimit) {
				float distanceToStopLine; 
				if (ComputeDistanceToStopLine (out distanceToStopLine)) {
					
					throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, speedLimit, ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled, stopLineCollider);
					throttleHelper.SetSpeedAtPoint (throttleGoal);
					CheckColliders ();
				} else {
					//ComputeDistanceToStopLine (stopLinePosition,pair);
					throw new UnityException ();
				}

			}
		}


		public bool ComputeDistanceToStopLine(out float dist) {
			if (ailogic.routeManager.lookAtPath!=null && ailogic.routeManager.lookAtPath.pathId == pathIdForPriority) {
				dist = ailogic.routeManager.DistanceToEndOfLookAtPath ();
				return true;
			}
			return ComputeDistanceToStopLine(stopLinePosition, plannedPath, out  dist);
		}

		public bool ComputeDistanceToStopLine(Transform stop, ConnectionInfo.PathDirectionInfo p, out float dist) {

			if (ailogic.currentLane.endIntersection == stop) {
				//We are alreday on the lane 
			
				Path currentPath=ailogic.currentLane.paths[0];
				int index=currentPath.FindClosestPointInInterpolatedPath(ailogic.vehicleInfo.carBody);
				dist = currentPath.GetPathDistanceFromIndexToEnd (index);
				return true;

			}
			//Try with the road
			if (intersection.GetFromRoadFromStopLine (stop)==ailogic.currentRoad) {
				//Find the lane corresponding to the stop

				for (int i = 0; i < ailogic.currentRoad.lanes.Length; i++) {
					if (ailogic.currentRoad.lanes [i].endIntersection == stop) {
						Path currentPath=ailogic.currentRoad.lanes [i].paths[0];
						int index=currentPath.FindClosestPointInInterpolatedPath(ailogic.vehicleInfo.carBody);
						dist = currentPath.GetPathDistanceFromIndexToEnd (index);
					
						return true;
					}
				}
			}



			//TODO: Is it critical to solve this issue
			PathConnector pc = stop.GetComponentInChildren<PathConnector> ();
			List<long> inc = pc.GetIncomingPathsToConnector ();

			//ailogic.Log ("pair.p"+ p.p.pathId);
			for (int i = 0; i < inc.Count; i++) {
				//ailogic.Log ("inc[i"+i+"]="+inc [i] + "conn="+pc.IsPathIdConnectedTo (inc [i], p.p.pathId));
				if (pc.IsPathIdConnectedTo (inc [i], p.p.pathId)) {
					//ailogic.Log ("inc[i"+i+"]="+inc [i] + "conn="+pc.IsPathIdConnectedTo (inc [i], p.p.pathId) + "dist="+ailogic.routeManager.GetPathDistanceToEndOfPath (inc [i]));
					if (ailogic.routeManager.GetPathDistanceToEndOfPath (inc [i], out dist) ){

						return true;
					} else {
						//May have been problems with overlap interesections. Try again
						Path clane=ailogic.routeManager.GetPathInLane (ailogic.currentLane);
					
						if (clane != null) {
							//ailogic.Log ( "1");
							if (ailogic.routeManager.GetPathDistanceFromToEndOfPath (ailogic.routeManager.GetPathInLane (ailogic.currentLane).pathId, inc [i], out dist) == true) {
								return true;
							}
						} else {
							//ailogic.Log ( "2");
							if (ailogic.routeManager.GetPathDistanceFromToEndOfPath (ailogic.routeManager.trackedPath.pathId, inc [i], out dist)) {
								return true;
							}
						}


					}

				}

			}
		
			//ailogic.Log ( "3");
			dist=-1f;
			//Try to recover 
			Transform me=ailogic.vehicleInfo.carBody;
			Vector3 lstop = me.InverseTransformPoint (stop.position);
			//ailogic.Log ("Recover from compute distance");
			//Debug.Break ();
			if (lstop.z > 0) {
				//It is in front of me, try eculidean 
				dist=lstop.magnitude;
				return true;
			} else {
				//We should ignore this stop
				return false;
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus StartIntersectionTimer() {
			intersectionTimerStart = Time.time;
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}


		public FluentBehaviourTree.BehaviourTreeStatus  SetDrivingCheckPositions(List<Transform> check) {
			//Debug.Log (ailogic.vehicleInfo.vehicleId+" Setting driving check " + check);
			ailogic.vision.SetCheckPositions (check);

			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}

		public FluentBehaviourTree.BehaviourTreeStatus  UnsetDrivingCheckPositions() {
			ailogic.vision.UnsetCheckPositions ();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}


		public float GetPriorityByDistanceInRoute ()
		{
			

			priorityValue = ailogic.routeManager.CheckEndDistanceIfPathIsInRouteForward (pathIdForPriority);
			return priorityValue;
			//return ailogic.routeManager.CheckEndDistanceIfPathIsInRouteForward (pathIdForPriority);

		}

		public FluentBehaviourTree.BehaviourTreeStatus SetAdaptToCurvature() {
			//throttleHelper.throttleMode = ThrottleProportionalControllerActionBTHelper.BrakeCondition.AdaptToCurvature;
			throttleHelper.SetAdaptToCurvature();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public  FluentBehaviourTree.BehaviourTreeStatus SetCrossingIntersectionState() {
			ailogic.vehicleInfo.SetCrossingIntersection ();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public  FluentBehaviourTree.BehaviourTreeStatus SetDrivingState() {
			ailogic.vehicleInfo.SetDriving ();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus SetApplyBehaviour() {

			throttleHelper.SetApplyBehaviour();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus SetNextPathSpeedLimit() {


			if (ailogic.routeManager.lookAtPath != null) {
				if (ailogic.routeManager.lookAtPath.GetComponent<VenerisLane> () != null) {
					throttleHelper.SetSpeedLimit (ailogic.routeManager.lookAtPath.GetComponent<VenerisLane> ().speed);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
			}


			throttleHelper.SetCurrentLaneOrDefaultFreeSpeed ();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus SetDefault() {

			SelfFinished ();
			ailogic.vehicleInfo.SetDriving ();
			ailogic.EndRunningBehaviour (this);

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus WaitIfJammed() {
			VenerisLane internalLane = internalPath.GetComponent<VenerisLane> ();
			if (internalLane.paths[0].totalPathLength>minimumPathLengthToWaitForJam) { //Too short lanes can be jammed just with us
				if (internalLane.occupancy >= jammedDensity) {
					if (internalLane.registeredVehiclesList.Count == 1) {
						if (internalLane.registeredVehiclesList [0] == ailogic.vehicleInfo) {//Too short lanes can be jammed just with us
							return FluentBehaviourTree.BehaviourTreeStatus.Success;
						}
							
					}
					//Check that vehicles are stopped
					for (int i = 0; i < internalLane.registeredVehiclesList.Count; i++) {
						if (internalLane.registeredVehiclesList [i] == ailogic.vehicleInfo) {
							continue;
						}
						if (internalLane.registeredVehiclesList [i].sqrSpeed > 0.02) {
							return FluentBehaviourTree.BehaviourTreeStatus.Success;
						}
					}
					//Keep waiting
					ailogic.vehicleInfo.SetWaitingForClearance ();
					//ailogic.Log("Waiting because it is jammed");
					//Debug.Break ();
					waitingForJam = true;
					return FluentBehaviourTree.BehaviourTreeStatus.Running;
				}
			} 
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
				

		}
		public FluentBehaviourTree.BehaviourTreeStatus ReCheckTrafficLight() {
			if (waitingForJam) {//We have been waiting, have to recheck tl
				if (approachAction == IntersectionApproachAction.TrafficLightPassed) {
					tlBehaviour.CheckTrafficLightState ();
					if (tlBehaviour.tlTrack.trafficLightState == TrafficLight.TrafficLightState.Red) {
						//Keep waiting
						//ailogic.Log ("Waiting because  traffic light is red after jam");
						return FluentBehaviourTree.BehaviourTreeStatus.Running;
					}
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		/*public bool SetBrakeToStop (Vector3 position, float distance, bool forceStop = false)
		{
			//throttleHelper.throttleMode=ThrottleProportionalControllerActionBTHelper.BrakeCondition.TargetCurvature;

			//Set free speed according to curvature of the internal path and speed limit
			if (ailogic.currentLane == null) {
				//Wait until we have a lane
				return  false;
			}

			throttleGoal = new ThrottleGoalForPoint (position, 0.5f, ailogic.currentLane.speed, distance, ailogic.vehicleInfo.totalDistanceTraveled,  forceStop);

			throttleHelper.SetStopAtPoint (throttleGoal);
			throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);
			return true;



		}
		*/



		void OnDestroy() { 
			//Remove listeners
			throttleGoal=null;
			ailogic.vehicleTrigger.RemoveEnterListener(HandleEnterVehicleTrigger);
			mainBehaviour = null;
			tlBehaviour = null;
			//ailogic.vehicleTrigger.RemoveExitListener (HandleExitVehicleTrigger);
		}
		/*protected virtual void HandleEnterVehicleTrigger (Collider other)
		{
			//if (!running) {
			//Do not do anything if this behaviour is not active
			//	return;
			//}

			//First make sure that the trigger is related to this intersection
			ailogic.Log (10, "Trigger of  " + intersection.name + "other="+other.name);

			if (other.GetComponentInParent<IntersectionInfo> () != intersection) {
				return;
			}

			if (other.CompareTag ("Intersection")) {

				if (stopLinePosition == other.transform) {
					ailogic.Log (10, "Stop line of " + intersection.name);
					intersectionStopLineReached = true;

					if (throttleGoal != null) {
						throttleGoal.areaReached = true;
					}

				}

			}
			if (other.CompareTag ("ConnectorTrigger")) {

				PathConnector pc = other.GetComponentInChildren<PathConnector> ();
				if (pc == null) {
					ailogic.LogError ("Connector should not be null"+other.transform.root.name);
				}
				if (internalPath == null) {
					ailogic.Log ("running="+running);
					ailogic.Log ("Name of trigger=" + other.name + " name of intersection triggered = " + other.GetComponentInParent<IntersectionInfo> ().intersectionId + "name of my intersection=" + intersection.name);
					ailogic.LogError ("Internal path should not be null"+internalPath);
				}


				ConnectionInfo.PathDirectionInfo n = ailogic.routeManager.GetPathIfIsInConnector(pc,internalPath.pathId,ailogic.routeManager.FollowingPathId(internalPath.pathId));
				if (n != null) {

					internalLaneEndReached = true;
				} 
			}
		}*/
		protected virtual void HandleEnterVehicleTrigger (Collider other)
		{
			//if (!running) {
			//Do not do anything if this behaviour is not active
			//	return;
			//}


		
			if (throttleGoal != null) {
				if (throttleGoal.areaTrigger == other) {
					throttleGoal.areaReached = true;
				}
			}


			if (other == stopLineCollider) {


					
				intersectionStopLineReached = true;



				return;

			}

			if (other == junctionCollider) {
				if (intersectionStopLineReached == false) {
					//May have missed the stopLine collider. Check now

					Collider[] ov= Physics.OverlapBox(ailogic.vehicleTrigger.transform.position,ailogic.vehicleTriggerColliderHalfSize);
					//ailogic.Log ("OverlapBox in Intersection behaviour found " + ov.Length);
					for (int i = 0; i < ov.Length; i++) {
						if (ov [i] == stopLineCollider) {

							intersectionStopLineReached = true;


							break;
						}
					}
				}
				return;
			}
			if (other.CompareTag ("ConnectorTrigger")) {
				//First make sure that the trigger is related to this intersection
				if (other.GetComponentInParent<IntersectionInfo> () != intersection) {
					return;
				}

				PathConnector pc = other.GetComponentInChildren<PathConnector> ();
				if (pc == null) {
					ailogic.LogError ("Connector should not be null"+other.transform.root.name);
				}
				if (internalPath == null) {
					ailogic.Log ("running="+running);
					ailogic.Log ("Name of trigger=" + other.name + " name of intersection triggered = " + other.GetComponentInParent<IntersectionInfo> ().intersectionId + "name of my intersection=" + intersection.name);
					ailogic.LogError ("Internal path should not be null"+internalPath);
				}


				/*ConnectionInfo.PathDirectionInfo n = ailogic.routeManager.GetPathIfIsInConnector(pc,internalPath.pathId,ailogic.routeManager.FollowingPathId(internalPath.pathId));
				if (n != null) {

					internalLaneEndReached = true;
				} 
				*/
				if (ailogic.routeManager.FollowingPathId (internalPath.pathId)!=-1) {
					internalLaneEndReached = true;
				}
			}
		}

	}
}
