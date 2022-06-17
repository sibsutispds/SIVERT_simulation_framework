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
	[System.Serializable]
	public class TrafficLightTracker
	{
		[System.Serializable]
		public class TrafficLightStateTrack
		{
			public TrafficLight trafficLight = null;
			public int trafficLightIndex = -1;
			public TrafficLight.TrafficLightState trafficLightState;
			public TrafficLight.TrafficLightState lastSeenState;
			public float distanceToBrakeAtAmber = 20f;
			public bool decidedToStopAtAmber = false;
			public bool waitingAtStopLine = false;
			public enum DecidedAction {Undecided, StopAtRedLight, AmberGoOn,  GreenGoOn};
			public DecidedAction currentAction;
			public TrafficLightStateTrack (TrafficLight t, int index)
			{
				trafficLight = t;
				trafficLightIndex = index;
				trafficLightState = TrafficLight.TrafficLightState.Undefined;
				lastSeenState = TrafficLight.TrafficLightState.Undefined;
				waitingAtStopLine = false;
				decidedToStopAtAmber = false;
				currentAction= DecidedAction.Undecided;
			}

			public void UpdateState ()
			{

				trafficLightState = trafficLight.GetState (trafficLightIndex);
			}

			public bool HasChangedState ()
			{
				if (lastSeenState != trafficLightState) {
					return true;
				} else {
					return false;
				}
			}

			public void SetLastSeenState (TrafficLight.TrafficLightState s)
			{
				lastSeenState = s;
			}
		}

		public TrafficLightStateTrack tlTrack = null;
		public float safeUrbanSpeed = 13.89f;
		public IntersectionBehaviour intersectionBehaviour = null;
		public IBehaviourTreeNode mainBehaviour = null;

		public TrafficLightTracker (IntersectionBehaviour b)
		{

			intersectionBehaviour = b;
			mainBehaviour = TrafficLightApproachTree ();


		}


		public IBehaviourTreeNode TrafficLightApproachTree ()
		{

			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Selector ("Have we reached the intersection?").
					Sequence ("change to intersection behaviour").
						Do ("Change to associated intersection behaviour", ()=> DecideChangeToAssociatedIntersectionBehaviour ()).
					End ().
					Sequence ("Track traffic light state").
						Do ("Check traffic light state", ()=> CheckTrafficLightState ()).
						Do ("Actions on state change", ()=> TrafficLightStateChangeActions ()).
						Selector ("Select one of the traffic light actions").
							Sequence ("Green light").
								Condition ("Is green light", ()=> {
									return tlTrack.trafficLightState == TrafficLight.TrafficLightState.Green;
								}).//Make some cautions factor
							//Do("reduce speed to approach traffic light",t=>ReduceSpeedToStopLine()).
							End ().
							Sequence ("Amber light").
								Condition ("Is amber light", ()=> {
									return tlTrack.trafficLightState == TrafficLight.TrafficLightState.Amber;
								}).
								Selector ("decide to stop or go on").
									Sequence ("decided to stop at amber?").
										Condition (" has decided to  stop?", ()=> {
											return tlTrack.decidedToStopAtAmber == true;
										}).
										Condition (" has reached intersection stop?", ()=> {
											return intersectionBehaviour.HasReachedIntersectionStop();
										}).
										Do ("wait until clear", ()=> WaitUntilGreenLight ()).
									End ().
									//Condition ("true condition", ()=> {
									//return true;
									//}).//Make sure we do not select next action
									Sequence ("decided to go on at amber?").
										Condition (" has decided to  go on?", ()=> {
										return (tlTrack.currentAction ==TrafficLightStateTrack.DecidedAction.AmberGoOn);
										}).
										Condition (" has reached intersection stop?", ()=> {
											return intersectionBehaviour.HasReachedIntersectionStop();
										}).
										Do ("wait until clear", ()=> EndTrafficLightAtAmber()).
									End ().
									Condition ("true condition", ()=> {
									return true;
									}).//Make sure we do not select next action
								End ().
							End ().
						Sequence ("Red light").
							Condition ("Is red light", ()=> {
								return tlTrack.trafficLightState == TrafficLight.TrafficLightState.Red;
							}).
								Selector ("stop or wait at red light").
									Sequence ("wait at red light").
										Condition (" has reached intersection stop?", ()=> {
											return intersectionBehaviour.HasReachedIntersectionStop();;
										}).
										Do ("wait until clear", ()=> WaitUntilGreenLight ()).
									End ().
									//Do("stop at stopline",t=>StopAtRedLight()).
									Condition ("true condition", ()=> {
									return true;
									}).//Make sure we do not select next action
								End ().
						End ().
					End ().//Selector traffic light actions
				End ().//Traffic light sequence
			End ().//Selector "have we reached intersection
			
			Build ();
		}


		public void SetTrafficLight (TrafficLight light, int index)
		{
			tlTrack = new TrafficLightStateTrack (light, index);
		}

		public FluentBehaviourTree.BehaviourTreeStatus EndTrafficLightAtAmber() {
			intersectionBehaviour.EndTrafficLightTrack();
			return  FluentBehaviourTree.BehaviourTreeStatus.Success;
			
		}
		public FluentBehaviourTree.BehaviourTreeStatus DecideChangeToAssociatedIntersectionBehaviour ()
		{
			//SetCrossingIntersectionState ();
			//ailogic.SetCurrentBehaviour (intersectionBehaviour);
			//ailogic.EndRunningBehaviour (this);




			CheckTrafficLightState();
			if (intersectionBehaviour.OnIntersection ()) {
				//We may have just barely reached the interesection because of our speed but it is red yet, force stop
				if (tlTrack.trafficLightState == TrafficLight.TrafficLightState.Red) {
					if (tlTrack.currentAction == TrafficLightStateTrack.DecidedAction.StopAtRedLight) {
						//This should have change only when a proper transition has been triggered, so keep on the stopline

						return FluentBehaviourTree.BehaviourTreeStatus.Failure;
					} else if (tlTrack.currentAction == TrafficLightStateTrack.DecidedAction.AmberGoOn) {
						//We had decided not to stop due to amber, just go on
						intersectionBehaviour.EndTrafficLightTrack();
						return  FluentBehaviourTree.BehaviourTreeStatus.Success;
					
					}

					if (intersectionBehaviour.ailogic.currentLane.endIntersection == intersectionBehaviour.intersection) {
					
						if (intersectionBehaviour.ailogic.currentLane.IsOnLane (intersectionBehaviour.ailogic.vehicleInfo.backBumper)) {
						
							return FluentBehaviourTree.BehaviourTreeStatus.Failure;
						}
					}
				}
				intersectionBehaviour.EndTrafficLightTrack();
				return  FluentBehaviourTree.BehaviourTreeStatus.Success;
			}

			if (intersectionBehaviour.HasReachedIntersectionStop ()) {
				if (tlTrack.trafficLightState == TrafficLight.TrafficLightState.Green || tlTrack.trafficLightState == TrafficLight.TrafficLightState.GreenNoPriority) {
					//Continue wiht intersection behaviour
					intersectionBehaviour.EndTrafficLightTrack();
					return  FluentBehaviourTree.BehaviourTreeStatus.Success;

				} 
				if (tlTrack.trafficLightState == TrafficLight.TrafficLightState.Amber) {
					if (tlTrack.currentAction == TrafficLightStateTrack.DecidedAction.GreenGoOn) {
						intersectionBehaviour.EndTrafficLightTrack();
						return  FluentBehaviourTree.BehaviourTreeStatus.Success;
					}
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Failure;

			


		}


		public FluentBehaviourTree.BehaviourTreeStatus CheckTrafficLightState ()
		{
			
			tlTrack.UpdateState ();
			return  FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus TrafficLightStateChangeActions ()
		{
			//This should go to a behaviour tree but then it becomes too lenghty
			//intersectionBehaviour.ailogic.Log("tracking traffic light. State=" + tlTrack.trafficLightState);
			if (tlTrack.HasChangedState ()) {


				switch (tlTrack.trafficLightState) {
				case TrafficLight.TrafficLightState.Green:
					return TransitionToGreen ();
					break;
				case TrafficLight.TrafficLightState.GreenNoPriority:
					//TODO: consider this as Green at the moment
					return TransitionToGreen ();
					break;
				case TrafficLight.TrafficLightState.Amber:
					tlTrack.SetLastSeenState (tlTrack.trafficLightState);
					return  DecideAmberAction ();
					break;
			
				case TrafficLight.TrafficLightState.Red:
					tlTrack.SetLastSeenState (tlTrack.trafficLightState);
					return StopAtRedLight ();
					break;

				}
			} 
			return  FluentBehaviourTree.BehaviourTreeStatus.Success;


		}

		public FluentBehaviourTree.BehaviourTreeStatus TransitionToGreen ()
		{
			if (tlTrack.lastSeenState == TrafficLight.TrafficLightState.Amber) {
				if (tlTrack.decidedToStopAtAmber) {
					tlTrack.decidedToStopAtAmber = false;

				}
				if (tlTrack.waitingAtStopLine) {
					tlTrack.waitingAtStopLine = false;
					intersectionBehaviour.SetNextPathSpeedLimit ();
					tlTrack.SetLastSeenState (tlTrack.trafficLightState);

					return intersectionBehaviour.SetApplyBehaviour ();
				}
			}
			if (tlTrack.lastSeenState == TrafficLight.TrafficLightState.Red) {
				intersectionBehaviour.ailogic.vehicleInfo.SetDriving ();
				if (tlTrack.waitingAtStopLine) { 
					tlTrack.waitingAtStopLine = false;

					intersectionBehaviour.SetNextPathSpeedLimit ();
					tlTrack.SetLastSeenState (tlTrack.trafficLightState);

					return intersectionBehaviour.SetApplyBehaviour ();
				} else {
					//We may have stopped before reaching the stopline because someone is there already
					//Drive normally

					intersectionBehaviour.SetApplyBehaviour();
				}
			}

			tlTrack.currentAction = TrafficLightStateTrack.DecidedAction.GreenGoOn;
			tlTrack.SetLastSeenState (tlTrack.trafficLightState);
			return ReduceSpeedToStopLine ();
		}

		public FluentBehaviourTree.BehaviourTreeStatus ReduceSpeedToStopLine ()
		{
			intersectionBehaviour.ReduceSpeedToStopLine (safeUrbanSpeed);

			return  FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus DecideAmberAction ()
		{
			float distanceToStopLine;
			if (intersectionBehaviour.ComputeDistanceToStopLine ( out distanceToStopLine)) {
				if (distanceToStopLine > tlTrack.distanceToBrakeAtAmber) {
					
					tlTrack.decidedToStopAtAmber = true;
					tlTrack.currentAction = TrafficLightStateTrack.DecidedAction.StopAtRedLight;
					if (intersectionBehaviour.SetStopAtStopLine (distanceToStopLine)) {
						return  FluentBehaviourTree.BehaviourTreeStatus.Success;
					} else {
						return  FluentBehaviourTree.BehaviourTreeStatus.Running;
					}
				} else {
					tlTrack.decidedToStopAtAmber = false;
					tlTrack.currentAction = TrafficLightStateTrack.DecidedAction.AmberGoOn;
				}
			} else {
				//ComputeDistanceToStopLine (stopLinePosition,pair);

				throw new UnityException ();
			}

			return  FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		public FluentBehaviourTree.BehaviourTreeStatus StopAtRedLight ()
		{
			if (tlTrack.waitingAtStopLine == false) {
				
				if (intersectionBehaviour.SetStopAtStopLine ()) {
					tlTrack.currentAction = TrafficLightStateTrack.DecidedAction.StopAtRedLight;
					return  FluentBehaviourTree.BehaviourTreeStatus.Success;
				} else {
					return  FluentBehaviourTree.BehaviourTreeStatus.Running;
				}
			} else {

				return  FluentBehaviourTree.BehaviourTreeStatus.Success;
			}
		}

		public FluentBehaviourTree.BehaviourTreeStatus WaitUntilGreenLight ()
		{
			intersectionBehaviour.ailogic.vehicleInfo.SetWaitingAtRedLight ();
			tlTrack.waitingAtStopLine = true;
			return  FluentBehaviourTree.BehaviourTreeStatus.Running;
		}


	}

}