/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Veneris
{


	public class IntersectionBehaviourSelector
	{

		

		public AILogic ailogic;
		public GameObject gameobject;
		public IntersectionInfo intersection = null;

		public Transform stopLinePosition = null;

	
		public BoxCollider stopLineCollider=null;
		public Transform internalStopPosition = null;
		public  AIBehaviour.PriorityType priorityType;
		
		public PathConnector connector = null;
		public Path internalPath = null;
		public List<Path> internalPaths = null;
		public bool internalLaneEndReached = false;
		//We have reached the end of the internal lane
		public bool internalLaneEndExit = false;
		public bool intersectionStopLineReached = false;
		//True when we reach the stopline
		public bool internalStopReached = false;
		//true when we reach an internal stop


		public long pathIdForPriority = -1;
		
		public ConnectionInfo.PathDirectionInfo plannedPath = null;
		public List<Transform> priorityCheckPositions = null;
		public  ThrottleGoalForPoint goal = null;

	

		public IntersectionBehaviour.IntersectionAction action;
		public IntersectionBehaviour.IntersectionApproachAction approachAction;
		public ConnectionInfo.ConnectionDirection currentConnectionDirection;
		//Used to solve locks

		//public TrafficLightApproach tlBehaviour = null;
		//public TrafficLightTracker tlBehaviour = null;
		public IntersectionBehaviour selectedBehaviour = null;



		public IntersectionBehaviourSelector (GameObject o, AILogic logic)
		{
			gameobject = o;
			ailogic = logic;
			action = IntersectionBehaviour.IntersectionAction.Undefined;
			approachAction = IntersectionBehaviour.IntersectionApproachAction.Undefined;
			selectedBehaviour = null;
			//tlBehaviour = null;
			
		}

		public void Setintersection (IntersectionInfo i)
		{
			intersection = i;
		}

		public void SetStopLinePosition (Transform t, BoxCollider bc)
		{
			stopLinePosition = t;
			stopLineCollider = bc;
		}

		public void SetPathConnector (PathConnector c)
		{
			connector = c;
		}

		public void SetPlannedPath (ConnectionInfo.PathDirectionInfo pair)
		{
			plannedPath = pair;
		}


		public IntersectionBehaviour SelectAndCreateBehaviour ()
		{
			if (SelectTypeOfApproach ()) {
				if (SelectTypeOfAction ()) {
					//if (approachAction == IntersectionBehaviour.IntersectionApproachAction.TrackTrafficLight) {
					//	return tlBehaviour;
					//} else {
						return selectedBehaviour;
					//}
				}
			}
			return selectedBehaviour;
		}


		

		
		




		




		/*public override void Prepare ()
		{
			
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			mainBehaviour = builder.
				Sequence ("select intersection behaviour").
			//Do("check required components",t=>CheckRequiredComponents()).
					Do ("select type of action", t => SelectTypeOfApproach ()).
					Do ("select type of action", t => SelectTypeOfAction ()).
					Do("Change to behaviour",t=>SetBehaviourAndFinish()).
				End ().
			Build ();
			/*	Selector ("Select and do one the actions").
							Sequence ("straight-without-braking").
								Condition ("check straight-without-braking", t => {return action == IntersectionAction.StraightWithoutBraking;}).
								Selector("Select traffic light or free approach").
									Sequence("approach with traffic light").
										Condition("approachAction is traffic light",t=>{return approachAction ==IntersectionApproachAction.TrackTrafficLight;}).
										Splice(TrafficLightApproachTree()).
									End().
									Sequence ("approach without traffic light or already in intersection").
										Condition("approachAction is intersection",t=>{return approachAction ==IntersectionApproachAction.GivenByIntersectionAction;}).
										Splice(StraightWithoutBrakingTree()).
									End ().
								End(). //Selector Select traffic light or free approach
							End ().
							Sequence ("prepare-to-turn-with-priority").
								Condition ("check prepare-to-turn-with-priority", t => {return action == IntersectionAction.PrepareToTurnWithPriority;}).
								//Do ("set mode to curvature", t => SetBrakeToTurn()).
								Selector("Select traffic light or free approach").
									Sequence("approach with traffic light").
										Condition("approachAction is traffic light",t=>{return approachAction ==IntersectionApproachAction.TrackTrafficLight;}).
										Splice(TrafficLightApproachTree()).
									End().
									Sequence ("approach without traffic light or already in intersection").
										Condition("approachAction is intersection",t=>{return approachAction ==IntersectionApproachAction.GivenByIntersectionAction;}).
										Splice(PrepareToTurnWithPriorityTree()).
									End(). //Sequence approach without traffic light or already in intersection"
								End().//Selector Select traffic light or free approach
							End ().
							Sequence ("prepare to stop").
								Condition ("check prepare-to-stop", t => {return action == IntersectionAction.PrepareToStop;}).
								Selector("Select traffic light or free approach").
									Sequence("approach with traffic light").
										Condition("approachAction is traffic light",t=>{return approachAction ==IntersectionApproachAction.TrackTrafficLight;}).
										Splice(TrafficLightApproachTree()).
									End().
									Sequence ("approach without traffic light or already in intersection").
										Condition("approachAction is intersection",t=>{return approachAction ==IntersectionApproachAction.GivenByIntersectionAction;}).
										Splice(PrepareToStopTree()).
									End().
								End().//Selector Select traffic light or free approach
							End(). //sequence prepare to stop
							Sequence ("stop at internal junction").
								Condition ("check stop at internal junction", t => {return action == IntersectionAction.StopAtInternalJunction;}).
								Selector("Select traffic light or free approach").
									Sequence("approach with traffic light").
										Condition("approachAction is traffic light",t=>{return approachAction ==IntersectionApproachAction.TrackTrafficLight;}).
										Splice(TrafficLightApproachTree()).
									End().
									Sequence ("approach without traffic light or already in intersection").
										Condition("approachAction is intersection",t=>{return approachAction ==IntersectionApproachAction.GivenByIntersectionAction;}).
										Splice(StopAtInternalJunctionTree()).
									End().
								End().//Selector Select traffic light or free approach
							End(). //sequence stop at internal junctions
						End(). //selector
				End (). //sequence "intersection";
				Build ();
				*/
			
		/*	

		mainBehaviour = builder.
			Sequence ("intersection").
			//Do("check required components",t=>CheckRequiredComponents()).

			Do ("select type of action", t => SelectTypeOfAction ()).
			Selector ("Select and do one the actions").
			Sequence ("straight-without-braking").
			Condition ("check straight-without-braking", t => {return action == IntersectionAction.StraightWithoutBraking;}).
			//Do("check priority positions",t=>SetDrivingCheckPositions(priorityCheckPositions)).
			//Do("unset priority positions when intersection is reached",t=>SetUnsetPriorityPositionsTriggerAction()).
			Do ("check straight-without-braking:change to default-behaviour", t => SetDefault ()).
			End ().
			Sequence ("prepare-to-turn-with-priority").
			Condition ("check prepare-to-turn-with-priority", t => {return action == IntersectionAction.PrepareToTurnWithPriority;}).
			//Do ("set mode to curvature", t => SetBrakeToTurn()).
			Parallel("do turn with priority",2,1). //Check start and stop of intersections while driving
			ExecuteUntilSuccessNTimes("check reached stop line once",1). //Only run succesfully once the following sequence
			Sequence("checked reached stop line").
			Condition(" intersection stop line reached?",t=>{return intersectionStopLineReached==true;}).
			Do("change to adapt to curvature",t=>SetAdaptToCurvature()).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("set current action in vehicle info",t=>SetCrossingIntersectionState()).
			End().
			End().
			Sequence("check-reached-end-of intersection").
			Condition("reached end?",t=>{return internalLaneEndReached==true;}).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("Apply behaviour",t=>SetApplyBehaviour()).
			Do ("change to default-behaviour", t => SetDefault ()).
			End().
			//We may do something else while turning at the intersection


			Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end
			End(). //Parallel
			End ().
			Sequence ("prepare to stop").
			Condition ("check prepare-to-stop", t => {return action == IntersectionAction.PrepareToStop;}).
			Parallel("do prepare to stop",3,1).

			ExecuteUntilSuccessNTimes("check priority positions once",1). //Only run succesfully once the following sequence
			Sequence("check priority positions").
			Condition(" has reached intersection stop and has stopped?",t=>{return (throttleHelper.HasReachedStopPoint() && intersectionStopLineReached==true);}).
			Do("wait until clear",t=>WaitUntilCleared()).
			Do("change to adapt to curvature",t=>SetAdaptToCurvature()).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("keep checking for vehicles with priority in the intersection",t=>SetDrivingCheckPositions(priorityCheckPositions)).
			Do("set current action in vehicle info",t=>SetCrossingIntersectionState()).
			End().
			End().

			Sequence("drive through intersection").
			Condition("reached end of intersection?",t=>{return internalLaneEndReached==true;}).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("Apply behaviour",t=>SetApplyBehaviour()).
			Do("stop checking for vehicles with priority in the intersection",t=>UnsetDrivingCheckPositions()).
			Do ("change to default-behaviour", t => SetDefault ()).
			End().


			Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end

			End (). //parallel
			End(). //sequence prepare to stop
			Sequence ("stop at internal junction").
			Condition ("check stop at internal junction", t => {return action == IntersectionAction.StopAtInternalJunction;}).
			Parallel("do stop at internal junction",3,1).
			ExecuteUntilSuccessNTimes("check priority positions once",1). //Only run succesfully once the following sequence
			Sequence("check priority positions").
			//Condition(" has reached intersection stop?",t=>{return (throttleHelper.HasReachedStopPoint() && intersectionStopLineReached==true);}). //Do not need to stop at the stop line
			Condition(" has reached intersection stop line?",t=>{return (intersectionStopLineReached==true);}). //Do not need to stop at the stop line							
			//Do("wait until clear",t=>WaitUntilCleared()).
			Do("Set throttle mode to stop at internal position", t=>SetStopAtInternalPosition(internalStopPosition.position)).
			//Do("change to adapt to curvature",t=>SetAdaptToCurvature()).
			Do("keep checking for vehicles with priority in the intersection",t=>SetDrivingCheckPositions(priorityCheckPositions)).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("set current action in vehicle info",t=>SetCrossingIntersectionState()).
			End().
			End().
			ExecuteUntilSuccessNTimes("check priority positions at internal stop once",1).
			Sequence("check priority positions at internal stop").
			Condition("has reached internal stop and has stopped?",t=>{return throttleHelper.HasReachedStopPoint() && internalStopReached==true;}).
			Do("wait until clear",t=>WaitUntilCleared()).
			Do("keep checking for vehicles with priority in the intersection",t=>SetDrivingCheckPositions(priorityCheckPositions)).
			Do("change to adapt to curvature",t=>SetAdaptToCurvature()).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("set current action in vehicle info",t=>SetCrossingIntersectionState()).
			//Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			//Do ("change to default-behaviour", t => SetDefault ()).
			End().
			End().
			Sequence("drive until end of intersection").
			Condition("reached end of intersection?",t=>{return internalLaneEndReached==true;}).
			Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
			Do("Apply behaviour",t=>SetApplyBehaviour()).
			Do("stop checking for vehicles with priority in the intersection",t=>UnsetDrivingCheckPositions()).
			Do ("change to default-behaviour", t => SetDefault ()).
			End().

			Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end

			End (). //parallel
			End(). //sequence stop at internal junctions
			End(). //selector
			End (). //sequence "intersection";
			Build ();
		



		}
	*/



		
		

		public void SetInternalPath (Path p)
		{
			internalPath = p;
			internalLaneEndReached = false;
		}
		public void SetInternalPaths(List<Path> intPaths) {
			internalPaths = intPaths;
		}

		public bool SelectTypeOfApproach ()
		{
			if (approachAction == IntersectionBehaviour.IntersectionApproachAction.Undefined) {
				if (plannedPath == null) {
					//Our vehicle may have seen  a connector for other path??

					//	Debug.Log ("Intersection stop: Connector is not for my current path" + connector.name);
					
					return false;
				} else {
					if (plannedPath.trafficLight != null) {
						
						approachAction = IntersectionBehaviour.IntersectionApproachAction.TrackTrafficLight;
						
						
						
					} else {
						approachAction = IntersectionBehaviour.IntersectionApproachAction.GivenByIntersectionAction;
					}
					return true;
				}
			} 
			return false;
			
		}

		public bool SelectTypeOfAction ()
		{
			if (action == IntersectionBehaviour.IntersectionAction.Undefined) {
				
				if (plannedPath == null) {

					return false;
				} else {

					SetInternalPath (plannedPath.p);
					if (SetNextAction (plannedPath)) {
						return true;
					} else {
						
						return false;
					}
				}
			} else {
				return false;
			}
		}

	
		// protected bool SetNextAction (ConnectionInfo.PathDirectionInfo pair)
		// {
		//
		// 	IntersectionPriorityInfo priority = pair.p.GetComponent<IntersectionPriorityInfo> ();
		// 	if (priority != null) {
		// 		currentConnectionDirection = pair.direction;
		// 		if (priority.HasHigherPriorityLanes ()) {
		// 			priorityCheckPositions = priority.GetCheckPositions ();
		// 			if (priority.StopAtInternalPosition ()) {
		// 				//Debug.Log ("stop at internal junction");
		// 				action = IntersectionBehaviour.IntersectionAction.StopAtInternalJunction;
		// 				internalStopPosition = priority.GetInternalStopPosition ();
		// 				
		// 				
		// 				
		// 				
		// 				//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
		// 				CreateStopAtInternalJunction ();
		// 				return true;
		// 				
		// 				
		// 			} else {
		// 				//There are lanes with priority that we have to check for incoming vehicles
		// 				//Debug.Log ("prepare to stop");
		// 				action = IntersectionBehaviour.IntersectionAction.PrepareToStop;
		//
		// 				
		// 				CreateStop ();
		// 				//return SetBrakeToStop (stopLinePosition.position, distanceToStopLine);
		// 				return true;
		// 				
		// 				
		// 			}
		// 			//In both cases we have to stop first a the stop line and check
		//
		//
		//
		//
		//
		// 		} else {
		// 			//We have prioriy
		// 			switch (pair.direction) {
		// 			case (ConnectionInfo.ConnectionDirection.Straight):
		// 			//	Debug.Log ("straight");
		// 				action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
		// 				priorityCheckPositions = intersection.stopLines;
		// 				
		// 				CreateStraightWithoutBraking ();
		// 				return true;
		// 				break;
		// 			default:
		// 			//	Debug.Log ("turn");
		// 			
		// 				action = IntersectionBehaviour.IntersectionAction.PrepareToTurnWithPriority;
		// 				
		// 				
		// 				CreateTurnWithPriority ();
		// 				return true;
		// 					//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
		// 				
		//
		// 				break;
		// 			}
		//
		// 		}
		// 	} else {
		// 		//We have prioriy
		// 		switch (pair.direction) {
		// 		case (ConnectionInfo.ConnectionDirection.Straight):
		// 		//	Debug.Log ("straight");
		// 			action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
		// 			priorityCheckPositions = intersection.stopLines;
		// 			
		// 			CreateStraightWithoutBraking ();
		// 			return true;
		// 			break;
		// 		default:
		// 		//	Debug.Log ("turn");
		//
		// 			action = IntersectionBehaviour.IntersectionAction.PrepareToTurnWithPriority;
		// 			
		// 		//initialDistanceToStop = Mathf.Abs (Vector3.Dot (ailogic.vehicleInfo.frontBumper.position - internalPath.GetFirstNode ().transform.position, ailogic.vehicleInfo.frontBumper.forward));
		// 			
		// 				//ComputeDistanceToStopLine (stopLinePosition,pair);
		// 			CreateTurnWithPriority ();
		// 			return true;
		// 				//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
		// 			
		// 			break;
		// 		}
		//
		// 	}
		// 	return false;
		//
		//
		// }

		protected bool SetNextAction (ConnectionInfo.PathDirectionInfo pair)
		{

			IntersectionPriorityInfo priority = pair.p.GetComponent<IntersectionPriorityInfo> ();
			if (priority != null) {
				currentConnectionDirection = pair.direction;
				if (priority.HasHigherPriorityLanes ()) {
					priorityCheckPositions = priority.GetCheckPositions ();
					if (priority.StopAtInternalPosition ()) {
						//Debug.Log ("stop at internal junction");
						action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
						internalStopPosition = priority.GetInternalStopPosition ();
						
						
						
						
						//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
						CreateStraightWithoutBraking ();
						return true;
						
						
					} else {
						//There are lanes with priority that we have to check for incoming vehicles
						//Debug.Log ("prepare to stop");
						action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;

						
						CreateStraightWithoutBraking ();
						//return SetBrakeToStop (stopLinePosition.position, distanceToStopLine);
						return true;
						
						
					}
					//In both cases we have to stop first a the stop line and check





				} else {
					//We have prioriy
					switch (pair.direction) {
					case (ConnectionInfo.ConnectionDirection.Straight):
					//	Debug.Log ("straight");
						action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
						priorityCheckPositions = intersection.stopLines;
						
						CreateStraightWithoutBraking ();
						return true;
						break;
					default:
					//	Debug.Log ("turn");
					
						action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
						
						
					CreateStraightWithoutBraking ();
						return true;
							//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
						

						break;
					}

				}
			} else {
				//We have prioriy
				switch (pair.direction) {
				case (ConnectionInfo.ConnectionDirection.Straight):
				//	Debug.Log ("straight");
					action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
					priorityCheckPositions = intersection.stopLines;
					
					CreateStraightWithoutBraking ();
					return true;
					break;
				default:
				//	Debug.Log ("turn");

					action = IntersectionBehaviour.IntersectionAction.StraightWithoutBraking;
					
				//initialDistanceToStop = Mathf.Abs (Vector3.Dot (ailogic.vehicleInfo.frontBumper.position - internalPath.GetFirstNode ().transform.position, ailogic.vehicleInfo.frontBumper.forward));
					
						//ComputeDistanceToStopLine (stopLinePosition,pair);
				CreateStraightWithoutBraking ();
					return true;
						//return SetBrakeToTurn (stopLinePosition.position, distanceToStopLine);
					
					break;
				}

			}
			return false;


		}
		
		public void SetIntersectionBehaviourCommonInterface (IntersectionBehaviour b)
		{
			
			
			b.SetIntersection(this.intersection);

			b.internalPath = this.internalPath;
			b.priorityType = this.priorityType;
			b.pathIdForPriority = this.pathIdForPriority;

			b.SetStopLinePosition (this.stopLinePosition, this.stopLineCollider);
			b.SetPathConnector (connector);
			b.SetPlannedPath (plannedPath);
			b.currentConnectionDirection = this.currentConnectionDirection;
			b.approachAction = this.approachAction;

			if (approachAction == IntersectionBehaviour.IntersectionApproachAction.TrackTrafficLight) {
				b.tlBehaviour = CreateTrafficLightTracker (b);
			}
			selectedBehaviour = b;

		}

		public void CreateStraightWithoutBraking ()
		{
			StraightWithoutBraking s = gameobject.AddComponent<StraightWithoutBraking> ();
			SetIntersectionBehaviourCommonInterface (s);
			
			s.Prepare ();


		}

		/*public void CreateTrafficLightApproach ()
		{
			
			TrafficLightApproach tla = gameobject.AddComponent<TrafficLightApproach> ();
			tlBehaviour = tla;
			SetIntersectionBehaviourCommonInterface (tla);
			tla.SetTrafficLight (plannedPath.trafficLight, plannedPath.trafficLightIndex);
			tla.Prepare ();
		}*/
		public TrafficLightTracker CreateTrafficLightTracker (IntersectionBehaviour b)
		{
			TrafficLightTracker tlt = new TrafficLightTracker (b);
			tlt.SetTrafficLight (plannedPath.trafficLight, plannedPath.trafficLightIndex);
			return tlt;
			
		}

		public void CreateStopAtInternalJunction ()
		{
			StopAtInternalJunction stopi = gameobject.AddComponent<StopAtInternalJunction> ();
			SetIntersectionBehaviourCommonInterface (stopi);
			stopi.internalStopPosition = this.internalStopPosition;
			stopi.internalPath = this.internalPath;
			stopi.internalPaths = internalPaths;
			if (stopi.internalPath != stopi.internalPaths [0]) {
				Debug.LogError ("Internal path is different from internalPaths[0]");
			}
			stopi.priorityCheckPositions = this.priorityCheckPositions;
			stopi.Prepare ();

			
			
		}

		public void CreateStop ()
		{
			Stop stop = gameobject.AddComponent<Stop> ();
			SetIntersectionBehaviourCommonInterface (stop);
			
			
			stop.priorityCheckPositions = this.priorityCheckPositions;
			stop.Prepare ();

			
			
		}

		public void CreateTurnWithPriority ()
		{
			TurnWithPriority turn =	gameobject.AddComponent<TurnWithPriority> ();
			SetIntersectionBehaviourCommonInterface (turn);
			turn.Prepare ();
			
			
		}

		



	}
}
