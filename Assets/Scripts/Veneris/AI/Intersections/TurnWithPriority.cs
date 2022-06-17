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
	public class TurnWithPriority : IntersectionBehaviour
	{
		


		public override void ActivateBehaviour ()
		{
			base.ActivateBehaviour ();
			//Recover state
			if (intersectionStopLineReached == false) {
				//if (intersectionStopLineReached == false) {
				if (ailogic.currentIntersection != intersection) {
					float distanceToStopLine; 
					float desiredSpeed;
					if (ComputeDistanceToStopLine ( out distanceToStopLine)) {
					//if (ComputeDistanceToStopLine (stopLinePosition, plannedPath, out distanceToStopLine)) {
						desiredSpeed = Mathf.Clamp (1.4f * throttleHelper.ComputeMaxCorneringSpeed (internalPath.maxCurvature), 0.5f, ailogic.currentLane.speed);


					} else {
						ailogic.Log("cannot find a path to the stop line " + stopLinePosition.name + " of " + stopLinePosition.parent.name);
						distanceToStopLine = (ailogic.vehicleInfo.carBody.position - stopLinePosition.position).magnitude;
						desiredSpeed = 0.5f;
			

						//throw new UnityException ();
					}
					throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position,desiredSpeed, ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled, stopLineCollider);
					throttleHelper.SetSpeedAtPoint (throttleGoal);
					throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);
				} else {
					//Assume we have already crossed the stopline
					intersectionStopLineReached = true;
				}
			} else if (internalLaneEndReached == false) {
				SetAdaptToCurvature ();
				SetNextPathSpeedLimit ();
				SetCrossingIntersectionState();
			}
			CheckColliders ();
		}
		public override void DeactivateBehaviour ()
		{
			base.DeactivateBehaviour ();

		}

		public override void Prepare ()
		{
			base.Prepare ();
			
			float distanceToStopLine; 
			/*if (ComputeDistanceToStopLine (stopLinePosition, plannedPath, out distanceToStopLine)) {
				throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, Mathf.Clamp (1.4f * throttleHelper.ComputeMaxCorneringSpeed (internalPath.maxCurvature), 0.5f, ailogic.currentLane.speed), ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled);

			} else {

				throw new UnityException ();
			}*/
			action = IntersectionAction.PrepareToTurnWithPriority;
			behaviourName="TurnWithPriority at  "+intersection.name;
			mainBehaviour = PrepareToTurnWithPriorityTree ();
			//throttleHelper.SetSpeedAtPoint(throttleGoal);
			//throttleHelper.SetSpeedLimit(ailogic.currentLane.speed);

			//Call at the end to let traffic light tracker work
			//base.Prepare ();
			SetApproachActionAndPriority();
		}

		public IBehaviourTreeNode PrepareToTurnWithPriorityTree() {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Parallel("do turn with priority",2,1). //Check start and stop of intersections while driving

					Do("Anti-blocking timer", ()=>CheckMaximumTimeAtIntersection()).
					ExecuteUntilSuccessNTimes("check reached stop line once",1). //Only run succesfully once the following sequence
						Sequence("checked reached stop line").
							Condition(" intersection stop line reached?",()=>{return intersectionStopLineReached==true;}).
							Do("Wait if jammed", ()=>WaitIfJammed()).
							Do("Check traffic light again", ()=>ReCheckTrafficLight()).
							Do("start no-block intersection timer",()=>StartIntersectionTimer()).
							Do("change to adapt to curvature",()=>SetAdaptToCurvature()).
							Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
							Do("set current action in vehicle info",()=>SetCrossingIntersectionState()).
						End().
					End().
					Sequence("check-reached-end-of intersection").
						Condition("reached end?",()=>{return internalLaneEndReached==true;}).
							Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
							//Do("Log behaviour",()=>{ailogic.Log(11,"check-reached-end-of intersection pttwp " +intersection.name); return FluentBehaviourTree.BehaviourTreeStatus.Success;}).
							Do("Apply behaviour",()=>SetApplyBehaviour()).
							Do ("change to default-behaviour", ()=> SetDefault ()).
					End().
				//We may do something else while turning at the intersection
					Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end
				End(). //Parallel
			Build ();
		}
		public FluentBehaviourTree.BehaviourTreeStatus CheckMaximumTimeAtIntersection() {
			if (intersectionTimerStart >=0f) {

				if ((Time.time - intersectionTimerStart) > maxTimeAtIntersection) {
					//Teleport
					ailogic.Log ("TurnWithPriority::maxTimeAtIntersection " + Time.time + "intersectionTimerStart=" + intersectionTimerStart + "diff=" + (Time.time - intersectionTimerStart));
					//Debug.Break ();

					//if (!ailogic.Teleport ("maxTimeAtIntersection "+intersection.sumoJunctionId,ailogic.routeManager.lookAtPath.pathId, out nextPath)) {
					ailogic.RemoveAndReinsert ("TurnWithPriority::maxTimeAtIntersection=" + Time.time + ":intersectionTimerStart=" + intersectionTimerStart + ":Intersection="+intersection.sumoJunctionId);
					//}
				}
			}

			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}

	

	}
}
