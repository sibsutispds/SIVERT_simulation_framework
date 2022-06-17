/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

namespace Veneris
{
	public class StrategicPlanAILogicCITS : AILogic
	{
		[System.Serializable]
		public class RoadPlan
		{
			public VenerisRoad road;
			public VenerisLane startLane;
			public VenerisLane targetLane;
			public List<LaneChangeQueueEntry> laneChanges;
			public IntersectionBehaviour intersectionBehaviour;

			public void AddChange (LaneChangeQueueEntry entry)
			{
				if (laneChanges == null) {
					laneChanges = new List<LaneChangeQueueEntry> ();
				}
				laneChanges.Add (entry);
			}

			public RoadPlan (VenerisRoad road)
			{
				this.road = road;
			}

		}

		[System.Serializable]
		public class RoutePlan
		{
			public List<RoadPlan> segments;


			public RoadPlan GetLastSegment ()
			{
				return segments [segments.Count - 1];
			}

			public RoadPlan GetSegmentFromEnd (int n)
			{
				if ((segments.Count - 1 - n) >= 0) {

					return segments [segments.Count - 1 - n];
				} else {
					return null;
				}
			}

			public int GetIndexOfRoadInRoutePlan (VenerisRoad road)
			{
				int index = -1;
				for (int i = 0; i < segments.Count; i++) {
					if (segments [i].road == road) {
						return i;
					}
				}
				return index;
			}

			public void AddSegment (RoadPlan segment)
			{
				if (segments == null) {
					segments = new List<RoadPlan> ();
				}
				segments.Add (segment);
			}


			public void Reverse ()
			{
				if (segments != null) {
					segments.Reverse ();
				}
			}



		}

		public bool planNextMovementsPending = true;
		public int edgesToPlan = 3;
		//Number of edges to in the route plan
		public int edgesInAdvance = 1;
		//Number of edges before the last one to update the route plan: 0 updates in the last edge of the plan
		public RoutePlan partialPlan = null;
		public SimplePriorityQueue<AIBehaviour,float> behaviourQueue;
		public List<AIBehaviour> endedBehaviours = null;
		public float currentPriority = 0f;


		//Assuming maxDec=10 m/s2 (from our tests), and a (urban) maximum speed around v=28 m/s, we need v*v/(2*maxDec) m to stop, around 40 m.
		//After a lane change we should still have that 40 m availble before the next intersection to be able to stop.
		//So being conservative, assuming we need half the length of the lane to make the change if the speed is high, we need 2*40 m minimum
		public float minimumPathLengthToWaitForLaneChange = 80f;
		public float costForLaneChangeOnTooShortRoadsWithVehicles = 30f;
		public float costForLaneChangeWithVehicles = 2f;
		public float costForLaneChangeOnTooShortRoadsWithDensity= 3f;
		public float costForLaneChangeWithDensity = 0.2f;

		protected override void Awake ()
		{
			base.Awake ();



		}
		// Use this for initialization
		protected override void Start ()
		{
			base.Start ();

			//Behaviour is not based on visual checks, so remove them
			vision.RemoveEnterListener (HandleVisionTriggerEnter);
			vision.RemoveExitListener (HandleVisionTriggerExit);
			behaviourQueue = new SimplePriorityQueue<AIBehaviour, float> ();
			endedBehaviours = new List<AIBehaviour> ();
			//Plan next moves
			planNextMovementsPending = true;
			routeManager.AddPath (routeManager.StartPath ());
			InitRoutePlan ();

		}

		protected override void Update ()
		{
			if (Time.timeScale == 0) {
				return;
			}
			DestroyEndedBehaviours ();

			if (planNextMovementsPending) {
				PlanNextMoves ();
			}
			//Check consistency
			if (currentLane.paths [0] == routeManager.trackedPath) {
				IntersectionBehaviour ib = currentBehaviour as IntersectionBehaviour;
				if (ib != null) {
					if (!routeManager.IsOnForwardRoads (ib.fromRoad)) {
						EndRunningBehaviour (currentBehaviour);
					}

				}
			}
			if (behaviourQueue.Count > 0) {
				if (behaviourQueue.First != currentBehaviour) {
					SetCurrentBehaviour (behaviourQueue.First);
				}
			} else {
				RecoverDefaultBehaviour ();
			}

			if (currentBehaviour == null) {
				RecoverDefaultBehaviour ();
			}


		}

		public bool OnEndOfRoute() {
			if (routeManager.GetLastRoadInRoute () == currentRoad) {
				return true;
			}
			return false;
		}
		protected override void HandleEndOfRoute (Collider other)
		{
			if (OnEndOfRoute ()) {
				base.HandleEndOfRoute (other);
			} else {
				
				//Now, we may have problems if end of route is on a short road or very close to a turning point
			
				//TODO: check all this
				//Recheck first
				CheckCurrentLane();
				if (OnEndOfRoute ()) {
					base.HandleEndOfRoute (other);

				} else if (routeManager.GetForwardRoads () == null) {
					//Just end the vehicle right now
					base.HandleEndOfRoute (other);
				} else 	if (routeManager.GetForwardRoads ().Count <= 2) {
					base.HandleEndOfRoute (other);
				}

			}
			//Do not do anything...
		}
		protected void EnqueueBehaviour (AIBehaviour b)
		{
			currentPriority += 10f;
			behaviourQueue.Enqueue (b, currentPriority);
		}


		public void LogAstarPath (AStarPath<AStarLaneNode> path, int id = -1)
		{
			if (id < 0) {
				Log ("Total cost due to path=" + path.TotalCost);
			} else {
				Log (id, "Total cost due to path=" + path.TotalCost);
			}
			int steps = 0;

			AStarPath<AStarLaneNode> pointer = path.PreviousSteps;
			if (id < 0) {
				Log ("step " + steps + ": " + path.LastStep.lane.sumoId + "cost=" + (path.TotalCost - pointer.TotalCost));

			} else {
				Log (id, "step " + steps + ": " + path.LastStep.lane.sumoId + "cost=" + (path.TotalCost - pointer.TotalCost));
			}
			while (pointer != null) {
				steps++;
				if (id < 0) {
					Log ("step " + steps + ": " + pointer.LastStep.lane.sumoId + ". Cost=" + pointer.TotalCost);
				} else {
					Log (id, "step " + steps + ": " + pointer.LastStep.lane.sumoId + ". Cost=" + pointer.TotalCost);
				}

				pointer = pointer.PreviousSteps;

			}

		}

		protected void InitRoutePlan ()
		{
			partialPlan = new RoutePlan ();
			//CheckCurrentLane ();
			List<VenerisRoad> forward = routeManager.GetForwardRoads (edgesToPlan + 1);
			if (forward != null) {
				//Log ("Planning next " + edgesToPlan + " moves.");
				//Check that we are in the right road
				VenerisRoad currentRoad=currentLane.GetComponentInParent<VenerisRoad> ();
				if ( currentRoad== forward [0]) {
					
					int end = forward.Count;
					VenerisLane endLane;
					if (end == 2) {

						List<VenerisLane> outcoming = GlobalRouteManager.Instance.GetOutcomingLanes (forward [end - 2], forward [end - 1], currentLane);
						//May fail if the starting lane is not connected
						if (outcoming.Count == 0) {
							//Try others
							for (int i = 0; i < currentRoad.lanes.Length; i++) {
								if (currentRoad.lanes [i] == currentLane) {
									continue;
								}
								outcoming=GlobalRouteManager.Instance.GetOutcomingLanes (forward [end - 2], forward [end - 1], currentRoad.lanes[i]);
								if (outcoming.Count >0) {
									break;
								}
							}

						} 
						if (outcoming.Count == 0) {
							endLane = null;
							LogError ("Cannot find path from startLane " + currentLane.sumoId);
						} else {
							endLane = outcoming [0];
						}


					} else if (end == 1) {
						//End of route
						planNextMovementsPending = false;
						return;
					} else {
						endLane = SelectLessOccupiedTargetLane (forward [end - 2], forward [end - 1]);
					}

					//Log ("starting a* from " + currentLane.sumoId + "to " + endLane.sumoId);
					AStarPath<AStarLaneNode> mcPath = GlobalRouteManager.Instance.GetMinimumCostPathOnRoads (currentLane, endLane, LaneChangeOrOccupancyCostWithDensity, forward);

					//LogAstarPath ( mcPath, 12);
					//LogAstarPath ( mcPath, 40);


					UpdateRoutePlan (mcPath);


					return;
				}
				Log ("not in right road, Current=" + currentLane.GetComponentInParent<VenerisRoad> ().sumoId + "forward=" + forward [0].sumoId);
			} 
			Log ("not in right road, Current=" + currentLane.GetComponentInParent<VenerisRoad> ().sumoId);

		}



		public int ScheduleRoutePlan ()
		{
			int enqueued = 0;
			for (int i = 0; i < partialPlan.segments.Count - 1; i++) {
				List<Path> intPaths;
				//if (partialPlan.segments [i].laneChanges !=null && partialPlan.segments [i].laneChanges.Count > 1) {
				//	Log ("Lane changes from " + partialPlan.segments [i].road.sumoId + " is " + partialPlan.segments [i].laneChanges.Count);
				//	Debug.Break ();
				//}
				//Log ("Scheduling " + partialPlan.segments [i].targetLane.sumoId + " to " + partialPlan.segments [i + 1].startLane.sumoId);
				AIBehaviour b = CreateBehaviour (partialPlan.segments [i].road, partialPlan.segments [i + 1].road, partialPlan.segments [i].targetLane, partialPlan.segments [i + 1].startLane, out intPaths);
				if (b != null) {
					EnqueueBehaviour (b);
					enqueued++;
					routeManager.AddPaths (intPaths);
					routeManager.AddPath (partialPlan.segments [i + 1].startLane.paths [0]);

				} else {
					Log ("Could not create behaviour for segment" + i);
				}
			}
			return enqueued;
		}

		public void DestroyEndedBehaviours ()
		{
			for (int i = 0; i < endedBehaviours.Count; i++) {
				Destroy (endedBehaviours [i]);
				
			}
			endedBehaviours.Clear ();
		}

		public override void EndRunningBehaviour (AIBehaviour b)
		{
			if (b == currentBehaviour) {
				if (behaviourQueue.First == b) {
					endedBehaviours.Add (behaviourQueue.Dequeue ());

				}
				if (behaviourQueue.Count > 0) {
					SetCurrentBehaviour (behaviourQueue.First);
				} else {
					RecoverDefaultBehaviour ();
				}
				
			}

		}

		public AIBehaviour CreateBehaviour (VenerisRoad startRoad, VenerisRoad nextRoad, VenerisLane startRoadEndLane, VenerisLane nextRoadstartLane, out List<Path> intPaths)
		{
			Path targetPath = startRoadEndLane.paths [0];
			intPaths = GlobalRouteManager.Instance.GetPathsFromLaneToLane (startRoad, nextRoad, startRoadEndLane, nextRoadstartLane);
		
			IntersectionBehaviourProvider intersectionProvider = startRoadEndLane.endIntersection;

			//Get PathConnector and direction info
			PathConnector pc = startRoadEndLane.endIntersection.GetComponentInChildren<PathConnector> ();
			ConnectionInfo ci = pc.GetPathsConnectedTo (targetPath.pathId);
			ConnectionInfo.PathDirectionInfo pinfo = ci.GetPathDirectionInfoInConnectedPaths (intPaths [0].pathId);
			//ConnectionInfo.PathDirectionInfo 
			if (pinfo == null) {
				Debug.Log ("PlanForRoadWithLane. Cannot find next path in connected paths");

				return null;
			}
			AIBehaviour newBehaviour;
			if (intersectionProvider.SetBehaviourWithPlannedPath (gameObject, out newBehaviour, intersectionProvider.transform, pinfo, startRoadEndLane.paths [0].pathId, intPaths)) {
				return newBehaviour;
			} else {
				return null;
			}

		}


		protected void PlanNextMoves ()
		{
			if (OnEndOfRoute ()) {
				//Nothing else to plan
				return;
			}
			//Start planning from a number of edges from the last segment in routePlan
			//Take into account that we could have missed a road
			int index = partialPlan.GetIndexOfRoadInRoutePlan (currentRoad);
			if (index < 0) {
				//Keep on trying
				planNextMovementsPending = true;
				return;
			} else if (index >= partialPlan.segments.Count - 1 - edgesInAdvance) {
				//if (currentRoad == partialPlan.GetSegmentFromEnd(edgesInAdvance).road) {
				RoadPlan startPlan = partialPlan.GetLastSegment ();
		

				List<VenerisRoad> forward = routeManager.GetForwardRoads (startPlan.road, edgesToPlan + 1);

				if (forward != null) {

					int end = forward.Count;
					VenerisLane endLane;
					if (end == 2) {
						
						endLane = GlobalRouteManager.Instance.GetOutcomingLanes (forward [end - 2], forward [end - 1], startPlan.targetLane) [0];

					} else if (end == 1) {
						//End of route
						planNextMovementsPending = false;
						return;
					} else {
						endLane = SelectLessOccupiedTargetLane (forward [end - 2], forward [end - 1]);
					}
					//Log ( "starting a* from " + startPlan.targetLane.sumoId + "to " + endLane.sumoId);
					AStarPath<AStarLaneNode> mcPath = GlobalRouteManager.Instance.GetMinimumCostPathOnRoads (startPlan.startLane, endLane, LaneChangeOrOccupancyCostWithDensity, forward);
					//If there are lane changes on the last segment, with the above possibilite (using starlane) we should remove them. Less elegant but provides better paths
					if (startPlan.laneChanges != null) {
						for (int i = 0; i < startPlan.laneChanges.Count; i++) {
							RemoveStrategicLaneChange (startPlan.laneChanges [i]);
						}
					}

					//Another alternative, does not require to remove previously scheduled lane changes
					//AStarPath<AStarLaneNode> mcPath = GlobalRouteManager.Instance.GetMinimumCostPathOnRoads (startPlan.targetLane, endLane, LaneChangeOrOccupancyCost, forward);
					if (mcPath == null) {
						LogError ("Cannot find path from " + startPlan.startLane.sumoId + "to " + endLane.sumoId);
						return;
					}
					//LogAstarPath (mcPath, 7);
					UpdateRoutePlan (mcPath);
					planNextMovementsPending = false;

				} else {
					//Keep on trying

					planNextMovementsPending = true;

				}


			} else {
				planNextMovementsPending = false;
			}
		}

		public void UpdateRoutePlan (AStarPath<AStarLaneNode> path)
		{
			List<RoadPlan> segments = CreateRoadPlans (path);
		
			partialPlan.segments = segments;
			int added = ScheduleRoutePlan ();

			
		}

		public List<RoadPlan> CreateRoadPlans (AStarPath<AStarLaneNode> path)
		{
			VenerisRoad prevRoad = null;
			RoadPlan segment = null;
			List<RoadPlan> plan = new List<RoadPlan> (8);
			AStarLaneNode prevNode = null;
			//Reverse order: AStarPath is a stack from destination to origin
			foreach (AStarLaneNode node in path) {
				//Log ("AStarLaneNode=" + node.ToString ());
				if (prevNode == null) {
					//Init
					//Log ("first");
					prevNode = node;
					prevRoad = node.road;
					segment = new RoadPlan (node.road);
					segment.targetLane = node.lane;
					continue;
				}
				if (node.road != prevRoad) {
					
					//Log ("new");
					segment.startLane = prevNode.lane;
					plan.Add (segment);
					prevRoad = node.road;
					segment = new RoadPlan (node.road);
					segment.targetLane = node.lane;


				} else {
					//Log ("change");
					//Add lane change in reverse order also
					LaneChangeQueueEntry entry = CreateLaneChange (node.lane, prevNode.lane);
					segment.AddChange (entry);
					//TODO: unify all the lane change model, and change computelanechange sequence
					AddStrategicLaneChange (entry);
				}
				prevNode = node;
			}
			//Add last segment
			segment.startLane = prevNode.lane;
			plan.Add (segment);
			plan.Reverse ();
			return plan;
		}

		public LaneChangeQueueEntry CreateLaneChange (VenerisLane fromLane, VenerisLane toLane)
		{
			return  new LaneChangeQueueEntry (fromLane, toLane);
		}

		public VenerisLane SelectLessOccupiedLane (List<VenerisLane> lanes)
		{
			if (lanes.Count < 1) {
				return null;
			} else if (lanes.Count == 1) {
				return lanes [0];
			} else {
				int minOccupancy = int.MaxValue;
				int ri = -1;
				for (int i = 0; i < lanes.Count; i++) {

					if (lanes [i].registeredVehiclesList.Count < minOccupancy) {
						minOccupancy = lanes [i].registeredVehiclesList.Count;
						ri = i;
					}
				}


				return lanes [ri];
			}

		}
		public VenerisLane SelectRandomTargetLane(VenerisRoad startRoad, VenerisRoad nextRoad) {
			int clanes=GlobalRouteManager.Instance.GetNumberOfConnectedLanes(startRoad,nextRoad);
			if (clanes > 1) {
				int ri = SimulationManager.Instance.GetRNG ().Next (0, clanes);

				return GlobalRouteManager.Instance.GetFromLane (startRoad, nextRoad, ri);


			} else {
				//Log ("Lanes from " + startRoad.sumoId + " to " + nextRoad.sumoId + " = " + clanes);
				return GlobalRouteManager.Instance.GetFromLane (startRoad, nextRoad,0);

			}
		}

		public VenerisLane SelectLessOccupiedTargetLane (VenerisRoad startRoad, VenerisRoad nextRoad)
		{

			//TODO: if all the lanes are equally occupied, it always returns the first one. We should consider other alternatives, it is not the best for 
			//some paths. Maybe lessoccupied or random
			int clanes = GlobalRouteManager.Instance.GetNumberOfConnectedLanes (startRoad, nextRoad);
			//Log (1, "selectless. From  " + startRoad.sumoId + " to  " + nextRoad.sumoId + " has clanes=" + clanes);
			if (clanes > 1) {
				int minOccupancy = int.MaxValue;
				int ri = -1;
				for (int i = 0; i < clanes; i++) {
					VenerisLane lane = GlobalRouteManager.Instance.GetFromLane (startRoad, nextRoad, i);
					if (lane.registeredVehiclesList.Count < minOccupancy) {
						minOccupancy = lane.registeredVehiclesList.Count;
						ri = i;
						//Log (1, "ri=" + ri + "minOccupancy=" + minOccupancy);
					}
				}


				return GlobalRouteManager.Instance.GetFromLane (startRoad, nextRoad, ri);


			} else {
				//Log ("Lanes from " + startRoad.sumoId + " to " + nextRoad.sumoId + " = " + clanes);
				return GlobalRouteManager.Instance.GetFromLane (startRoad, nextRoad, 0);

			}
		}


		protected override void HandleLaneTag (Collider other)
		{
			base.HandleLaneTag (other);
			planNextMovementsPending = true;


		}

		public float LaneChangeOrOccupancyCostWithVehicles (AStarLaneNode node, AStarLaneNode neighbor)
		{
			//TODO: decide criteria: weighted sum...
			if (node.road == neighbor.road) {
				//This is a lane change 
				return LaneChangeCostWithVehicles (node.lane, neighbor.lane);
			} 
			//Use occupancy as cost
			return (float)(neighbor.lane.registeredVehiclesList.Count);
		}
		public float LaneChangeOrOccupancyCostWithDensity (AStarLaneNode node, AStarLaneNode neighbor)
		{
			//TODO: decide criteria: weighted sum...
			if (node.road == neighbor.road) {
				//This is a lane change 
				return LaneChangeCostWithDensity (node.lane, neighbor.lane);
			} 
			//Use occupancy as cost
			return (neighbor.lane.occupancy);
		}

		public float LaneChangeCostWithVehicles (VenerisLane start, VenerisLane target)
		{
			//TOOD: decide criteria, we could also check if there are traffic lights on the target lane...

			//Add also occupancy on the target lane
			float cost=(float) target.registeredVehiclesList.Count;
			if (start.paths [0].totalPathLength < 1.5f * vehicleTriggerColliderHalfSize.z) {
				return (cost + 3f * costForLaneChangeOnTooShortRoadsWithVehicles);
			}
			if (start.paths [0].totalPathLength < minimumPathLengthToWaitForLaneChange) {
				return (cost+costForLaneChangeOnTooShortRoadsWithVehicles);
			}
			return (cost+costForLaneChangeWithVehicles);
		}
		public float LaneChangeCostWithDensity (VenerisLane start, VenerisLane target)
		{
			//TOOD: decide criteria, we could also check if there are traffic lights on the target lane...

			//Add also occupancy on the target lane
			float cost= target.occupancy;
			if (start.paths [0].totalPathLength < 1.5f * vehicleTriggerColliderHalfSize.z) {
				return (cost + 3f * costForLaneChangeOnTooShortRoadsWithDensity);
			}
			if (start.paths [0].totalPathLength < minimumPathLengthToWaitForLaneChange) {
				return (cost+costForLaneChangeOnTooShortRoadsWithDensity);
			}

			//Include cost of internal stops to discourage routes
			//Log (target.sumoId+".target.paths [0].pathId=" + target.paths [0].pathId);
			/*if (target.endIntersection!=null) {
			PathConnector con=target.endIntersection.GetComponentInChildren<PathConnector>();
			ConnectionInfo ci = con.GetPathsConnectedTo (target.paths [0].pathId);
		
			//for (int i = 0; i < ci.Count; i++) {
				List<ConnectionInfo.PathDirectionInfo> cp = ci.connectedPaths;
				for (int j = 0; j < cp.Count; j++) {
					IntersectionPriorityInfo ipi = cp [j].p.GetComponent<IntersectionPriorityInfo> ();
					if (ipi.StopAtInternalPosition ()) {
						cost += 1f;
					}
				}
			//}
			}*/
			return (cost+costForLaneChangeWithDensity);
		}
	}
}


