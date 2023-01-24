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
	public class Stop : IntersectionBehaviour
	{
		
		public List<Transform> priorityCheckPositions=null;



		public override void ActivateBehaviour ()
		{
			base.ActivateBehaviour ();
			//Recover state
			if (intersectionStopLineReached == false) {
				//Make sure we are not already on the intersection
				if (ailogic.currentIntersection != intersection) {
					float distanceToStopLine; 
					float desiredSpeed = 0.5f;
					if (ComputeDistanceToStopLine ( out distanceToStopLine)) {
					//if (ComputeDistanceToStopLine (stopLinePosition, plannedPath, out distanceToStopLine)) {

					} else {
						//TODO: cannot find a path to the stop line, just use here euclidean distance. Solve this
						ailogic.Log("cannot find a path to the stop line " + stopLinePosition.name + " of " + stopLinePosition.parent.name);
						distanceToStopLine = (ailogic.vehicleInfo.carBody.position - stopLinePosition.position).magnitude;


						//throw new UnityException ();
					}
					throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, desiredSpeed, ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled,stopLineCollider);
					throttleHelper.SetStopAtPoint (throttleGoal); 
					throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);

				} else {
					//Assume we have already crossed the stopline
					intersectionStopLineReached = true;
				}
			} else if (internalLaneEndReached == false) {
				SetNextPathSpeedLimit ();
				SetCrossingIntersectionState();
			}
			CheckColliders ();

		}
		public override void DeactivateBehaviour ()
		{
			base.DeactivateBehaviour ();

		}
		public IBehaviourTreeNode PrepareToStopTree() {
			

			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Parallel("do prepare to stop",3,1).
					
					Do("Anti-blocking timer", ()=>CheckMaximumTimeAtIntersection()).
					ExecuteUntilSuccessNTimes("check priority positions once",1). //Only run succesfully once the following sequence
						Sequence("check priority positions").
							Condition(" has reached intersection stop and has stopped?",()=>{return (throttleHelper.HasStoppedAtGoalPoint() && intersectionStopLineReached==true);}).
							Do("Wait if jammed", ()=>WaitIfJammed()).
							Do("Check traffic light again", ()=>ReCheckTrafficLight()).
							Do("start no-block intersection timer",()=>StartIntersectionTimer()).
							Do("wait until clear",()=>WaitUntilCleared()).
							Do("change to adapt to curvature",()=>SetAdaptToCurvature()).
							Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
							Do("keep checking for vehicles with priority in the intersection",()=>SetDrivingCheckPositions(priorityCheckPositions)).
							Do("set current action in vehicle info",()=>SetCrossingIntersectionState()).
						End().
					End().
					Sequence("drive through intersection").
						Condition("reached end of intersection?",()=>{return internalLaneEndReached==true;}).
						Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
						//Do("Log behaviour",t=>{ailogic.Log(11,"drive through intersection " +intersection.name); return FluentBehaviourTree.BehaviourTreeStatus.Success;}).
						Do("Apply behaviour",()=>SetApplyBehaviour()).
						Do("stop checking for vehicles with priority in the intersection",()=>UnsetDrivingCheckPositions()).
						Do ("change to default-behaviour", () => SetDefault ()).
					End().
					Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end
				End (). //parallel
			Build ();
		}
		public override void Prepare ()
		{
			base.Prepare ();
			action = IntersectionAction.PrepareToStop;
			behaviourName="Stop at intersection "+intersection.name;

			mainBehaviour = PrepareToStopTree ();


			//Call at the end to let traffic light tracker work

			SetApproachActionAndPriority();
		}
		public FluentBehaviourTree.BehaviourTreeStatus CheckMaximumTimeAtIntersection() {
			if (intersectionTimerStart >=0f) {

				if ((Time.time - intersectionTimerStart) > maxTimeAtIntersection) {
					//Teleport
					ailogic.Log ("Stop::maxTimeAtIntersection " + Time.time + "intersectionTimerStart=" + intersectionTimerStart + "diff=" + (Time.time - intersectionTimerStart));
					//Debug.Break ();

					//if (!ailogic.Teleport ("maxTimeAtIntersection "+intersection.sumoJunctionId,ailogic.routeManager.lookAtPath.pathId, out nextPath)) {
					ailogic.RemoveAndReinsert ("Stop::maxTimeAtIntersection=" + Time.time + ":intersectionTimerStart=" + intersectionTimerStart + ":Intersection="+intersection.sumoJunctionId);
					//}
				}
			}
	
			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}
	
	

		public FluentBehaviourTree.BehaviourTreeStatus WaitUntilCleared() {
			
			ailogic.vehicleInfo.SetWaitingForClearance ();
			//Debug.Log ("Enter WaitUntilCleared");
			//ailogic.Log(145, "Enter WaitUntilCleared");
			//ailogic.Log(40, "Enter WaitUntilCleared");
			foreach (Transform t in priorityCheckPositions) {
				//Debug.Log ("Checking "+t.position);
				//RaycastHit[] hits = ailogic.vision.CheckForEntitiesWithTag (t.position, "CarCollider",Vector3.Distance(transform.position,t.position)*1.2f);
				RaycastHit[] hits = ailogic.vision.CheckPositionForVehicles(t.position,Vector3.Distance(transform.position,t.position)*1.2f);
				if (hits != null) {
					//Debug.Log ("hits left:"+hits.Length);
					//Debug.Log ("There is a car waiting: "+hits[0].collider.tag + " name="+hits[0].transform.name);
					//ailogic.Log(145,"There is a car waiting: "+hits[0].collider.tag + " name="+hits[0].transform.name);
					//ailogic.Log(40,"There is a car waiting: "+hits[0].collider.tag + " name="+hits[0].transform.name);
					bool cleared = CheckVehiclesWaitingAtIntersection (hits);
					//ailogic.Log(145,"cleared=" + cleared);
					if (cleared==false ) {

						return FluentBehaviourTree.BehaviourTreeStatus.Running;
					}

				}
			}



			//ailogic.Log (40,"Exit WaitUntilCleared with success");
			//ailogic.Log (129,"Exit WaitUntilCleared with success");
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public bool CheckVehiclesWaitingAtIntersection(RaycastHit[] hits) {
			//Debug.Log (ailogic.vehicleInfo.vehicleId +"Checking vehicles at the intersection");
			int cleared = 0;
			int waiting = 0;
			for (int i = 0; i < hits.Length; i++) {
			//foreach (RaycastHit h in hits) {
				//Debug.Log (ailogic.vehicleInfo.vehicleId + "hit in clearance " + h.transform.name);
				Stop[] istops= hits[i].transform.GetComponentsInChildren<Stop>();
				//Debug.Log (ailogic.vehicleInfo.vehicleId + "intersection stops " +istops.Length);
				foreach (Stop s in istops) {
					if (s.intersection == this.intersection) {
						//Debug.Log (ailogic.vehicleInfo.vehicleId + "intersection coincident " +this.intersection.name);
						AILogic ai=hits[i].transform.root.GetComponentInChildren<AILogic>();
						if (ai.vehicleInfo.currentActionState == VehicleInfo.VehicleActionState.WaitingForClearance) {
							//Get me the list of vehicles it is waiting for

							bool waitForMe = s.CheckIfVehiclesAtPriorityPositions (transform);
							//Debug.Log(ailogic.vehicleInfo.vehicleId+" waitForMe ="+waitForMe); 

							if (waitForMe) {
								waiting++;
								if (ApplyRightBeforeLeftRule ( hits[i].transform)) {
									++cleared;
								}
							}
						}
					}
				}

			}
			if (waiting == cleared) {
				return true;
			} else {
				return false;
			}

		}
		public bool CheckIfVehiclesAtPriorityPositions(Transform other) {

			VehicleVisionPerceptionModel vision = ailogic.vision;
			foreach (Transform t in priorityCheckPositions) {
				ailogic.Log (129,"Checking "+t.position);
				ailogic.Log (40,"Checking "+t.position);
				//TODO: change to checkPositionForVehicle
				RaycastHit[] hits=vision.CheckForEntitiesWithTag (t.position, "CarCollider",Vector3.Distance(transform.position,t.position)*1.2f);

				if (hits != null) {
					foreach (RaycastHit h in hits) {
						//Debug.Log (ailogic.vehicleInfo.vehicleId + "hit in CheckIfVehiclesAtPriorityPositions" + h.transform.root.name + "other=" + other.root.name);
						if (h.transform.root == other.root) {
							return true;
						}
					}
				}
			}
			return false;

		}


		public bool ApplyRightBeforeLeftRule( Transform other) {


			//Right-before-left rule
			Vector3 relative = ailogic.vehicleInfo.carBody.transform.InverseTransformPoint (other.position);
			if (relative.x > 0) {
				//It is on my right, I cannot go
				return false;
			} else {
				return true;
			}

		}


	}
}
