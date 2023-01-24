/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluentBehaviourTree;

namespace Veneris
{
	public class FollowPathWithIDM : AIBehaviour
	{

		public ProportionalPathTrackerActionBTHelper steerHelper = null;
		public IDMInteractionActionBTHelper throttleHelper = null;

		//public Path currentPath = null;


		public AILogic ailogic = null;

		// Use this for initialization
		void Awake ()
		{
			if (steerHelper == null) {
				steerHelper = GetComponent<ProportionalPathTrackerActionBTHelper> ();
			}
			if (throttleHelper == null) {
				throttleHelper = GetComponent<IDMInteractionActionBTHelper> ();
			}


	
		}
		// Use this for initialization
		void Start ()
		{

		}

		public override void Prepare ()
		{
			base.Prepare ();
			mainBehaviour = GetTree ();
			Debug.Log ("FollowPathWithIDM created");
			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();


			}
			ailogic.vehicleTrigger.AddEnterListener (HandleEnterVehicleTrigger);
		
		}

		public IBehaviourTreeNode GetTree ()
		{
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();

			return builder.Sequence ("start-moving").
								ExecuteUntilSuccessNTimes("set speed limit once",1). //Only run succesfully once the following sequence
								Do("Set speed limit",()=>SetSpeedLimit()).
								End().
								Condition ("Follow-path-with-IDM::has-path", () => HasPath ()).
								Parallel ("Follow-path-with-IDM::follow-path", 2, 2).

									Do ("Follow-path-with-IDM::steer", () => steerHelper.ProportionalSteerController ()).
									Do ("Follow-path-with-IDM::throttle", 	() => throttleHelper.IDMThrottleControl ()).
								End ().
							End ().
					Build ();
		}



		bool HasPath ()
		{
			
			if (ailogic.routeManager.trackedPath != null) {
				return true;
			} else {
				Debug.Log (ailogic.vehicleInfo.vehicleId + " has no path");
				return false;
			}
		}
		//Enforce speed limit.  Should be changed with a trigger when a new lane or road is entered
		public FluentBehaviourTree.BehaviourTreeStatus SetSpeedLimit() {


			if (ailogic.currentLane != null) {
				throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);
			//	Debug.Log ("Setting speed limit at " + ailogic.currentLane.speed);
				return FluentBehaviourTree.BehaviourTreeStatus.Success;

			}
			return  FluentBehaviourTree.BehaviourTreeStatus.Failure;

		}
		
		void HandleEnterVehicleTrigger (Collider other)
		{
			if (other.CompareTag ("Intersection")) {
//				Debug.Log (ailogic.vehicleInfo.vehicleId+"Follow Path: intersection reached, getting PathConnector");
				ailogic.routeManager.SetNextPathFromConnector (other.GetComponentInChildren<PathConnector> ());
			}
			if (other.CompareTag ("ConnectorTrigger")) {
//				Debug.Log (ailogic.vehicleInfo.vehicleId+"-Follow Path: ConnectorTrigger reached, getting PathConnector "+other.name);
				if (ailogic.routeManager.SetNextPathFromConnector (other.GetComponentInChildren<PathConnector> ())) {
					Debug.Log (ailogic.vehicleInfo.vehicleId + "-Setting new current path=" + ailogic.routeManager.trackedPath.pathId);
				} 
			}
			if (other.CompareTag ("InternalStopTrigger")) {
//				Debug.Log (ailogic.vehicleInfo.vehicleId+"-Follow Path: InternalStopTrigger reached, getting PathConnector "+other.name);
				if (ailogic.routeManager.SetNextPathFromConnector (other.GetComponentInChildren<PathConnector> ())) {
					Debug.Log (ailogic.vehicleInfo.vehicleId + "-Setting new current path=" + ailogic.routeManager.trackedPath.pathId);
				}
			}
			if (other.CompareTag ("Lane")) {
				//Check for lane changes
				Debug.Log (ailogic.vehicleInfo.vehicleId+"-Follow Path: Checking lane changes at "+ other.transform.parent.name);

				//Enforce speed limit here...
				//SetSpeedLimit();
			}
	

		}




	}
}
