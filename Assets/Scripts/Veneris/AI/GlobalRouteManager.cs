/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Veneris
{
	public class GlobalRouteManager : Singleton<GlobalRouteManager>
	{

		public List<SumoConnection> sumoConnectionList = null;
		protected Dictionary<VenerisRoad,VenerisRoadConnectionEntry> roadConnectionDictionary=null;
		protected List<AStarLaneNode> globalLaneGraph;
		protected Dictionary<VenerisLane,AStarLaneNode> laneNodeDictionary;
		public bool ready=false;
		public delegate void GlobalRouteManagerReady();
		protected GlobalRouteManagerReady readyListeners=null;

		public void RegisterGlobalRouteManagerReadyListener(GlobalRouteManagerReady l) {
			readyListeners += l;
		}

		void Awake () {
			//Create dictionary to group connections
			roadConnectionDictionary = new Dictionary<VenerisRoad, VenerisRoadConnectionEntry>();
			laneNodeDictionary = new Dictionary<VenerisLane, AStarLaneNode> ();
			ready = false;


		}
		void Start() {
			FillDictionary ();
			globalLaneGraph= CreateGlobalLaneGraph();
			ready = true;
			if (readyListeners != null) {
				readyListeners ();
			}
			
		}
		public void SetSumoConnectionList(List<SumoConnection> l) {
			sumoConnectionList = l;
		}

		public void FillDictionary() {
			Debug.Log ("[GlobalRouteMangager] Filling dictionaries");
			if (sumoConnectionList != null) {
				for (int i = 0; i < sumoConnectionList.Count; i++) {
					if (sumoConnectionList [i].internalLane == false) {
						VenerisRoadConnectionEntry e= null;
						//Debug.Log("sumoConnectionList [i].fromRoad=" + sumoConnectionList [i].fromRoad);
						if (roadConnectionDictionary.ContainsKey (sumoConnectionList [i].fromRoad)) {
							
							e = roadConnectionDictionary [sumoConnectionList [i].fromRoad];
						} else {
							e = new VenerisRoadConnectionEntry ();
							roadConnectionDictionary.Add (sumoConnectionList [i].fromRoad, e);

						}

						List<Path> ipaths = new List<Path> ();
						List<SumoConnection> iconns = new List<SumoConnection> ();
						if (sumoConnectionList [i].via != null) {
							CreateInternalPathList (sumoConnectionList [i].via, ipaths, iconns);
						}
					
					
						e.AddConnectionEntry ( sumoConnectionList [i].toRoad,sumoConnectionList [i].fromLane, sumoConnectionList [i].toLane, ipaths, iconns);

					}
				}
			}
			Debug.Log ("[GlobalRouteMangager] roadConnectionDictionary.Count="+roadConnectionDictionary.Count);

		}
		protected void CreateInternalPathList(string via, List<Path> paths, List<SumoConnection> sumoConns) {
			//Recursively fill the paths
			for (int i = 0; i < sumoConnectionList.Count; i++) {
				if (sumoConnectionList [i].fromSumoId.Equals (via)) {
					paths.Add (sumoConnectionList [i].internalPath);
					sumoConns.Add (sumoConnectionList [i]);
					if (sumoConnectionList [i].via != null) {
						CreateInternalPathList (sumoConnectionList [i].via, paths, sumoConns);
					} else {
						return;
					}
				}
			}
		}

		public List<AStarLaneNode> CreateGlobalLaneGraph() {
			Debug.Log ("Creating global lane network graph");
			List<AStarLaneNode> graph = new List<AStarLaneNode> ();
			//First pass, add connected lanes as neighbors
			for (int i = 0; i < sumoConnectionList.Count; i++) {
				if (sumoConnectionList [i].internalLane) {
					continue;
				}
				AStarLaneNode node;
				AStarLaneNode neighbor;

				if (!laneNodeDictionary.TryGetValue (sumoConnectionList [i].fromLane, out node)) {
					node = new AStarLaneNode (sumoConnectionList [i].fromRoad, sumoConnectionList [i].fromLane);
					laneNodeDictionary.Add (sumoConnectionList [i].fromLane, node);

					graph.Add (node);
				}
				if (!laneNodeDictionary.TryGetValue (sumoConnectionList [i].toLane, out neighbor)) {
					neighbor = new AStarLaneNode (sumoConnectionList [i].toRoad, sumoConnectionList [i].toLane);
					laneNodeDictionary.Add (sumoConnectionList [i].toLane, neighbor);

					graph.Add (neighbor);
				}
				node.AddNeighbor (neighbor);

			}

			//Second pass, add lanes on the same road as neighbors
			VenerisRoad[] roads=GameObject.FindObjectsOfType<VenerisRoad>();
			for (int j = 0; j < roads.Length; j++) {
				for (int k = 0; k < roads[j].lanes.Length; k++) {
					if (laneNodeDictionary.ContainsKey(roads[j].lanes [k])) { //Some roads or lanes may not be connected
						AStarLaneNode laneNode = laneNodeDictionary [roads[j].lanes [k]];
						AddLanesOnRoadAsNeighbors (laneNode, roads [j].lanes);
					}
				}

			}
			Debug.Log ("Graph.Count=" + graph.Count);
			return graph;
		}

		public void AddLanesOnRoadAsNeighbors(AStarLaneNode node, VenerisLane[] lanes) {

			//All lanes are neighbors, result in multilanechanges
			/*for (int i = 0; i < lanes.Length; i++) {
				if (node.lane == lanes [i]) {
					continue;
				} else {
					AStarLaneNode neighbor=laneNodeDictionary[lanes [i]];
					node.AddNeighbor (neighbor);
				}
			}*/

			//Only make adjacent roads neighbors, to add a cost for every single lane change
			int index=0;
			for (int i = 0; i < lanes.Length; i++) {
				if (node.lane == lanes [i]) {
					index=i;
					break;

				}
			}
			if (index > 0) {
				if (laneNodeDictionary.ContainsKey(lanes [index-1])) {
					AStarLaneNode neighbor = laneNodeDictionary [lanes [index-1]];
					node.AddNeighbor (neighbor);
				}
			}
			if (index < lanes.Length - 1) {
				if (laneNodeDictionary.ContainsKey (lanes [index + 1])) {
					AStarLaneNode neighbor = laneNodeDictionary [lanes [index + 1]];
					node.AddNeighbor (neighbor);
				}
			}

		}

		public List<Path> GetPathsFromLaneToLane(VenerisRoad fromRoad, VenerisRoad toRoad,VenerisLane fromLane, VenerisLane toLane) {
			return roadConnectionDictionary [fromRoad].GetPathsFromLaneToLane(toRoad,fromLane,toLane);
		}
		public int GetNumberOfConnectedLanes(VenerisRoad fromRoad, VenerisRoad toRoad) {
			//Debug.Log ("fromRoad=" + fromRoad + "toRoad=" + toRoad);
			return roadConnectionDictionary [fromRoad].GetNumberOfConnectedLanes (toRoad);

		}
		public List<Path> GetPathsFromLane(VenerisRoad fromRoad, VenerisRoad toRoad,VenerisLane fromLane) {
			return roadConnectionDictionary [fromRoad].GetPathsFromLane(toRoad,fromLane);
		}
		public List<Path> GetPathsFromLane(VenerisRoad fromRoad, VenerisRoad toRoad,int fromLaneIndex) {
			return roadConnectionDictionary [fromRoad].GetPathsFromLane(toRoad,fromLaneIndex);
		}
		public VenerisLane GetFromLane(VenerisRoad fromRoad, VenerisRoad toRoad,int fromLaneIndex ) {
			return roadConnectionDictionary [fromRoad].GetFromLane(toRoad,fromLaneIndex);
		}
		public List<VenerisLane> GetOutcomingLanes(VenerisRoad fromRoad, VenerisRoad toRoad,VenerisLane fromLane) {
			return roadConnectionDictionary [fromRoad].GetOutcomingLanes (toRoad, fromLane);
		}

		public bool IsLaneConnected(VenerisLane lane, VenerisRoad fromRoad, VenerisRoad toRoad) {
			for (int i = 0; i < GetNumberOfConnectedLanes (fromRoad, toRoad); i++) {
				if (GetFromLane (fromRoad, toRoad, i) == lane) {
					return true;
				}
			}
			return false;
		}

		public AStarPath<AStarLaneNode> GetMinimumCostPathOnRoads(VenerisLane start, VenerisLane destination, List<VenerisRoad> roads) {
			//Debug.Log ("getmin=" + start.sumoId + "dest=" + destination.sumoId);

			return AStarAlgorithm.FindPathOnRouteRoads (laneNodeDictionary[start], laneNodeDictionary[destination],  LaneChangeOrOccupancyCost,  DijkstraEstimate,roads);
		}
		public AStarPath<AStarLaneNode> GetMinimumCostPathOnRoads(VenerisLane start, VenerisLane destination, Func<AStarLaneNode, AStarLaneNode, float> distanceFunc,	 List<VenerisRoad> roads) {
			//Debug.Log ("getmin=" + start.sumoId + "dest=" + destination.sumoId);

			return AStarAlgorithm.FindPathOnRouteRoads (laneNodeDictionary[start], laneNodeDictionary[destination],  distanceFunc,  DijkstraEstimate,roads);
		}

		public AStarPath<AStarLaneNode> GetMinimumCostPathOnRoads(VenerisLane start, VenerisLane destination, Func<AStarLaneNode, AStarLaneNode, float> distanceFunc,	Func<AStarLaneNode, float> estimateFunc, List<VenerisRoad> roads) {
			//Debug.Log ("getmin=" + start.sumoId + "dest=" + destination.sumoId);

			return AStarAlgorithm.FindPathOnRouteRoads (laneNodeDictionary[start], laneNodeDictionary[destination],  distanceFunc,  estimateFunc,roads);
		}


		/*public List<AStarLaneNode> CreateAStarGraphFromRouteRoads(List<VenerisRoad> roads) {
			List<AStarLaneNode> graph = new List<AStarLaneNode> ();
			//Each  lane is a node
			//For each lane, look if there are paths within the provided roads
			Debug.Log("Creating graph");
			for (int i = 0; i < roads.Count-1; i++) {
				Debug.Log("Creating graph. road="+roads[i].sumoId);
				for (int j = 0; j < roads[i].lanes.Length; j++) {
					Debug.Log("Creating graph. lane="+roads[i].lanes[j].sumoId);
					AStarLaneNode node = new AStarLaneNode (roads [i], roads [i].lanes [j]);
					AddRoadLanesAsNeighbors (node, roads [i].lanes);
					AddConnectedLanesAsNeighbors (node, roads [i + 1]);
					graph.Add (node);
				}
			}
			//For the last road, add lanes
			for (int i = 0; i < roads [roads.Count -1].lanes.Length; i++) {
				Debug.Log ("Creating graph. lane=" + roads [roads.Count -1].lanes [i].sumoId);
				AStarLaneNode node = new AStarLaneNode (roads [roads.Count -1], roads [roads.Count -1].lanes [i]);
				AddRoadLanesAsNeighbors (node,roads [roads.Count -1].lanes);
			}

			Debug.Log ("grapfh.count=" + graph.Count);
			return graph;
		}
		public void AddRoadLanesAsNeighbors(AStarLaneNode node, VenerisLane[] lanes) {
			for (int i = 0; i < lanes.Length; i++) {
				if (lanes [i] == node.lane) {
					continue;
				} else {
					Debug.Log("Adding lane neighbor to ="+node.lane.sumoId+"nlane="+lanes [i].sumoId);
					node.AddNeighbor (new AStarLaneNode (node.road, lanes [i]));
				}
			}
		}
		public void AddConnectedLanesAsNeighbors(AStarLaneNode node, VenerisRoad nextRoad) {
			List<VenerisLane> conn = GetOutcomingLanes (node.road, nextRoad, node.lane);
			for (int i = 0; i < conn.Count; i++) {
				Debug.Log("Adding connected neighbor to ="+node.lane.sumoId+"nlane="+conn [i].sumoId);
				node.AddNeighbor (new AStarLaneNode (nextRoad, conn[i]));
			}
		}
		*/

		public float DijkstraEstimate(AStarLaneNode node) {
			//If estimate is 0, we get Dijkstra algorithm
			return 0f;
		}
		public float LaneChangeOrOccupancyCost(AStarLaneNode node,  AStarLaneNode neighbor) {
			
			if (node.road == neighbor.road) {
				//This is a lane change 
				return 10f;
			}
			//Use occupancy as cost
			return (float) ( neighbor.lane.registeredVehiclesList.Count);
		}
	}
}
