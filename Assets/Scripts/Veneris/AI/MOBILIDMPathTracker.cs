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
	

	public class MOBILIDMPathTracker : AIBehaviour
	{

	


		[System.Serializable]
		public class LaneChangeStatus
		{
			
			//public List<LaneChangeQueueEntry> laneChangesQueue = null;
			public bool delayStart=false;
			public int sectionToStart=0;
			public bool laneChangePending = false;
			public bool hasSentRequest = false;
			public LaneChangeRequest requestSent = null;
			public SingleLaneChangeSequence laneChangeSequence;
			public LaneChangeStatus() {
				laneChangeSequence = new SingleLaneChangeSequence();

				Clear();
			}
			public void Clear() {
				laneChangePending = false;
				requestSent = null;
				sectionToStart=0;
				delayStart=false;
				hasSentRequest = false;




			}
		}


		protected AILogic ailogic = null;

		public float checkDecceleration
		{
			get { return ailogic.brake; }
		}
		public float checkThrottle
		{
			get { return ailogic.throttle; }
		}



		public bool showLog=false;
		public ProportionalPathTrackerActionBTHelper steerHelper = null;
		public MOBILIDMIDMInteractionActionBTHelper throttleHelper = null;

		public LeadingVehicleSelector leadingVehicleSelector = null;
		public bool useCoroutineFrontCheck = false;
		public float lastCheckFrontTime = 0f;

		public bool setBrake = false;

		[SerializeField]
		private float _checkFrontTime =0.1f; //Time to check for leader vehicle: Could be considered a reaction time
		public float checkFrontTime {
			get { return _checkFrontTime; }
			set {
				_checkFrontTime = value;

			}
		}

	

		[System.Serializable]
		public class EvasiveManeuverData
		{
			//Time to wait until two vehicles are considered locked
		



			public float startTime = 0f;
			public float maxTimeBeforeTeleporting = 0f;
			//public float evasiveAngleStep = 10f;
			//public float maxEvasiveAngle = 30f;
			public bool executeEvasiveManeuver = false;
			public bool evasiveManeuverStarted = false;
			public Collider[] evasiveBuffer;
			public VehicleInfo insideFrontVehicle = null;
			public Collider insideVehicleCollider = null;
			//public float selectedAngle = 0f;
			//public Vector3 steerLocalPoint;
			//public float lastTimeAngleComputed = 0f;
			//public float angleComputationInterval=0.5f;
			//public bool angleSelected = false;
			public GameObject evasivePath=null;
			public Path cachedLookAtPath = null;
			public EvasiveManeuverData() {
				evasiveBuffer = new Collider[2];
			}

			public void Reset ()
			{
				insideFrontVehicle = null;
				insideVehicleCollider = null;
				//selectedAngle = 0f;
				//lastTimeAngleComputed = 0f; //To force first chekc
				executeEvasiveManeuver = false;

			
				//angleSelected = false;
				evasivePath=null;
				cachedLookAtPath = null;
				startTime = 0f;
				maxTimeBeforeTeleporting = 0f;
				evasiveManeuverStarted = false;
			}

		}



		public EvasiveManeuverData evasiveManeuverData = null;



		public LaneChangeStatus currentLaneChange = null;
		public float minTimeToChangeLane = 5f;
		public float minLaneChangeSpeed =0.5f;

		public bool forceSpeedToFinishLaneChange = false;
		public bool brakeForcedToFinishLaneChange=false;

		public LeaderInfo farAwayLeader = null;
	
		public LeaderInfo closeLeader = null;

		public TimerData blockTimer = null;
		public VehicleInfo potentialBlocker = null;


		public bool examineCloseVehicles = false;

		
		public bool EmergencyBrake = false;
		public bool IsEmergencyEnabled
		{
			get { return EmergencyBrake;}
			set { EmergencyBrake = value;}
		}
		
		public bool IBrake = false;
		public bool IsIBrake
		{
			get { return IBrake;}
			set { IBrake = value;}
		}

		private bool enableScenario = false;
		
		public bool setEnableScenario
		{
			get { return enableScenario;}
			set { enableScenario = value;}
		}
		
		public List<GameObject> breakLightGo;
		
		//public LaneChangeSequence laneChangeSequence = null;

		// Use this for initialization
		void Awake ()
		{
			FindObjectwithTag("BrakeLights", this.transform.root.transform);
			
			
			if (steerHelper == null) {
				steerHelper = GetComponent<ProportionalPathTrackerActionBTHelper> ();
			}
			if (throttleHelper == null) {
				throttleHelper = GetComponent<MOBILIDMIDMInteractionActionBTHelper> ();
			}



		}
		// Use this for initialization
		void Start ()
		{
			behaviourName = "MOBILIDMPathTracker";

		}

		public override void Prepare ()
		{
			base.Prepare ();

			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();
				


			}

			if (ailogic.vehicleManager != null) {
				ailogic.vehicleManager.AddRemoveListener (HandleDestroyTrigger);
			}
	
			//Fill ailogic delegates
			ailogic.GetPath=this.GetPath;
			ailogic.Steer = this.Steer;
			ailogic.DisplaceSteerTo = this.DisplaceSteerTo;
			ailogic.Throttle = this.Throttle;

			ailogic.SetThrottleGoal = SetThrottleGoal;


			mainBehaviour = GetTree ();
		

			ailogic.vehicleTrigger.AddEnterListener (HandleEnterVehicleTrigger);

			currentLaneChange = new LaneChangeStatus ();
			farAwayLeader = new LeaderInfo ();
			closeLeader = new LeaderInfo ();
			leadingVehicleSelector = new LeadingVehicleSelector (ailogic, throttleHelper.idmSafetyGap,farAwayLeader);
			throttleHelper.SetLeadingVehicleSelector (leadingVehicleSelector);
		
			lastCheckFrontTime =Time.time;
			evasiveManeuverData = new EvasiveManeuverData ();
			blockTimer = new TimerData (10f);
			examineCloseVehicles = false;


		}
		protected void HandleDestroyTrigger (VehicleInfo info)
		{

		

			if (leadingVehicleSelector.leaderInfo.leader != null) {
				if (leadingVehicleSelector.leaderInfo.leader == info) {
					leadingVehicleSelector.UnsetLeadingVehicle ();
				}
			}

			if (evasiveManeuverData.insideFrontVehicle != null) {
				if (evasiveManeuverData.insideFrontVehicle== info) {
					if (evasiveManeuverData.executeEvasiveManeuver == true) {
						EndEvasiveManeuver ();
					}
				}
			}
			if (closeLeader.leader != null) {
				if (closeLeader.leader==info) {
					closeLeader.Clear ();
				}
			}
			if (farAwayLeader.leader != null) {
				if (farAwayLeader.leader == info) {
					farAwayLeader.leader = null;
					UnsetLeadingVehicle ();
				}
			}

		}


		protected void SetLeadingVehicle (VehicleInfo i, LeaderReason reason)
		{

			//leadingVehicle.activeLeadingVehicle = true;
			//leadingVehicle.vehicle = i;
			if (i != potentialBlocker) {
				blockTimer.Unset ();
				potentialBlocker = null;
			} else {
				if (reason == LeaderReason.InFrontNotBlocking) {
					if (blockTimer.Check ()) {
						ailogic.Log ("Mutual blocking with " + i.vehicleId + " for more than " + blockTimer.maxTime,showLog);
						ailogic.RemoveAndReinsert ("Mutual blocking with " + i.vehicleId + " for more than " + blockTimer.maxTime);
					}
				}
			}

			ailogic.vehicleInfo.leadingVehicle = i;
			ailogic.vehicleInfo.leaderReason = reason;
		}
		protected bool CheckMutualBlocking(VehicleInfo i) {
			if (i.leadingVehicle == ailogic.vehicleInfo) {
				if (i.leaderReason == LeaderReason.InFrontNotBlocking) {
					return true;
				}
			}
			return false;
		}
		protected void UnsetLeadingVehicle ()
		{
			//leadingVehicle.activeLeadingVehicle = false;
			//leadingVehicle.vehicle = null;
			leadingVehicleSelector.UnsetLeadingVehicle();
			ailogic.vehicleInfo.leadingVehicle = null;
			ailogic.vehicleInfo.leaderReason = LeaderReason.None;
		}

		protected void CheckFrontPeriodicallyNoCoroutine() {
			UnsetLeadingVehicle();
			leadingVehicleSelector.DecideLeadingVehicle ();
			SetLeadingVehicle (leadingVehicleSelector.leaderInfo.leader, leadingVehicleSelector.leaderInfo.reason);
			//if (i!=null) {
			//	SetLeadingVehicle (i);
			//}
		}


			
			




		void OnDestroy ()
		{
			if (ailogic != null) {
				if (ailogic.vehicleManager != null) {
					ailogic.vehicleManager.RemoveRemoveListener (HandleDestroyTrigger);
				}
			}
		}


		public void GetPath() {
			if (GetPathPoint () == FluentBehaviourTree.BehaviourTreeStatus.Success) {
				return;
			} else {
				GetNextPathPoint ();
			}
		}
		public void Steer() {
			steerHelper.ProportionalSteerController ();
		}
		public void Throttle() {
			throttleHelper.IDMThrottleControl ();
		}
		public void Throttle(float f) {
			throttleHelper.ApplyThrottle (f);
		}
		public void DisplaceSteerTo (Vector3 displacement) {
			steerHelper.SteerRelativeToCurrentPoint (displacement);
		}

		public void SetThrottleGoal(ThrottleMode mode, ThrottleGoalForPoint goal) {
			if (mode == ThrottleMode.StopAtPoint) {
				throttleHelper.SetStopAtPoint (goal);
				return;
			}
			if (mode == ThrottleMode.SpeedAtPoint) {
				throttleHelper.SetSpeedAtPoint (goal);
				return;
			}
		}


		public IBehaviourTreeNode GetLaneChangeTree() {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			/*return builder.	
				
				//Selector("MOBIL-IDM-Tracker::Get next path point").
				Sequence("Execute or check lane change ").
					Sequence("Lane Change action ").
						Do("MOBIL-IDM-Tracker::is executing previous change",()=>IsExecutingLaneChange()). //If success, go on until it has finished
					//Condition ("MOBIL-IDM-Tracker: lane change pending", ()=> LaneChangePending ()).//If false go to parallel and drive normally
						
						Condition("MOBIL-IDM-Tracker wait until end of turning", ()=>HasFinishedTurning()). //Wait until we are on the lane to proceed
						Condition("MOBIL-IDM-Tracker: is backBumper on lane",()=>IsBackBumperOnLane()). //Wait until our car is completely on the lane to start maenuver
						Do ("acquire lane change path ", ()=> SetLaneChangeRequest ()). //If failure, should go to parallel
						Do("Decide to start or delay change", ()=>StartOrDelayLaneChange()).
						Do("CheckArrivingEndOfLane",()=>CheckArrivingEndOfLane()).
						Condition("Is change possible?", ()=>IsLaneChangePossible()).
						Do("Start lane change maneuver",() =>StartLaneChangeManeuver()).
						//Do("Try lanechange",()=>throttleHelper.TryLaneChange()).
						//Condition("Is Maneuver finished?",()=>{return currentLaneChange.requestSent.isFinished;}). //This should be true if we have finished TryLaneChange
						Do("Wait to finish lane change",()=>IsLaneChangeFinished()). 
						Do("Finish lane change maenuver and set next one or default",()=>FinishCurrentChange()).
					End().//Sequence lane change
					Do("MOBIL-IDM-Tracker:: check for lane change and set current path",()=>CheckForLaneChangeInCurrentPath()). //If failure go to parallel and drive normally
				End(). //Sequence
			Build ();
*/
		
			return builder.	


					
						
				Selector("Execute or check lane change").
					Sequence("Check or go on").
						Condition ("procced with checking or finish execution", ()=> ProceedWithCheckForLaneChange ()).//If failure go to check new change
						Do("check for lane change and set current path",()=>CheckForLaneChangeInCurrentPath()). //If success go to parallel and drive normally. Next step will start lane change
					End().
					Sequence("Lane Change actions ").
						Selector("Prepare for lane change or wait until it is finished").
							Sequence("Check executing").
								Condition("is executing maneuver",()=>IsExecutingLaneChange()).
								Do("Wait to finish lane change",()=>IsLaneChangeFinished()). 
								Do("Finish lane change maenuver and set next one or default",()=>FinishCurrentChange()).
							End().//Sequence executing
							Sequence("Prepare to lane change").
								Condition("wait until end of turning", ()=>HasFinishedTurning()). //Wait until we are on the lane to proceed
								Condition(" is backBumper on lane",()=>IsBackBumperOnLane()). //Wait until our car is completely on the lane to start maenuver
								Do ("acquire lane change path ", ()=> SetLaneChangeRequest ()). 
								Do("Decide to start or delay change", ()=>StartOrDelayLaneChange()).
								Do("CheckArrivingEndOfLane",()=>CheckArrivingEndOfLane()).
								Condition("Is change possible?", ()=>IsLaneChangePossible()).
								Do("Start lane change maneuver",() =>StartLaneChangeManeuver()).
							End(). //Sequence prepare
						End(). //Selector Prepare for lane change or wait until it is finished
					End().//Sequence Lane Change actions
				End(). //Selector
			Build ();


		}

		public IBehaviourTreeNode GetExamineEnvironmentTree () {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Sequence ("Examine Environment").
					Condition("is time to examine environment?",()=>IsTimeToExamineEnvironment()).
					Do ("Examine far away vehicles", () => ExamineFarAwayVehicles ()).
					Do ("Examine close vehicles", () => ExamineCloseVehiclesMinimum ()).
					Do("Select leader",()=>SelectLeader()).
				End ().
				Build ();
		}
		public IBehaviourTreeNode GetFollowPathTree () {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Parallel ("MOBIL-IDM-Tracker:::follow-path", 2, 2).
					Do ("MOBIL-IDM-Tracker:::steer", ()=> ApplySteer()).
					Do("Decide free speed goal", ()=>DecideSpeed()).
					Do ("MOBIL-IDM-Tracker:::throttle", ()=> ApplyThrottle ()).
				End ().//Parallel
			Build ();
		}

		public IBehaviourTreeNode GetEvasiveManeuverTree () {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Selector("Evasive Manevuer").
					Sequence ("Execute evasive maneuver ").
						Do ("SetUp evasive maneuver", () => SetUpEvasiveManeuver ()).
						Do ("Is safe to start evasive Maneuver", ()=>IsSafeToStartEvasiveManeuver()).
						Selector("End maneuver or follow path").
							Do("Get point in evasive path or end manevuer",()=>GetPointOrEndManeuver()).
							Do(" End maneuver",()=>EndEvasiveManeuver()).
						End().
						Do ("steer to evasive path", ()=> ApplySteer ()).
						Do("Decide free speed goal", ()=>DecideSpeed()).
				//Do("Ignore leader if is blocking", ()=>IgnoreBlockingVehicle()).
						Do ("Evasive Throttle", () =>  ApplyThrottle ()).
					End ().
					Do("True node", ()=>{return FluentBehaviourTree.BehaviourTreeStatus.Success;}). //Make sure we do not go to other action until the maenuver is ended
				End(). //Selector
			Build ();
		}
		public IBehaviourTreeNode GetTree ()
		{
			//	return GetBasicTree ();


			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();

			return builder.
				Sequence ("start-moving").
					ExecuteUntilSuccessNTimes ("set speed limit once", 1).//Only run succesfully once the following sequence
						Sequence("Start paths and speed").
							Do("Set starting path",()=>SetStartingPath()).
							Do ("Set speed limit", ()=> SetSpeedLimit ()).
						End().
					End ().
					Selector("MOBIL-IDM-Tracker::Get next path point"). //When  one fails, go to the other
				//Do ("MOBIL-IDM-Tracker::get point in current path", ()=> GetPathPoint()).
						Sequence("MOBIL-IDM-Tracker::Try to get point in current  path").
							Do("MOBIL-IDM-Tracker:: find point current path",()=> GetPathPoint()).
							
						End().
						Sequence("MOBIL-IDM-Tracker::Try to get point in the next path").
							//TODO: it works OK at the moment,it minimizes the problem of short roads but a change implies normally a lane change, we should check safety...
							Do("MOBIL-IDM-Tracker:: find point the next path",()=>GetNextPathPoint()).
						//Do("MOBIL-IDM-Tracker:: check lane change and set current path",()=>CheckForLaneChangeInNextPath()).
						End().
						Do("Go to parallel if others fail",()=>{return FluentBehaviourTree.BehaviourTreeStatus.Success;}). //Required to go to parallel if those fail
					End().
					Parallel("Try lane Change and Drive",3,1).
						Splice(GetExamineEnvironmentTree()).
						Selector("Driving actions").
							Do("Check C-ITS scenario message", ()=> DoCitsScenario()).
							Sequence("Evasive Maneuver").
								Condition("execute evasive maneuver?",()=>{return evasiveManeuverData.executeEvasiveManeuver;}).
								Splice(GetEvasiveManeuverTree()).
							End().
							Parallel("Normal driving",2,1).
								Splice(GetLaneChangeTree()).
								Splice(GetFollowPathTree()).
							End().
						End().//Selector
					End(). //Parallel

				End(). //Sequence
			Build();

		}

		public FluentBehaviourTree.BehaviourTreeStatus DoCitsScenario()
		{
			// if (Input.GetKeyDown(KeyCode.B))
			// {
			// 	enableScenario = true;
			// }
			
			if ((this.transform.parent.name == "Vehicle 0") & (enableScenario))
			{
				throttleHelper.ApplyThrottle (-1f);
				this.IsIBrake = true;
				// Debug.Log("I in EEBL Emergency braking: " + this.transform.parent.name);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			
			if (IsEmergencyEnabled)
			{
				throttleHelper.ApplyThrottle (-8f);
				
				
				foreach (var lights in breakLightGo)
				{
					lights.GetComponent<Renderer>().enabled = true;
				}
				
				// this.transform.root.transform.GetChild(11).GetChild(37).gameObject.GetComponent<Renderer>().enabled =true;
				
				Debug.Log("Emergency brake by: " + this.transform.parent.name + " at time: " + Time.time);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			else
			{
				foreach (var lights in breakLightGo)
				{
					lights.GetComponent<Renderer>().enabled = false;
				}
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus ApplyThrottle() {
			if (setBrake) {
				ailogic.Log ("SetBrake=true", showLog);
				throttleHelper.ApplyThrottle (-1f);
				setBrake = false;

			}
			else {
				 throttleHelper.IDMThrottleControl ();
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus ApplySteer() {

			return steerHelper.ProportionalSteerController ();
		}


		public bool IsTimeToExamineEnvironment() {
			if ((Time.time - lastCheckFrontTime) >= checkFrontTime) {
				
				lastCheckFrontTime = Time.time;
				return true;
			} else {
				return false;
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus IsSafeToStartEvasiveManeuver () {
			if (evasiveManeuverData.evasiveManeuverStarted==false) {
				
				setBrake = true;
				float angle = steerHelper.GetSteeringWheelRotationToPosition (ailogic.routeManager.lookAtPath.GetFirstNode ().transform.position);
				if (ailogic.vision.CheckRotatedPositionOccupiedByOtherVehicle (ailogic.vehicleInfo.carBody.position, angle)) {
					
					return FluentBehaviourTree.BehaviourTreeStatus.Failure;
				} else {
					setBrake = false;
					evasiveManeuverData.evasiveManeuverStarted = true;
				}
				
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus GetPointOrEndManeuver() {
			if ((Time.time-evasiveManeuverData.startTime)>evasiveManeuverData.maxTimeBeforeTeleporting) {
				ailogic.Log("Time.time-blockSolverData.startTime="+(Time.time-evasiveManeuverData.startTime));
				if (ailogic.currentIntersection != null) {
					ailogic.RemoveAndReinsert ("blockSolverData.maxTimeBeforeTeleporting.Intersection=" + ailogic.currentIntersection.sumoJunctionId + ":Lane=" + ailogic.currentLane);
				} else {
					ailogic.RemoveAndReinsert ("blockSolverData.maxTimeBeforeTeleporting.Lane=" + ailogic.currentLane);
				}
			
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;

			}
			if (evasiveManeuverData.insideFrontVehicle == null) {
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
			if (ailogic.vehicleInfo.backBumper.InverseTransformPoint( evasiveManeuverData.insideFrontVehicle.carBody.position).z<0f) {
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
			return GetPathPoint ();
		}
		public FluentBehaviourTree.BehaviourTreeStatus IgnoreBlockingVehicle () {
			if (leadingVehicleSelector.leaderInfo.leader == evasiveManeuverData.insideFrontVehicle) {
				leadingVehicleSelector.SetLeader (null, 0.0f, LeaderReason.IgnoreBlocking);
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus EndEvasiveManeuver() {
			//recover lookatpath
			ailogic.routeManager.SetLookAtPath(evasiveManeuverData.cachedLookAtPath);
			evasiveManeuverData.executeEvasiveManeuver = false;
			Destroy (evasiveManeuverData.evasivePath);
			evasiveManeuverData.Reset ();
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus  SelectLeader() {
			//Log
			//ailogic.Log("Close leader="+closeLeader.ToString()+". Far away leader="+farAwayLeader.ToString());
			if (closeLeader.leader == null) {
				/*if (farAwayLeader.leader == null) {
					//No leader, free speed

					return  FluentBehaviourTree.BehaviourTreeStatus.Success;
				} else {
					//Already set


				}*/
				SetLeadingVehicle(leadingVehicleSelector.leaderInfo.leader,leadingVehicleSelector.leaderInfo.reason);
				return  FluentBehaviourTree.BehaviourTreeStatus.Success;
			} else {
				/*if (blockSolverData.executeEvasiveManeuver == true) {
					if (closeLeader.leader == blockSolverData.insideFrontVehicle) {
						ailogic.Log("Close leader="+closeLeader.ToString()+". Far away leader="+farAwayLeader.ToString());
						return  FluentBehaviourTree.BehaviourTreeStatus.Success;
					}
				}*/
				leadingVehicleSelector.SetLeader (closeLeader.leader,closeLeader.distance,closeLeader.reason);
				return  FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
		



			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus  ExamineFarAwayVehicles() {
			UnsetLeadingVehicle();
			examineCloseVehicles= leadingVehicleSelector.SelectLeadingVehicleWithoutCloseVehicles ();


			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		/*public FluentBehaviourTree.BehaviourTreeStatus ExamineCloseVehiclesInSafetyArea() { 
			closeLeader.Clear ();
			if (examineCloseVehicles) {
				
				List<Collider> vehicles = ailogic.vision.GetVehiclesInFrontSafetyArea ();
				float facingMinimimunSqrDistance = float.MaxValue;
				float insideMinimimunSqrDistance = float.MaxValue;

				VehicleInfo blockingVehicle = null;
				VehicleInfo insideFrontVehicle = null;
				float distSqr = 0.0f;
				for (int i = 0; i < vehicles.Count; i++) {
					
					VehicleSafetyAreaDetector detector = vehicles [i].GetComponent<VehicleSafetyAreaDetector> ();
					distSqr = (detector.info.frontBumper.position - ailogic.vehicleInfo.frontBumper.position).sqrMagnitude;
					if (detector.position == VehicleSafetyAreaDetector.AreaPosition.Front) {
						//Make sure it is facing us
						if (Vector3.Angle (ailogic.vehicleInfo.carBody.forward, detector.info.carBody.forward) > 90f) {
							if (distSqr < facingMinimimunSqrDistance) {
								facingMinimimunSqrDistance = distSqr;
								blockingVehicle = detector.info;
							}
						}
					} else if (detector.position == VehicleSafetyAreaDetector.AreaPosition.Back) {
						//Consider this a normal situation
					} else {
						if (distSqr < insideMinimimunSqrDistance) {
							insideMinimimunSqrDistance = distSqr;
							insideFrontVehicle = detector.info;
						}
					}
				}
				if (blockingVehicle != null) {
					if (ailogic.vehicleInfo.currentActionState != VehicleInfo.VehicleActionState.WaitingForClearance) { //Do not try to avoid until we are moving again
						if (blockSolverData.insideFrontVehicle == null) {
							if (blockingVehicle.currentActionState == VehicleInfo.VehicleActionState.EvasiveManeuver) {
								//Wait until the other one has finished its maneuver, but set leader to avoid collision
								closeLeader.SetLeader (blockingVehicle, Mathf.Sqrt (facingMinimimunSqrDistance), LeaderReason.WaitEvasiveManeuver);
								//SetLeadingVehicle (blockingVehicle);
							} else {
								blockSolverData.insideFrontVehicle = blockingVehicle;
								blockSolverData.insideVehicleCollider = blockingVehicle.carCollider;
								blockSolverData.executeEvasiveManeuver = true;
								ailogic.vehicleInfo.SetEvasiveManevuer ();
							}
						} else if (blockSolverData.insideFrontVehicle != blockingVehicle) {
							ailogic.Log ("Different blocking vehicle " + blockSolverData.insideFrontVehicle.vehicleId + "new=" + blockingVehicle.vehicleId);
							Debug.Break ();
							//TODO: what to do? go to teleport...?
						}
					}
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
				if (insideFrontVehicle != null) {
					if (insideFrontVehicle.sqrSpeed <= 0.02f) {
						//Consider the vehicle stopped	
						//Check front
						ailogic.Log ("Checking inside for vehicle " + insideFrontVehicle.vehicleId, showLog);

						if (leadingVehicleSelector.IsCloseVehicleInFront (insideFrontVehicle)) {


							//Now, check what happens if we throttle to our steerpoint
							if (leadingVehicleSelector.IsSteeringPointSafe (steerHelper.steerLookAheadPoint.position)) {
								//Try to steer to avoid blocks
								//ailogic.Log ("Turning radius=" + ailogic.ComputeTurningRadius (steerHelper.GetSteeringWheelRotationToLookAheadPoint ()));
								return FluentBehaviourTree.BehaviourTreeStatus.Success;
							} else {
								ailogic.Log ("steering to point not safe");
								//Debug.DrawLine (ailogic.vehicleInfo.carBody.position, steerHelper.steerLookAheadPoint.position, Color.white);


							}
						} else if (leadingVehicleSelector.IsCloseVehicleInVelocityTrajectory (insideFrontVehicle,ailogic.ComputeTurningRadius(steerHelper.GetSteeringWheelRotationToLookAheadPoint()),10)) { 
							ailogic.Log ("vehicle in velocity trajectory "+ insideFrontVehicle.vehicleId);
						} else {
							//Try to advance
							return FluentBehaviourTree.BehaviourTreeStatus.Success;
						}
					}
					//Make this our leader, otherwise we can collide with it
					closeLeader.SetLeader (insideFrontVehicle, Mathf.Sqrt (insideMinimimunSqrDistance), LeaderReason.InsideAndFront);
					//SetLeadingVehicle (insideFrontVehicle);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}


				//Now get vehicles on the sides and let them pass is they want to change lane
				float wantChangeMinimimunSqrDistance = float.MaxValue;
				VehicleInfo insideWantLaneChangeVehicle = null;
				vehicles = ailogic.vision.GetVehiclesInLeftSafetyArea ();
				for (int i = 0; i < vehicles.Count; i++) {
					VehicleSafetyAreaDetector detector = vehicles [i].GetComponent<VehicleSafetyAreaDetector> ();
					distSqr = (detector.info.frontBumper.position - ailogic.vehicleInfo.frontBumper.position).sqrMagnitude;
					if (detector.info.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane || detector.info.currentActionState == VehicleInfo.VehicleActionState.ChangingLane) {
						if (detector.info.turnSignal == VehicleInfo.TurnSignalState.Right) {
							if (distSqr < wantChangeMinimimunSqrDistance) {
								wantChangeMinimimunSqrDistance = distSqr;
								insideWantLaneChangeVehicle = detector.info;
							}
						}
					}
				}
				vehicles = ailogic.vision.GetVehiclesInRightSafetyArea ();
				for (int i = 0; i < vehicles.Count; i++) {
					VehicleSafetyAreaDetector detector = vehicles [i].GetComponent<VehicleSafetyAreaDetector> ();
					distSqr = (detector.info.frontBumper.position - ailogic.vehicleInfo.frontBumper.position).sqrMagnitude;
					if (detector.info.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane || detector.info.currentActionState == VehicleInfo.VehicleActionState.ChangingLane) {
						if (detector.info.turnSignal == VehicleInfo.TurnSignalState.Left) {
							if (distSqr < wantChangeMinimimunSqrDistance) {
								wantChangeMinimimunSqrDistance = distSqr;
								insideWantLaneChangeVehicle = detector.info;
							}
						}
					}
				}
				if (insideWantLaneChangeVehicle != null) {
					//Make this leader
					closeLeader.SetLeader (insideWantLaneChangeVehicle, Mathf.Sqrt (wantChangeMinimimunSqrDistance), LeaderReason.InsideAndChangingLane);
					//SetLeadingVehicle (insideWantLaneChangeVehicle);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;


		}
		*/

		public FluentBehaviourTree.BehaviourTreeStatus ExamineCloseVehiclesMinimum() {
			closeLeader.Clear ();
			if (examineCloseVehicles) {
				//Check if someone is in front of me
				VehicleInfo blockingVehicle = null;
				VehicleInfo inFrontNotBlocking = null;
				VehicleInfo insideWantLaneChangeVehicle = null;

				Collider blockingCollider = null;
				for (int j = 0; j < leadingVehicleSelector.insideList.Count; j++) {

					VehicleInfo i = leadingVehicleSelector.insideList [j];
					/*if (leadingVehicleSelector.IsVehicleCollidingInTheFuture (i, 10)) {
						//ExtDebug.DrawBox (ailogic.vehicleInfo.carBody.position + ailogic.vehicleInfo.velocity*(10*Time.fixedDeltaTime), ailogic.vehicleTriggerColliderHalfSize, ailogic.vehicleInfo.carBody.rotation, Color.white);
						//ExtDebug.DrawBox (i.carBody.position + i.velocity*(10*Time.fixedDeltaTime), i.aiLogic.vehicleTriggerColliderHalfSize, i.carBody.rotation, Color.white);


						if (ailogic.vehicleInfo.carBody.InverseTransformPoint (i.carBody.position).z > 0) {
							if (i.carBody.InverseTransformPoint (ailogic.vehicleInfo.carBody.position).z <= 0) {
								ailogic.Log ("Braking. About to collide with " + i.vehicleId);
								setBrake = true;
							}
						}

					}*/

				
					Collider c;
					if (leadingVehicleSelector.IsCloseVehicleInFront (i, out c) || leadingVehicleSelector.IsCloseVehicleInVelocityTrajectory (i,ailogic.ComputeTurningRadius(steerHelper.GetSteeringWheelRotationToLookAheadPoint()),10)) {
						if (evasiveManeuverData.executeEvasiveManeuver == true) {
							if (i == evasiveManeuverData.insideFrontVehicle) {
								continue;
							}
						}
						if (leadingVehicleSelector.IsCloseVehicleBlocking (i)) {
							
							//Select the vehicle which has us as leader to facilitate resolving locks
							if (blockingVehicle == null) {
								blockingVehicle = i;
								blockingCollider = c;
							} else {
								if (i.leadingVehicle == ailogic.vehicleInfo) {
									blockingVehicle = i;
									blockingCollider = c;
								}
							}
							ailogic.Log ("In front. Blocking=" + blockingVehicle.vehicleId,showLog);

						} else {
							if (inFrontNotBlocking == null) {
								inFrontNotBlocking = i;

							} else {
								if (i.leadingVehicle == ailogic.vehicleInfo) {
									inFrontNotBlocking = i;

								}
							}
							ailogic.Log ("In front.  Not Blocking=" + inFrontNotBlocking.vehicleId, showLog);
						}

						//If it is changing lane, let it pass
						if (i.currentActionState == VehicleInfo.VehicleActionState.ChangingLane || i.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane) {
							if (i.targetLaneChange == ailogic.currentLane) {
								//We may have more than one....
								insideWantLaneChangeVehicle = i;

								ailogic.Log ("insideWantLaneChangeVehicle=" + insideWantLaneChangeVehicle.vehicleId, showLog);

							}
						} 

					} else {
						ailogic.Log ("No problem" +i.vehicleId, showLog);
					}
				}

				if (blockingVehicle != null) {
					DecideBlockingVehicleAction (blockingVehicle, leadingVehicleSelector.GetTTCInfoFromInsideList (blockingVehicle).distance);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
				if (inFrontNotBlocking != null) {
					DecideInFrontNotBlockingVehicleAction (inFrontNotBlocking, leadingVehicleSelector.GetTTCInfoFromInsideList (inFrontNotBlocking).distance);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
				if (insideWantLaneChangeVehicle) {
					//Make this leader
					closeLeader.SetLeader(insideWantLaneChangeVehicle,leadingVehicleSelector.GetTTCInfoFromInsideList(insideWantLaneChangeVehicle).distance,LeaderReason.InsideAndChangingLane);
					SetLeadingVehicle (insideWantLaneChangeVehicle,LeaderReason.InsideAndChangingLane);
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}


			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public void DecideBlockingVehicleAction(VehicleInfo blockingVehicle, float distance) {
			if (ailogic.vehicleInfo.currentActionState == VehicleInfo.VehicleActionState.WaitingForClearance || ailogic.vehicleInfo.currentActionState == VehicleInfo.VehicleActionState.WaitingAtRedLight) {
					return;
			}//Do not try to avoid until we are moving again
			if (evasiveManeuverData.insideFrontVehicle == null) {
				if (blockingVehicle.currentActionState == VehicleInfo.VehicleActionState.EvasiveManeuver) {
					//Wait until the other one has finished its maneuver, but set leader to avoid collision
					closeLeader.SetLeader (blockingVehicle, distance, LeaderReason.WaitEvasiveManeuver);
					SetLeadingVehicle (blockingVehicle, LeaderReason.WaitEvasiveManeuver);
				} else {
					evasiveManeuverData.insideFrontVehicle = blockingVehicle;
					evasiveManeuverData.insideVehicleCollider = blockingVehicle.carCollider;
					evasiveManeuverData.executeEvasiveManeuver = true;
					ailogic.vehicleInfo.SetEvasiveManevuer ();
					//New leader will be selected when we set up evasive maneuver
				}
			} else if (evasiveManeuverData.insideFrontVehicle != blockingVehicle) {
				ailogic.Log ("Different blocking vehicle " + evasiveManeuverData.insideFrontVehicle.vehicleId + "new=" + blockingVehicle.vehicleId);
				//Debug.Break ();
				//TODO: what to do? go to teleport...?
				ailogic.RemoveAndReinsert("Different blocking vehicle " + evasiveManeuverData.insideFrontVehicle.vehicleId + "new=" + blockingVehicle.vehicleId);
			}

		}

		public void DecideInFrontNotBlockingVehicleAction(VehicleInfo inFrontNotBlocking, float distance) {
			if (inFrontNotBlocking.sqrSpeed <= 0.02f) {
				//Consider the vehicle stopped	
				//Check what happens if we throttle to our steerpoint
				//Try a few longer lookaheads and check if it is safe
				float increaseFactor=1.25f;
				Path prevLookAtPath = ailogic.routeManager.lookAtPath;
				for (int i = 0; i < 3; i++) {
					
					if (leadingVehicleSelector.IsSteeringPointSafe (steerHelper.steerLookAheadPoint.position)) {
						//Try to steer to avoid blocks
						return;
					} else {

					

						Debug.DrawLine (ailogic.vehicleInfo.carBody.position, steerHelper.steerLookAheadPoint.position, Color.yellow);
						float lookAhead = steerHelper.lookAheadDistance * increaseFactor;

						//ailogic.Log ("steering to point not safe, old= "+steerHelper.lookAheadDistance+"new="+lookAhead);
						steerHelper.lookAheadDistance = lookAhead;
						if (steerHelper.GetPathPoint (lookAhead) == FluentBehaviourTree.BehaviourTreeStatus.Failure) {
							steerHelper.GetNextPathPoint ();
						}



					}
				}

				//Didnt work. Recover path
				steerHelper.SetLookAtPath(prevLookAtPath);
			}
			//Make this our leader, otherwise we can collide with it
			ailogic.Log("Close leader is ="+inFrontNotBlocking, showLog);
			//throttleHelpier.showLog = true;

			closeLeader.SetLeader(inFrontNotBlocking,distance,LeaderReason.InFrontNotBlocking);
			SetLeadingVehicle (inFrontNotBlocking,LeaderReason.InFrontNotBlocking);
			//Track potential blocking
			if (CheckMutualBlocking (inFrontNotBlocking)) {
				potentialBlocker = inFrontNotBlocking;
				if (!blockTimer.IsSet ()) {
					ailogic.Log ("Setting block timer =" + Time.time, showLog);
					blockTimer.Set ();
				}
			}
		}


		/*public FluentBehaviourTree.BehaviourTreeStatus ExamineCloseVehicles() {
			//TODO: consider multiple outcomes on the same category, list..
			//Possible outcomes
			VehicleInfo blockingVehicle = null;
			VehicleInfo insideFrontVehicle = null;
			
			VehicleInfo insideWantLaneChangeVehicle = null;
			Collider blockingCollider = null;
			for (int j = 0; j < leadingVehicleSelector.insideList.Count; j++) {
				
				VehicleInfo i=leadingVehicleSelector.insideList[j];
				//ailogic.Log ("Examining close vehicles" + i.vehicleId);
				Collider c;
				if (leadingVehicleSelector.IsCloseVehicleInFront (i,out c)) {
					if (leadingVehicleSelector.IsCloseVehicleBlocking (i)) {
						//Select the vehicle which has us as leader to facilitate resolving locks
						if (blockingVehicle == null) {
							blockingVehicle = i;
							blockingCollider = c;
						} else {
							if (i.leadingVehicle == ailogic.vehicleInfo) {
								blockingVehicle = i;
								blockingCollider = c;
							}
						}
					} else {
						if (insideFrontVehicle == null) {
							insideFrontVehicle = i;

						} else {
							if (i.leadingVehicle == ailogic.vehicleInfo) {
								insideFrontVehicle = i;

							}
						}
					}


				} else {
					//Not in front yet, but may want to change lane
					//If it is changing lane, let it pass
					if (i.currentActionState == VehicleInfo.VehicleActionState.ChangingLane || i.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane) {
						if (i.targetLaneChange == ailogic.currentLane) {
							//We may have more than one....
								insideWantLaneChangeVehicle = i;



						}
					} 
				}
					
					
					


			}

			//Now decide
			//First, if it is blocking... try to resolve block...


			if (blockingVehicle != null) {
				if (ailogic.vehicleInfo.currentActionState != VehicleInfo.VehicleActionState.WaitingForClearance) { //Do not try to avoid until we are moving again
					if (blockSolverData.insideFrontVehicle == null) {
						if (blockingVehicle.currentActionState == VehicleInfo.VehicleActionState.EvasiveManeuver) {
							//Wait until the other one has finished its maneuver, but set leader to avoid collision
							leadingVehicleSelector.SetLeader(blockingVehicle,leadingVehicleSelector.GetTTCInfoFromInsideList(blockingVehicle).distance,LeaderReason.WaitEvasiveManeuver);
							SetLeadingVehicle (blockingVehicle);
						} else {
							blockSolverData.insideFrontVehicle = blockingVehicle;
							blockSolverData.insideVehicleCollider = blockingCollider;
							blockSolverData.executeEvasiveManeuver = true;
							ailogic.vehicleInfo.SetEvasiveManevuer ();
						}
					} else if (blockSolverData.insideFrontVehicle != blockingVehicle) {
						ailogic.Log ("Different blocking vehicle "+blockSolverData.insideFrontVehicle.vehicleId +"new="+blockingVehicle.vehicleId);
						//Debug.Break ();
						//TODO: what to do? go to teleport...?
					}
				}
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			} 
			if (insideFrontVehicle != null) {
				if (insideFrontVehicle.sqrSpeed <= 0.02f) {
				//Consider the vehicle stopped	
					//Check what happens if we throttle to our steerpoint
					if (leadingVehicleSelector.IsSteeringPointSafe (steerHelper.steerLookAheadPoint.position)) {
						//Try to steer to avoid blocks
						return FluentBehaviourTree.BehaviourTreeStatus.Success;
					} else {
						ailogic.Log ("steering to point not safe");
						Debug.DrawLine (ailogic.vehicleInfo.carBody.position, steerHelper.steerLookAheadPoint.position, Color.white);


					}
				}
				//Make this our leader, otherwise we can collide with it
				leadingVehicleSelector.SetLeader(insideFrontVehicle,leadingVehicleSelector.GetTTCInfoFromInsideList(insideFrontVehicle).distance,LeaderReason.InFrontNotBlocking);
				SetLeadingVehicle (insideFrontVehicle);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			if (insideWantLaneChangeVehicle) {
				//Make this leader
				leadingVehicleSelector.SetLeader(insideWantLaneChangeVehicle,leadingVehicleSelector.GetTTCInfoFromInsideList(insideWantLaneChangeVehicle).distance,LeaderReason.InsideAndChangingLane);
				SetLeadingVehicle (insideWantLaneChangeVehicle);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		*/
		public FluentBehaviourTree.BehaviourTreeStatus SetUpEvasiveManeuver() {
			/*Quaternion rightEvasive = Quaternion.AngleAxis (evasiveAngleStep, Vector3.up);

			ailogic.Log ("lookahead" + steerHelper.GetLocalSteerLookAheadPoint().ToString("F6"));
			Vector3 currentSteer = rightEvasive * steerHelper.GetLocalSteerLookAheadPoint();
			ailogic.Log ("evasive manevuer steer" + currentSteer.ToString("F6"));

			Debug.DrawLine (steerHelper.steerLookAheadPoint.position, ailogic.vehicleInfo.carBody.TransformPoint(currentSteer), Color.green);
			*/
			//if (Time.time - blockSolverData.lastTimeAngleComputed > blockSolverData.angleComputationInterval) {
			if (evasiveManeuverData.evasivePath==null) {
			/*Vector3 center = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (0f, 0.6f, ailogic.vehicleInfo.vehicleLength * 0.5f));
				float rangle = 0f;
				while (rangle <= blockSolverData.maxEvasiveAngle) {
					int hits = ailogic.vision.CheckRotatedPositionOccupiedByVehicle (center, rangle, blockSolverData.evasiveBuffer);
					if (hits > blockSolverData.evasiveBuffer.Length) {
						hits = blockSolverData.evasiveBuffer.Length;
						ailogic.Log ("Leader buffer short");
					} else if (hits == 1) {
						//This is me,
						ailogic.Log ("Found rangle with no block=" + rangle);
						break;
					}
					ailogic.Log ("rangle=" + rangle);
					rangle += blockSolverData.evasiveAngleStep;
					//for (int j = 0; j < hits; j++) {
					//	if (evasiveBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == insideFrontVehicle) {
					//	}
					//}
				}
				float langle = 0f;
				while (langle >= -blockSolverData.maxEvasiveAngle) {
					int hits = ailogic.vision.CheckRotatedPositionOccupiedByVehicle (center, langle, blockSolverData.evasiveBuffer);
					if (hits > blockSolverData.evasiveBuffer.Length) {
						hits = blockSolverData.evasiveBuffer.Length;
						ailogic.Log ("Leader buffer short");
					} else if (hits == 1) {
						//This is me,
						ailogic.Log ("Found langle with no block=" + langle);
						break;
					}
					ailogic.Log ("langle=" + langle);
					langle -= blockSolverData.evasiveAngleStep;
					//for (int j = 0; j < hits; j++) {
					//	if (evasiveBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == insideFrontVehicle) {
					//	}
					//}
				}


				if (rangle <= Mathf.Abs (langle)) {
					blockSolverData.selectedAngle = rangle;
				} else {
					blockSolverData.selectedAngle = langle;
				}
				//Check that we have actually found one free of obstacles
				if (Mathf.Abs (blockSolverData.selectedAngle) <= blockSolverData.maxEvasiveAngle) {
					blockSolverData.angleSelected = true;
					Quaternion evasiveRotation = Quaternion.AngleAxis (blockSolverData.selectedAngle, Vector3.up);
					ailogic.Log ("lookahead" + steerHelper.GetLocalSteerLookAheadPoint ().ToString ("F6"));
					blockSolverData.steerLocalPoint = evasiveRotation * steerHelper.GetLocalSteerLookAheadPoint ();
					ailogic.Log ("evasive manevuer steer" + blockSolverData.steerLocalPoint.ToString ("F6"));
				} else {
					//Teleport?
					blockSolverData.angleSelected = true;
					Quaternion evasiveRotation = Quaternion.AngleAxis (blockSolverData.selectedAngle, Vector3.up);
					ailogic.Log ("lookahead" + steerHelper.GetLocalSteerLookAheadPoint ().ToString ("F6"));
					blockSolverData.steerLocalPoint = evasiveRotation * steerHelper.GetLocalSteerLookAheadPoint ();
					ailogic.Log ("evasive manevuer steer" + blockSolverData.steerLocalPoint.ToString ("F6"));

				}
				*/
				//RaycastHit hit;
				//ailogic.vision.CheckFrontForVehicle (out hit);
				//ailogic.Log ("hit="+hit.collider.GetComponentInParent<VehicleInfo> ().vehicleId);
				//Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, hit.point, Color.yellow);
				//Debug.DrawRay (hit.point, hit.normal, Color.blue);
				//Vector3 nv = Vector3.Cross (hit.normal, Vector3.up);
				//Debug.DrawRay (ailogic.vehicleInfo.frontBumper.position, nv, Color.magenta);
				//Vector3 nv2 = Vector3.Cross (ailogic.vehicleInfo.frontBumper.position-hit.point, Vector3.up);
				//Debug.DrawRay (ailogic.vehicleInfo.frontBumper.position, nv2, Color.green);
				/*Vector3 cp=blockSolverData.insideVehicleCollider.ClosestPoint (ailogic.vehicleInfo.frontBumper.position);
				//Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, cp, Color.yellow);
				//Vector3 nv3 = Vector3.Cross (ailogic.vehicleInfo.frontBumper.position-cp, Vector3.up);
				//Debug.DrawRay (ailogic.vehicleInfo.frontBumper.position, nv3, Color.red);
				Vector3 lp=blockSolverData.insideVehicleCollider.transform.InverseTransformPoint(cp);
				//Find our position on the edges of the box collider
				BoxCollider bc=blockSolverData.insideVehicleCollider.GetComponent<BoxCollider>();
				Vector3 vertex1=Vector3.zero;
				Vector3 vertex2=Vector3.zero;
				Vector3 ep=Vector3.zero;
				Vector3 en=Vector3.zero;
				Vector3 normal = Vector3.zero;
				float angle = 0f;
				ailogic.Log ( "lp=" + lp.ToString("F6")+"bc="+bc.size*0.5f);
				if (Mathf.Abs(Mathf.Abs(lp.x)- bc.size.x*0.5f)<=0.1) {
					ailogic.Log ("lp=" + lp.ToString("F6")+" is on z edge" + bc.size.ToString("F6"));
					 ep = blockSolverData.insideVehicleCollider.transform.TransformPoint(new Vector3 (lp.x, 0, bc.size.z*0.5f));
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, ep, Color.magenta);
					float angle1 = Vector3.Angle (ep - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward);
					 en = blockSolverData.insideVehicleCollider.transform.TransformPoint( new Vector3 (lp.x, 0, -bc.size.z*0.5f));
					float angle2 = Vector3.Angle (en - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, en, Color.green);

					if (angle1 <= angle2) {
						ailogic.Log ("selected top vertex");
						vertex1 = ep;

						vertex2 = en;
						angle = angle1;
					} else {
						ailogic.Log ("selected bottom vertex");
						vertex1 = en;
						vertex2 = ep;
						angle = angle2;


					}
					normal = blockSolverData.insideVehicleCollider.transform.right*Mathf.Sign(lp.x);
				} else if (Mathf.Abs(Mathf.Abs(lp.z)- bc.size.z*0.5f)<=0.1){
					ailogic.Log ("lp=" + lp.ToString("F6")+" is on x edge" + bc.size.ToString("F6"));
					 ep = blockSolverData.insideVehicleCollider.transform.TransformPoint( new Vector3 ( bc.size.x*0.5f, 0, lp.z));
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, ep, Color.magenta);
					float angle1 = Vector3.Angle (ep - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward);
					 en = blockSolverData.insideVehicleCollider.transform.TransformPoint(new Vector3 (-bc.size.x*0.5f, 0, lp.z));
					float angle2 = Vector3.Angle (en - ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, en, Color.green);

					if (angle1 <= angle2) {
						ailogic.Log ("selected right vertex");
						vertex1 = ep;
						vertex2 = en;
						angle = angle1;

					} else {
						ailogic.Log ("selected left vertex");
						vertex1 = en;
						vertex2 = ep;
						angle = angle2;

					}
					//Outward from the vehicle
					normal = blockSolverData.insideVehicleCollider.transform.forward*Mathf.Sign(lp.z);
				} else {
					return FluentBehaviourTree.BehaviourTreeStatus.Failure;
				}

				//Follow edge with less angle
				//Separate along the normal


				//Vector3 normal=Vector3.Cross(Vector3.up,vertex1-vertex2);
				Debug.DrawRay (vertex1, normal, Color.cyan);
				ailogic.Log("angel="+angle+"ms="+normal);
				Vector3 targetPoint = vertex1 + normal.normalized * 2f;



				Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, targetPoint, Color.black);
				int index = ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (targetPoint);
				Vector3 safetyTurnPoint = targetPoint + (targetPoint-ailogic.vehicleInfo.frontBumper.position).normalized * 2f;
				Debug.DrawLine ( targetPoint, safetyTurnPoint, Color.red);
				Vector3 endpoint = ailogic.routeManager.lookAtPath.GetLastPathPoint().position;
				for (int i = index; i < ailogic.routeManager.lookAtPath.interpolatedPath.Length; i++) {
					if (Vector3.Angle (ailogic.vehicleInfo.frontBumper.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint) >= 130f) {
						ailogic.Log ("Angle in path=" + Vector3.Angle (ailogic.vehicleInfo.frontBumper.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint));
						endpoint = ailogic.routeManager.lookAtPath.interpolatedPath [i].position;
						break;
					}
				}
				Debug.DrawLine (targetPoint,endpoint, Color.blue);
				GameObject go = new GameObject ("evasive path");
				go.transform.position = Vector3.zero;

				Path path = go.AddComponent<Path> ();
				ailogic.Log ("node 0="  +ailogic.vehicleInfo.frontBumper.position);
				ailogic.Log ("node 1="  +targetPoint);
				ailogic.Log ("node 2="  +safetyTurnPoint);
				ailogic.Log ("node 3="  +endpoint);
				
				path.AddNode (ailogic.vehicleInfo.frontBumper.position);
				path.AddNode (targetPoint);
				path.AddNode (safetyTurnPoint);
				path.AddNode (endpoint);
				path.BindToTerrain ();
				path.InitPathStructures ();
				blockSolverData.evasivePath = go;
				blockSolverData.cachedLookAtPath = ailogic.routeManager.lookAtPath;
				ailogic.routeManager.SetLookAtPath (path);
				go.transform.SetParent (transform);
				Debug.Break ();
				*/
				/*BoxCollider bc=blockSolverData.insideVehicleCollider.GetComponent<BoxCollider>();
				float radiusSqr = (bc.size.z) * (bc.size.z) * 0.25f + (bc.size.x) * (bc.size.x) * 0.25f;
				Vector3 lp = blockSolverData.insideVehicleCollider.transform.position-ailogic.vehicleInfo.frontBumper.position;
				float dr = lp.sqrMagnitude;
				float alpha=Mathf.Asin(Mathf.Sqrt(radiusSqr/dr));
				float angle = Vector3.SignedAngle (ailogic.vehicleInfo.frontBumper.forward, lp,Vector3.up);
				float lsqr = lp.sqrMagnitude - radiusSqr;
				float l = Mathf.Sqrt (lsqr);
				float a1 = (angle * Mathf.Deg2Rad) + alpha;
				float a2 = (angle * Mathf.Deg2Rad) - alpha;
				Debug.Log ("alpha="+alpha+"l="+l+"angle="+angle+"a1="+a1*Mathf.Rad2Deg+"a2="+a2*Mathf.Rad2Deg);
				float x = l* Mathf.Sin (a1);
				float z=l*Mathf.Cos (a1);
				float x2 = l * Mathf.Sin (a2);
				float z2 = l * Mathf.Cos (a2);
				Vector3 p1 = ailogic.vehicleInfo.frontBumper.TransformPoint (new Vector3 (x, 0.0f, z));
				Vector3 p2 = ailogic.vehicleInfo.frontBumper.TransformPoint (new Vector3 (x2, 0.0f, z2));
				GameObject go = new GameObject ("sphere");
				SphereCollider sc = go.AddComponent<SphereCollider> ();
				go.transform.position = bc.transform.position;
				sc.radius = Mathf.Sqrt (radiusSqr);
				sc.isTrigger = true;
				Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, p1,Color.black);
				Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, p2,Color.blue);

				Vector3 targetPoint=Vector3.zero;
				if (Mathf.Abs(a1)<=Mathf.Abs(a2)) {
					targetPoint=p1;
				} else {
					targetPoint=p2;
				}
				Vector3 safetyTurnPoint = targetPoint + (targetPoint-ailogic.vehicleInfo.frontBumper.position).normalized * 2f;
				Debug.DrawLine ( targetPoint, safetyTurnPoint, Color.red);
				Vector3 endpoint = ailogic.routeManager.lookAtPath.GetLastPathPoint().position;
				int index = ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (targetPoint);
				for (int i = index; i < ailogic.routeManager.lookAtPath.interpolatedPath.Length; i++) {
					if (Vector3.Angle (ailogic.vehicleInfo.frontBumper.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint) >= 130f) {
						ailogic.Log ("Angle in path=" + Vector3.Angle (ailogic.vehicleInfo.frontBumper.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint));
						endpoint = ailogic.routeManager.lookAtPath.interpolatedPath [i].position;
						break;
					}
				}
				Debug.DrawLine (targetPoint,endpoint, Color.blue);
				go = new GameObject ("evasive path");
				go.transform.position = Vector3.zero;

				Path path = go.AddComponent<Path> ();
				ailogic.Log ("node 0="  +ailogic.vehicleInfo.frontBumper.position);
				ailogic.Log ("node 1="  +targetPoint);
				ailogic.Log ("node 2="  +safetyTurnPoint);
				ailogic.Log ("node 3="  +endpoint);

				//path.AddNode (ailogic.vehicleInfo.frontBumper.position);
				path.AddNode (targetPoint);
				path.AddNode (safetyTurnPoint);
				path.AddNode (endpoint);
				path.BindToTerrain ();
				path.InitPathStructures ();
				blockSolverData.evasivePath = go;
				blockSolverData.cachedLookAtPath = ailogic.routeManager.lookAtPath;
				ailogic.routeManager.SetLookAtPath (path);
				go.transform.SetParent (transform);
				Debug.Break ();
				*/
				BoxCollider bc=evasiveManeuverData.insideVehicleCollider.GetComponent<BoxCollider>();
				float radiusSqr = (bc.size.z) * (bc.size.z)  + (bc.size.x) * (bc.size.x) ;//sum radius 
				Vector3 lp = evasiveManeuverData.insideVehicleCollider.transform.position-ailogic.vehicleInfo.carBody.position;
				float dr = lp.sqrMagnitude;
				if (radiusSqr >= dr) {
					//Reduce radius and try again
					radiusSqr=0.866f*dr; //This gets an alpha of 60 degrees
				}
				float alpha=Mathf.Asin(Mathf.Sqrt(radiusSqr/dr));
				float angle = Vector3.SignedAngle (ailogic.vehicleInfo.carBody.forward, lp,Vector3.up);
				float lsqr = lp.sqrMagnitude - radiusSqr;
				float l = Mathf.Sqrt (lsqr);
				float a1 = (angle * Mathf.Deg2Rad) + alpha;
				float a2 = (angle * Mathf.Deg2Rad) - alpha;
				//ailogic.Log ("radiusSqr="+radiusSqr+"lp="+lp.sqrMagnitude+"alpha="+alpha+"l="+l+"angle="+angle+"a1="+a1*Mathf.Rad2Deg+"a2="+a2*Mathf.Rad2Deg);
				float x = l* Mathf.Sin (a1);
				float z=l*Mathf.Cos (a1);
				float x2 = l * Mathf.Sin (a2);
				float z2 = l * Mathf.Cos (a2);
				Vector3 p1 = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (x, 0.0f, z));
				Vector3 p2 = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (x2, 0.0f, z2));

				/*GameObject go = new GameObject ("sphere");
				SphereCollider sc = go.AddComponent<SphereCollider> ();
				go.transform.position = bc.transform.position;
				sc.radius = Mathf.Sqrt (radiusSqr);
				sc.isTrigger = true;
				go.transform.SetParent (transform);
				*/
				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, p1,Color.black);
				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, p2,Color.blue);

				Vector3 targetPoint=Vector3.zero;
				if (Mathf.Abs(a1)<=Mathf.Abs(a2)) {
					targetPoint=p1;
				} else {
					targetPoint=p2;
				}
				Vector3 safetyTurnPoint = targetPoint + (targetPoint-ailogic.vehicleInfo.carBody.position).normalized * 2f;
				Debug.DrawLine ( targetPoint, safetyTurnPoint, Color.red);
				Vector3 endpoint = ailogic.routeManager.lookAtPath.GetLastPathPoint().position;
				int index = ailogic.routeManager.lookAtPath.FindClosestPointInInterpolatedPath (targetPoint);
				for (int i = index; i < ailogic.routeManager.lookAtPath.interpolatedPath.Length; i++) {
					if (Vector3.Angle (ailogic.vehicleInfo.carBody.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint) >= 130f) {
						//ailogic.Log ("Angle in path=" + Vector3.Angle (ailogic.vehicleInfo.carBody.position-safetyTurnPoint, ailogic.routeManager.lookAtPath.interpolatedPath [i].position - targetPoint));
						endpoint = ailogic.routeManager.lookAtPath.interpolatedPath [i].position;
						break;
					}
				}
				Debug.DrawLine (targetPoint,endpoint, Color.blue);
				GameObject go = new GameObject ("evasive path");
				go.transform.position = Vector3.zero;

				Path path = go.AddComponent<Path> ();
			

				//path.AddNode (ailogic.vehicleInfo.frontBumper.position);
				path.AddNode (targetPoint);
				path.AddNode (safetyTurnPoint);
				path.AddNode (endpoint);
				path.BindToTerrain ();
				path.InitPathStructures ();
				path.pathVisible = true;
				evasiveManeuverData.evasivePath = go;
				evasiveManeuverData.cachedLookAtPath = ailogic.routeManager.lookAtPath;
				ailogic.routeManager.SetLookAtPath (path);
				go.transform.SetParent (transform);


				evasiveManeuverData.startTime = Time.time;
				evasiveManeuverData.maxTimeBeforeTeleporting = path.totalPathLength / 0.5f; //TODO: what should we set..
				//TODO:. Finally, select leader again. Should be moved to behaviour tree

				ExamineFarAwayVehicles ();
				ExamineCloseVehiclesMinimum ();

				//Debug.Break ();




			}

			return FluentBehaviourTree.BehaviourTreeStatus.Success;


		}

		public FluentBehaviourTree.BehaviourTreeStatus  BlockSolverThrottle() {
			Throttle (0.5f);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}


		public bool ProceedWithCheckForLaneChange() {
			if (currentLaneChange.laneChangePending) {
				return false; //Do not check until lane change is finished
			} else {
				return true;
			}
		}


		public bool IsExecutingLaneChange() {
			if (currentLaneChange.hasSentRequest) {
				if (currentLaneChange.requestSent.isExecutingManeuver) {
					return true;
				}
			}
			return false;

		}


		public bool IsLaneChangePossible() {
			return throttleHelper.IsLaneChangePossible();
		}

		public FluentBehaviourTree.BehaviourTreeStatus StartLaneChangeManeuver() {

			if (currentLaneChange.requestSent.isExecutingManeuver==false) {
				if (brakeForcedToFinishLaneChange) {
					brakeForcedToFinishLaneChange = false;
					setBrake = false;
					//throttleHelper.recoverThrottleState ();

				}
				throttleHelper.StartLaneChangeManeuver ();
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus IsLaneChangeFinished() {
			return throttleHelper.IsLaneChangeFinished ();
		}
	

		/*public IBehaviourTreeNode GetFollowRunnerTree () {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Parallel ("MOBIL-IDM-Tracker:::follow-path", 2, 2).
				Do ("MOBIL-IDM-Tracker:::runner speed", ()=> SetRunnerSpeed()).
				Do ("MOBIL-IDM-Tracker:::steer", ()=> steerFollowRunner.ProportionalSteerController ()).
				Do ("MOBIL-IDM-Tracker:::throttle", ()=> throttleHelper.IDMThrottleControl ()).
				End ().//Parallel
				Build ();
		}

		public FluentBehaviourTree.BehaviourTreeStatus SetRunnerSpeed ()
		{
			runnerAgent.speed = ailogic.vehicleInfo.speed;
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		*/



		public FluentBehaviourTree.BehaviourTreeStatus DecideSpeed ()
		{
			//Check curvature and possibly speed limits and then set the speed goal of the vehicle at this update
			//Can use 
			//maxCurvature: too strict
			//averageCurvature
			//curvature at current Point
			//if (throttleHelper.throttleMode == ThrottleMode.ApplyBehaviour || throttleHelper.throttleMode == ThrottleMode.AdaptToCurvature) {
			float speedGoal=0f;
			if (throttleHelper.throttleMode == ThrottleMode.StopAtPoint || throttleHelper.throttleMode == ThrottleMode.SpeedAtPoint) {
				speedGoal = throttleHelper.GetSpeedForSpeedAtPoint ();
				//ailogic.Log (31, "GetSpeedForSpeedAtPoint=" + speedGoal);

			} else {
				speedGoal = throttleHelper.freeSpeed;
				//ailogic.Log (31, "freeSpeed=" + speedGoal);
			}
			float curvature = steerHelper.steerLookAheadPoint.curvature;
			//float curvature = ailogic.routeManager.currentPath.averageCurvature;
			if (curvature >= 0) {
				float speed = throttleHelper.ComputeMaxCorneringSpeed (curvature);
				//ailogic.Log (31, "curvature speed=" + speed);
				if (ailogic.currentLane != null) {
					if (speedGoal > speed) {
						//ailogic.Log ("Changing freeSpeed from " + throttleHelper.freeSpeed + " to " + speed + "because of averageCurvature=" + curvature);
						//ailogic.Log (31 ,"Changing freeSpeed from " + throttleHelper.freeSpeed + " to " + speed + "because of curvature at point=" + curvature);
						speedGoal=speed;
					} else {
						if (throttleHelper.throttleMode == ThrottleMode.ApplyBehaviour || throttleHelper.throttleMode == ThrottleMode.AdaptToCurvature) {
							if (ailogic.currentLane.speed < speed) {
								//Recover lane speed limit
								speedGoal= ailogic.currentLane.speed;
						//		ailogic.Log (31, "recover speed limit");
							}
						}
					}
				}
			}

			//ailogic.Log (31, "speedGoal="+speedGoal);
			//Try to adapt speed to lane change
			if (currentLaneChange.laneChangePending) {
				if (currentLaneChange.requestSent != null) {
					//ailogic.Log (31, "speedGoal="+speedGoal);
					if (currentLaneChange.requestSent.isExecutingManeuver == false) {
						//Adapt speed only if the maneuver has not begun yet
						speedGoal = AdaptSpeedToLaneChange (speedGoal);
					}
				}
			}


			throttleHelper.SetSpeedLimit (speedGoal);
			
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}


		public float AdaptSpeedToLaneChange(float speedGoal) {
			if (currentLaneChange.requestSent.followerPosition == LaneChangeRequest.LaneChangeFollowerPosition.InFront ) {
				//Reduce a little speed to help finish change
				if (forceSpeedToFinishLaneChange) {

					speedGoal = speedGoal * 0.1f;
					//	ailogic.Log (31, "Forcing brake to finish lane change=" + speedGoal);
				} else {
					speedGoal = speedGoal * 0.7f;
					//	ailogic.Log (31, " LaneChangeRequest.LaneChangeFollowerPosition.InFront=" + speedGoal);
				}
			} else if (currentLaneChange.requestSent.followerPosition == LaneChangeRequest.LaneChangeFollowerPosition.Behind) {
				if (forceSpeedToFinishLaneChange) {
					speedGoal = speedGoal * 2f;
					//	ailogic.Log (31, "Forcing acceleration to finish lane change=" + speedGoal);
				} else {
					speedGoal = speedGoal * 1.2f;
					//	ailogic.Log (31, " LaneChangeRequest.LaneChangeFollowerPosition.Behind=" + speedGoal);
				}
			} else if (currentLaneChange.requestSent.followerPosition == LaneChangeRequest.LaneChangeFollowerPosition.Parallel) {

				if (forceSpeedToFinishLaneChange) {
					if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (currentLaneChange.requestSent.follower.frontBumper.position).z > 0) {
						speedGoal = speedGoal * 0.1f;
						//		ailogic.Log (31, "Forcing brake to finish lane change=" + speedGoal);
					} else {
						speedGoal = speedGoal * 2f;
						//		ailogic.Log ("Forcing acceleration to finish lane change=" + speedGoal);
					}
				} else {
					speedGoal = speedGoal * 0.7f;
					//	ailogic.Log (31, " currentLaneChange.requestSent.followerPosition == LaneChangeRequest.LaneChangeFollowerPosition.Parallel=" + speedGoal);
				}
			}
			//ailogic.Log (31, "Forcing a/b to finish lane change=" + speedGoal);
			if (speedGoal < minLaneChangeSpeed) {
				//To avoid blocks
				speedGoal = minLaneChangeSpeed;
			}
			return speedGoal;
			
		}

	

	
		public FluentBehaviourTree.BehaviourTreeStatus SetStartingPath() {
			if (ailogic.routeManager != null) {
				if (ailogic.routeManager.StartPath() != null) {
					ailogic.routeManager.SetLookAtPath (ailogic.routeManager.StartPath ());
					//steerHelper.SetLookAtPath (ailogic.routeManager.StartPath());
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Failure;
		}

		bool HasFinishedTurning() {
			
			if (currentLaneChange.laneChangeSequence.hasFinishedTurning) {
				return true;
			} else {
				if (ailogic.steeringWheelRotation < 1f && ailogic.steeringWheelRotation > -1f) {
					currentLaneChange.laneChangeSequence.hasFinishedTurning = true;
					return true;
				} else {
					return false;
				}
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus GetPathPoint() {
			
			if (HasPath () == false) {
				return FluentBehaviourTree.BehaviourTreeStatus.Failure;
			}
			//1) Find the point (index in interpolated path) in the path closest to this vehicle
		

			return steerHelper.GetPathPoint();
			//Debug.Log("rearAx="+rearAx
		}
		public FluentBehaviourTree.BehaviourTreeStatus GetNextPathPoint() {
			

			return steerHelper.GetNextPathPoint ();
		
		}

		/*public FluentBehaviourTree.BehaviourTreeStatus CheckForLaneChangeInNextPath() {
			//ailogic.routeManager.SetCurrentPath (steerHelper.nextPath);

			ailogic.CheckForLaneChangeInPath (ailogic.routeManager.lookAtPath);
		
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		*/
		public FluentBehaviourTree.BehaviourTreeStatus CheckForLaneChangeInCurrentPath ()
		{

			LaneChangeQueueEntry entry;
			if (ailogic.CheckForLaneChangeInPath (ailogic.routeManager.lookAtPath, out entry)) {
				//ailogic.Log ("LaneChange for " + ailogic.routeManager.lookAtPath.pathId + "cur="+ailogic.currentLane.sumoId+"entry="+entry.startLane.sumoId, showLog);
				if (ailogic.currentLane == entry.startLane) {
					
					CheckAndComputeLaneChanges (entry);

				}


			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		bool HasPath ()
		{

			if (ailogic.routeManager.trackedPath != null) {
				return true;
			} else {
				return false;
			}
		}
		bool IsBackBumperOnLane() {
			
			if (currentLaneChange.laneChangeSequence.isBackBumperOnLane) {
				
				return true;
			} else {
				if (ailogic.CheckCurrentLane () == currentLaneChange.laneChangeSequence.startLane) {
					currentLaneChange.laneChangeSequence.isBackBumperOnLane = true;

					return true;
				}


				return false;
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus CheckArrivingEndOfLane() {
			//Check if we are close to the end of lane and decide to brake
			int currentSection = ailogic.currentLane.FindPointInSections (ailogic.vehicleInfo.frontBumper);
			if (ailogic.currentLane.sections.Count > 5) {



				if (currentSection >= Mathf.FloorToInt((ailogic.currentLane.sections.Count*0.8f))) {
					if (currentLaneChange.requestSent.isExecutingManeuver == false) {
						//We have not started yet, brake
						//ailogic.Log (currentSection + ". End of lane " +ailogic.currentLane.sumoId);

						forceSpeedToFinishLaneChange = true;
					} else {
						//ailogic.Log ( "End of lane  no force "+ailogic.currentLane.sumoId);
						forceSpeedToFinishLaneChange = false;
					}
				}
			}

			//Brake at end of lane to finish lane change
			if (currentSection == ailogic.currentLane.sections.Count - 1) {
				if (currentLaneChange.requestSent.isExecutingManeuver == false) {
					//if (brakeForcedToFinishLaneChange == false) {
						//throttleHelper.SaveThrottleState ();
						//throttleHelper.SetBrake ();
						brakeForcedToFinishLaneChange = true;
						setBrake = true;
					//}
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}

		public FluentBehaviourTree.BehaviourTreeStatus StartOrDelayLaneChange() {
			if (currentLaneChange.hasSentRequest && currentLaneChange.requestSent.isExecutingManeuver) {
				//ailogic.Log (55, "from "+currentLaneChange.laneChangeSequence.startLane.sumoId+ "return success at StartOrDelayLaneChange" );

				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			//First check if the path is long enough

			//float timeToReachEnd = ailogic.routeManager.lookAtPath.totalPathLength / ailogic.vehicleInfo.speed;
			float timeToReachEnd = currentLaneChange.laneChangeSequence.startLane.paths[0].totalPathLength / ailogic.vehicleInfo.speed;
			if (timeToReachEnd >  minTimeToChangeLane) {
				
				//Delay
				int limit = Mathf.FloorToInt (ailogic.currentLane.sections.Count * 0.5f);
				int currentSection = ailogic.currentLane.FindPointInSections (ailogic.vehicleInfo.carBody);
				if (currentSection < 0) {
					//Recheck lane and try again
					ailogic.CheckCurrentLane();
					currentSection = ailogic.currentLane.FindPointInSections (ailogic.vehicleInfo.carBody);


				}
				if (currentLaneChange.delayStart == false) { 
					
					currentLaneChange.delayStart = true;
						
					if (currentSection > limit) {
						currentLaneChange.sectionToStart = limit;
					} else {
						//Select random section
						currentLaneChange.sectionToStart = SimulationManager.Instance.GetRNG ().Next (currentLaneChange.sectionToStart, limit);
					}

				}
				if (currentSection>= currentLaneChange.sectionToStart) {
					//Start right now

					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				} else {
					//ailogic.Log (41, "from " + currentLaneChange.laneChangeSequence.startLane.sumoId + "return success at StartOrDelayLaneChange return running" + currentSection);
					return FluentBehaviourTree.BehaviourTreeStatus.Running;
				}

			} else {
				//Start right now
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
			
				
		}
		bool LaneChangePending ()
		{
			
			return currentLaneChange.laneChangePending;
		}

		public void LaneChangeCompleted(LaneChangeQueueEntry origin) {
			throttleHelper.CancelLaneChangeRequest (currentLaneChange.requestSent);
			//laneChangesQueue.Remove(origin);
			currentLaneChange.Clear();
			//laneChangePending = false;
			//requestSent = null;

			ailogic.LaneChangeCompleted (origin);
			ailogic.vehicleInfo.SetDriving ();
		}

		public void CancelLaneChange(LaneChangeQueueEntry origin) {
			if (currentLaneChange.hasSentRequest) {
				if (currentLaneChange.laneChangeSequence.origin == origin) {
					throttleHelper.CancelLaneChangeRequest (currentLaneChange.requestSent);
				}
			}
			currentLaneChange.Clear ();
			ailogic.LaneChangeCancelled (origin);
			ailogic.vehicleInfo.SetDriving ();
		}

		public FluentBehaviourTree.BehaviourTreeStatus FinishCurrentChange ()
		{
			throttleHelper.FinishLaneChangeManeuver ();
			throttleHelper.CancelLaneChangeRequest (currentLaneChange.requestSent);
			if (brakeForcedToFinishLaneChange) {
				//throttleHelper.recoverThrottleState ();
				brakeForcedToFinishLaneChange = false;
				setBrake = false;
			}
		
				//Go back to default behaviour
			LaneChangeCompleted(currentLaneChange.laneChangeSequence.origin);

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		//Enforce speed limit.  Should be changed with a trigger when a new lane or road is entered
		public FluentBehaviourTree.BehaviourTreeStatus SetSpeedLimit ()
		{


			if (ailogic.currentLane != null) {
				throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);

				return FluentBehaviourTree.BehaviourTreeStatus.Success;

			}
			return  FluentBehaviourTree.BehaviourTreeStatus.Failure;

		}
		public FluentBehaviourTree.BehaviourTreeStatus SetLaneChangeRequest ()
		{
			//The lane must be in our road, otherwise...

			if (currentLaneChange.hasSentRequest) {
				if (throttleHelper.pendingRequest.targetLane == currentLaneChange.requestSent.targetLane) {
					//ailogic.Log (55, "SetLaneChangeRequest 1");
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				} 
			} else {
				currentLaneChange.requestSent = new LaneChangeRequest (currentLaneChange.laneChangeSequence.startLane, currentLaneChange.laneChangeSequence.targetLane);
				throttleHelper.SetLaneChangeRequest (currentLaneChange.requestSent);
				currentLaneChange.hasSentRequest = true;
				return FluentBehaviourTree.BehaviourTreeStatus.Success;
			}



			return FluentBehaviourTree.BehaviourTreeStatus.Failure;

		

				

		}
		void HandleEnterVehicleTrigger (Collider other)
		{
			

			if (other.CompareTag ("Lane")) {
				
				//Check pending
				if (LaneChangePending()) {
					
					VenerisRoad road = other.GetComponentInParent<VenerisRoad> ();
					if (road != currentLaneChange.laneChangeSequence.startRoad) {
						//ailogic.Log (11, "Cancelling lane change for " + currentLaneChangeSequence.targetPathId);
						CancelLaneChange (currentLaneChange.laneChangeSequence.origin);
					}
				}


			
			}



		}

		protected void CheckAndComputeLaneChanges (LaneChangeQueueEntry entry)
		{
			
			//ailogic.Log ("CheckAndComputeLaneChanges " + entry.startLane.sumoId, showLog);
			currentLaneChange.laneChangePending = true;
			ComputeLaneChangeSequence (entry);






		}

		protected void ComputeLaneChangeSequence( LaneChangeQueueEntry origin) {
			currentLaneChange.laneChangeSequence.Initialize (origin.startLane, origin.targetLane,origin);

		}
		
		private GameObject GetChildWithName(GameObject obj, string name) {
			Transform trans = obj.transform;
			Transform childTrans = trans. Find(name);
			if (childTrans != null) {
				return childTrans.gameObject;
			} else {
				return null;
			}
		}
		
		public void FindObjectwithTag(string _tag, Transform parent)
		{
			breakLightGo.Clear();
			GetChildObject(parent, _tag);
		}
		public void GetChildObject(Transform parent, string _tag)
		{
			for (int i = 0; i < parent.childCount; i++)
			{
				Transform child = parent.GetChild(i);
				if (child.tag == _tag)
				{
					breakLightGo.Add(child.gameObject);
				}
				if (child.childCount > 0)
				{
					GetChildObject(child, _tag);
				}
			}
		}

		/*
		protected void ComputeLaneChangeSequence (long startPid, long targetId, VenerisLane startLane, LaneChangeQueueEntry origin)
		{

			//The lane must be in our road, otherwise...

			//ailogic.Log ("ComputeLaneChangeSequence");
			//foreach (Path p in ailogic.currentRoad.GetComponentsInChildren<Path>()) {
			for (int i = 0; i < ailogic.currentRoad.lanes.Length; i++) {
				Path p = ailogic.currentRoad.lanes [i].paths [0];
				if (p.pathId == targetId) {
					VenerisLane targetLane =ailogic.currentRoad.lanes [i];
					currentLaneChange.laneChangeSequence.pathList.Clear ();
					currentLaneChange.laneChangeSequence.Initialize (startPid,targetId, startLane, origin);
					if (targetLane.laneId != (ailogic.currentLane.laneId + 1) && targetLane.laneId != (ailogic.currentLane.laneId - 1)) {
						//Separated by more than one lane
						 
						//ailogic.Log ("separated by multiple lanes "+targetLane.sumoId+" current "+ailogic.currentLane.sumoId);
						//ailogic.Log ("lane.laneId "+targetLane.laneId+" +1 "+(ailogic.currentLane.laneId ));
						//ailogic.Log ("Mark change of lane from " + startPid + " to " + targetId + "with startLane="+startLane.laneId+" + targetLane="+targetLane.laneId);

						if (targetLane.laneId > ailogic.currentLane.laneId) {
							for (long j = ailogic.currentLane.laneId+1; j <= targetLane.laneId; j++) {
								//What if there are more than one path per lane...?
								currentLaneChange.laneChangeSequence.AddPath (ailogic.currentRoad.lanes [j].paths[0]);
								
							}
						} else {
							for (long k = ailogic.currentLane.laneId-1; k >= targetLane.laneId; k--) {
								//What if there are more than one path per lane...?
								currentLaneChange.laneChangeSequence.AddPath (ailogic.currentRoad.lanes [k].paths[0]);

							}
						}
						return;
					} else {
						currentLaneChange.laneChangeSequence.AddPath(p);
						return;
					}

				}
			}

			//We should have found the lane
			ailogic.LogError("Lane change not in our road. startPid= " + startPid + "targetId="+targetId+"road="+startLane.GetComponentInParent<VenerisRoad>().sumoId);
			return;

		}
		*/
		/*protected void HandleDestroyTrigger (int vid)
		{


			if (leadingVehicleSelector.leaderInfo.leader != null) {
				if (leadingVehicleSelector.leaderInfo.leader.vehicleId == vid) {
					leadingVehicleSelector.UnsetLeadingVehicle ();
				}
			}
			if (blockSolverData.insideFrontVehicle != null) {
				if (blockSolverData.insideFrontVehicle.vehicleId == vid) {
					if (blockSolverData.executeEvasiveManeuver == true) {
						EndEvasiveManeuver ();
					}
				}
			}
			if (closeLeader.leader != null) {
				if (closeLeader.leader.vehicleId == vid) {
					closeLeader.Clear ();
				}
			}
			if (farAwayLeader.leader != null) {
				if (farAwayLeader.leader.vehicleId == vid) {
					farAwayLeader.leader = null;
					UnsetLeadingVehicle ();
				}
			}

		}*/

	}
}
