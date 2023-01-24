/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Veneris
{
	[System.Serializable]
	public class VehicleGenerationInfo {
		public enum SumoVehicleTypes
		{
			passenger
		};

	

		public int id;
		public SumoVehicleTypes type;
		public float departTime; 
		public string departLane; //Use sumo values and implement insertion in the manager
		public List<long> routes;
		public List<VenerisRoad> routeRoads;
		public List<long> strategicLaneChanges;
		public VehicleGenerationInfo(int id, SumoVehicleTypes type, float departTime, string departLane) {
			this.id = id;
			this.type = type;
			this.departTime = departTime;
			this.departLane= departLane;
			this.routes = new List<long> ();
		}
		public void AddPathToRoutes(long pathId) {
			routes.Add (pathId);
		}
		public void SetRoutesFromPathList(List<long> routes) {
			this.routes = routes;
		} 
		public void SetRouteRoads(List<VenerisRoad> roads) {
			this.routeRoads = roads;
		} 

		public void SetStrategicLaneChangesFromList(List<long> c) {
			this.strategicLaneChanges = c;
		}
	}
	public class SumoVehicleManager : VehicleManager
	{

		[System.Serializable]
		public struct VehicleInsertionEntry
		{
			public Path path;
			public Vector3 position;
			public Quaternion rotation;
			public int id;
			public bool triggerInsertionListeners;
			public List<VenerisRoad> routeRoads;
		}

		public List<VehicleInsertionEntry> insertionQueue=null;
		public List<VehicleGenerationInfo> vehicleGenerationList = null;
		public int inserted=0;
		public int insertIndex = 0;
		public bool useDifferentVehicleVisual=false;
		public int currentVehicleVisualIndex = 0;
		//public string[] vehicleVisuals = {"STDRSCar", "Variations/STDRSCar-C", "Variations/STDRSCar-Taxi", "Variations/STDRSCar-Police", "Variations/STDRSCar-Suv", "Variations/STDRSCar-Wagon" };
		public List<string> vehicleVisuals;
		public string vehiclePrefabName="GSCMVenerisCar_s90";
		public string[] vehicleVisualSuffixes = { "-C", "-Taxi", "-Police", "-Suv", "-Wagon" };
		//public string[] vehicleVisuals = {"STDRSCar", "Variations/STDRSCar-C",  "Variations/STDRSCar-Police", "Variations/STDRSCar-Suv", "Variations/STDRSCar-Wagon" };
		//public GameObject vehiclePrefab;
		public float scaleGenerationFrequency=1f;
		public float occupiedInsertionLocationDelay = 1f;
		public bool DoNotOverlapInfrastructure = false;
		//public Dictionary<long,Path> idToPathDictionary = null;
		//public Dictionary<long, AILogic> activeVehicleDictionary = null;
		private Coroutine scheduleCoroutine;
		private Coroutine checkLocationCoroutine;
		public Vector3 vehicleOccupyPositionSize;

		//Insert with Astart
		public float costForLaneChangeOnTooShortRoadsWithDensity= 3f;
		public float costForLaneChangeWithDensity = 0.2f;
		//Assuming maxDec=10 m/s2 (from our tests), and a (urban) maximum speed around v=28 m/s, we need v*v/(2*maxDec) m to stop, around 40 m.
		//After a lane change we should still have that 40 m availble before the next intersection to be able to stop.
		//So being conservative, assuming we need hlaf the length of the lane to make the change if the speed is high, we need 2*40 m minimum
		public float minimumPathLengthToWaitForLaneChange = 80f;



		public int arrivedVehicles = 0;
		public float minLengthToInsert = 7f;
		protected WaitForSeconds intervalInsertionLocationDelay = null;
		private int vehicleLayerMask;

		//Statistics
		protected float lastCollectedTime = 0;
		public WeightedAverage activeVehiclesStat=null;
		public WeightedAverage insertionQueueStat=null;
		public int removedAndReinserted = 0;
		public bool startInsertion=true;
		public int DisplacementCoef = 10;


		void Awake() {
			Debug.Log ("SumoVehicleManager::Awake()");
			if (!GlobalRouteManager.Instance.ready) {
				GlobalRouteManager.Instance.RegisterGlobalRouteManagerReadyListener (OnGlobalRouteManagerReady);
				startInsertion = false;
			}
			if (useDifferentVehicleVisual) {

				BuildVisualList ();
			}
		
		}
		public void OnGlobalRouteManagerReady() {
			startInsertion = true;
			
		}
		// Use this for initialization
		void Start ()
		{
			Debug.Log ("SumoVehicleManager::Start()");
		
			BuildDictionaries ();
			if (vehicleGenerationList != null ) {
				if (vehicleGenerationList.Count > 0) {
					activeVehicleDictionary = new Dictionary<int, AILogic> ();
					//StartCoroutine (ScheduleFirstInsertion());

				}
				//Use the double the vehicle size, except for the height, to check occupied position at insertion
				vehicleOccupyPositionSize = vehiclePrefab.transform.Find ("Agent/VehicleTrigger").GetComponent<BoxCollider> ().size;
				vehicleOccupyPositionSize.y = vehicleOccupyPositionSize.y * 0.5f;
				vehicleLayerMask = 1<<LayerMask.NameToLayer ("Vehicle");


				//Now schedule coroutine
				//scheduleCoroutine = StartCoroutine(ScheduleInsertion ());
			}
			maxActiveVehicles = 0;
			activeVehiclesStat = new WeightedAverage ();
			activeVehiclesStat.ResetValues ();
			insertionQueueStat = new WeightedAverage ();
			insertionQueueStat.ResetValues ();
			lastCollectedTime = Time.time;
			intervalInsertionLocationDelay = new WaitForSeconds (occupiedInsertionLocationDelay);
			insertionQueue = new List<VehicleInsertionEntry> ();
			SimulationManager.Instance.RegisterEndSimulationListener (OnEndSimulation);
		
		}
	
		public override string GetInfoText ()
		{
			string m= base.GetInfoText ();
			m += ":inserted="+inserted+":arrived="+arrivedVehicles+":total="+vehicleGenerationList.Count+":avNumberActive="+activeVehiclesStat.ComputeWeightedAverage()+":insertionQueue="+insertionQueue.Count+":avInsertionQueue="+insertionQueueStat.ComputeWeightedAverage()+":removedAndReinserted="+removedAndReinserted;
			return m;
		}

		public void OnEndSimulation() {
			SimulationManager.Instance.GetGeneralResultLogger ().Record (GetInfoText());

		}
		IEnumerator CheckInsertionLocationCoroutine() {
			while (true) {
				
				yield return intervalInsertionLocationDelay;


				for (int i = insertionQueue.Count-1; i>=0; i--) {
					if (CheckInsertionLocationVehicleOccupied (insertionQueue[i].position,insertionQueue[i].rotation)) {
						//Debug.Log (Time.time+"InsertionQueue.Insertion of " + insertionQueue [i].id + "delayed.");

					} else {
						int id = insertionQueue [i].id;
						InsertVehicle (insertionQueue[i].position,insertionQueue[i].rotation, id, insertionQueue[i].path,insertionQueue[i].routeRoads);
						if (insertionQueue [i].triggerInsertionListeners) {
							
							TriggerInsertionListeners (vehicleGenerationList [id],id);


						}
						//Collect before removing
						insertionQueueStat.CollectWithLastTime(insertionQueue.Count);
						insertionQueue.RemoveAt (i);
						//StopCoroutine (checkLocationCoroutine);


					}
					
				}
				if (insertionQueue.Count == 0) {
					StopCoroutine (checkLocationCoroutine);
					checkLocationCoroutine = null;
					break;
				}

			}
		}


		// void FixedUpdate() {
		// 	if (startInsertion) {
		// 		//Have to use fixedupdate to insert at exact times, otherwise, with coroutines, there is a lag
		// 		if (insertIndex < vehicleGenerationList.Count) {
		// 			while (vehicleGenerationList [insertIndex].departTime <= Time.time) {
		// 				//Debug.Log ("insertIndex=" + insertIndex + "departTime=" + vehicleGenerationList [insertIndex].departTime);
		// 				TryInsertVehicle ();
		// 				insertIndex++;
		// 				if (insertIndex >= vehicleGenerationList.Count) {
		// 					break;
		// 				}
		//
		// 			}
		// 		}
		// 	}
		// }
		
		void FixedUpdate() {
			if (startInsertion) {
				//Have to use fixedupdate to insert at exact times, otherwise, with coroutines, there is a lag
				if (insertIndex < vehicleGenerationList.Count) {
					while (Mathf.Abs(vehicleGenerationList [insertIndex].departTime - Time.time) <= 1) {
						//Debug.Log ("insertIndex=" + insertIndex + "departTime=" + vehicleGenerationList [insertIndex].departTime);
						TryInsertVehicle ();
						insertIndex++;
						if (insertIndex >= vehicleGenerationList.Count) {
							break;
						}

					}
				}
			}
		}
		IEnumerator ScheduleInsertion() {
			while (true) {
				
				if (insertIndex == vehicleGenerationList.Count) {
					StopCoroutine (scheduleCoroutine);
					 break;
				}


				//yield return new WaitForSeconds ((vehicleGenerationList [inserted].departTime-vehicleGenerationList [inserted-1].departTime) * scaleGenerationFrequency);
				//float nt=vehicleGenerationList [insertIndex].departTime* scaleGenerationFrequency-Time.time;
				//Debug.Log ("insertIndex=" + insertIndex + "nt=" + nt);
				yield return new WaitForSeconds (vehicleGenerationList [insertIndex].departTime* scaleGenerationFrequency-Time.time);
				while (vehicleGenerationList [insertIndex].departTime <= Time.time) {
					TryInsertVehicle ();
					insertIndex++;
				}
			


			}

		}

		public override void RemoveAndReinsert(VehicleInfo info, List<VenerisRoad> routeRoads) {
			int id = info.vehicleId;
			Debug.Log (Time.time + ": Removing and reinserting vehicle " + id);
			RemovedVehicle (info);
			TryInsertVehicle (routeRoads, id, false);
			removedAndReinserted++;
			
		}


		public override void RemovedVehicle(VehicleInfo vid) {
			
			TriggerRemoveListeners (vid);
			//Collect before insertion and removal
			activeVehiclesStat.Collect ((float)activeVehicleDictionary.Count, Time.time - lastCollectedTime);
			lastCollectedTime = Time.time;

			activeVehicleDictionary.Remove (vid.vehicleId);

		}
		public override void EndOfRouteReached (VehicleInfo info)
		{
			base.EndOfRouteReached (info);
			RemovedVehicle (info);
			arrivedVehicles++;
			if (activeVehicleDictionary.Count == 0 && insertIndex == vehicleGenerationList.Count) {
				//Call end simulation because there is no more vehicles
				SimulationManager.Instance.EndSimulation ();

			}

		}
		public override AILogic IsVehicleActive (int vid)
		{
			AILogic l;
			 activeVehicleDictionary.TryGetValue (vid,out l);
			return l;

		}
		bool CheckInsertionLocationInfrastructureFree(Transform point, Quaternion orientation) {
			//Check only for infrastrure tags
			Collider[] infra=Physics.OverlapBox(point.position,vehicleOccupyPositionSize, orientation);
			ExtDebug.DrawBox (point.position, vehicleOccupyPositionSize, orientation, Color.red);
			for (int i = 0; i < infra.Length; i++) {
				if (infra [i].CompareTag ("Intersection") || infra [i].CompareTag ("InternalStopTrigger") || infra [i].CompareTag ("ConnectorTrigger")) {
					if (point.InverseTransformPoint (infra [i].transform.position).z > 0) {
						//It is in front of vehicle
						return false;
					}

				}
				
			}
			return true;

		}
		bool CheckInsertionLocationVehicleOccupied(Vector3 position, Quaternion orientation) {
			//Check only for vehicles at the moment


			return Physics.CheckBox (position, vehicleOccupyPositionSize, orientation, vehicleLayerMask);

		}





		bool TryInsertVehicle (List<VenerisRoad> routeRoads=null, int id=-1, bool triggerInsertionListeners=true)
		{

			//Get the initial path

			if (routeRoads == null) {
				routeRoads = vehicleGenerationList [insertIndex].routeRoads;
			} 
			//Path initialPath = SelectLane (routeRoads);
			Path initialPath = SelectLaneWithAStar (routeRoads);


			if (id < 0) {
				//A new vehicle
				// id = insertIndex;
				id = vehicleGenerationList[insertIndex].id;

			}
//			Debug.Log ("Inserting vehicle=" + id + "at time=" + Time.time + " with depart time=" + vehicleGenerationList [insertIndex].departTime);
			bool isActive = vehiclePrefab.activeSelf;
			if (isActive) {
				vehiclePrefab.SetActive (false);
			}


			Quaternion rotation = Quaternion.identity;
			Vector3 startPosition = Vector3.zero;




			// List<GameObject> nodesType = initialPath.GetNodes();
			// Transform startOfPath = nodesType[id].transform;
			Transform startOfPath = initialPath.GetFirstNode ().transform;
			
			rotation = Quaternion.LookRotation (initialPath.FindClosestPointInfoInPath (startOfPath).tangent);

			//Align the backnumper with the start of path to avoid overlap intersections
			startOfPath.rotation = rotation;
			float blength = vehiclePrefab.transform.Find ("BackBumper").transform.position.z;
			float koef = id * DisplacementCoef;
			startPosition = startOfPath.TransformPoint (0f, 0f, (insertIndex+1)*blength * -1.05f + koef); //Minus sign because the backbumper z position is negative
			startPosition.y += 0.6f;
				




			if (CheckInsertionLocationVehicleOccupied (startPosition, rotation)) {
				VehicleInsertionEntry entry = new VehicleInsertionEntry ();
				entry.position = startPosition;
				entry.rotation = rotation;
				entry.path = initialPath;
				entry.id = id;
				entry.triggerInsertionListeners = triggerInsertionListeners;
				entry.routeRoads = routeRoads;

				//Collect stats before inserting
				insertionQueueStat.CollectWithLastTime(insertionQueue.Count);
				insertionQueue.Add (entry);
				if (checkLocationCoroutine == null) {
					checkLocationCoroutine = StartCoroutine (CheckInsertionLocationCoroutine ());
				}
				//Debug.Log (Time.time + ": Delayed insertion of vehicle: " + insertIndex + " because start position is occupied :" + (initialPath.GetComponent<VenerisLane>().sumoId)); 
				//To keep inserting other vehicles

				
			} else {
				InsertVehicle (startPosition, rotation, id, initialPath, routeRoads);
				if (triggerInsertionListeners) {
					TriggerInsertionListeners (vehicleGenerationList [id], id);

				}
	
			}


			return true; 
				

					
		}

		public Path SelectLaneWithAStar(List<VenerisRoad> routeRoads) {
			if (routeRoads.Count > 2) {
				Path path = null;
				//Avoid unrealistically short roads provided by Sumo, to avoid problems due to the length of the vehicle
				//If a road is shorter than the vehicle, there will be indefinition in the current road
				VenerisRoad startRoad = null;
				int laneIndex = -1;
				for (int i = 0; i < routeRoads.Count; i++) {

					int end = i + 2;
					if (end >= routeRoads.Count) {
						return SelectLane (routeRoads);
					}
					startRoad = routeRoads [i];
					VenerisLane endLane = SelectLessOccupiedTargetLane (routeRoads [end -1], routeRoads [end ]);
					float minCost = float.MaxValue;
					laneIndex = -1;
					for (int j = 0; j < startRoad.lanes.Length; j++) {
						if (startRoad.lanes [j].paths [0].totalPathLength >= minLengthToInsert) {
							AStarPath<AStarLaneNode> mcPath = GlobalRouteManager.Instance.GetMinimumCostPathOnRoads (startRoad.lanes [j], endLane, LaneChangeOrOccupancyCostWithDensity, routeRoads);
							if (mcPath.TotalCost <= minCost) {
								laneIndex = j;
								minCost = mcPath.TotalCost;
							}
							//Debug.Log ("startRoad=" + startRoad.sumoId + "lane=" + j +"endRoad="+routeRoads[end].sumoId+ "cost=" + mcPath.TotalCost);
							//LogAstarPath (mcPath);
						}

					}
					if (laneIndex >= 0) {
						//Remove previous edges from routes
						if (i > 0) {
							routeRoads.RemoveRange (0, i);
							if (startRoad != routeRoads [0]) {
								Debug.LogError ("First road does not coincide with starting road for vehicle " + vehicleGenerationList [insertIndex].id + " startroad=" + startRoad.sumoId + " first road=" + vehicleGenerationList [insertIndex].routeRoads [0].sumoId);
							}
						}
						break;
					}
				}

				return startRoad.lanes [laneIndex].paths [0];
			} else {
				return SelectLane (routeRoads);
			}

			
			

		}
		public void LogAstarPath (AStarPath<AStarLaneNode> path)
		{

			Debug.Log ( "Total cost due to path=" + path.TotalCost);

			int steps = 0;

			AStarPath<AStarLaneNode> pointer = path.PreviousSteps;
			if (pointer != null) {
				Debug.Log ("step " + steps + ": " + path.LastStep.lane.sumoId + "cost=" + (path.TotalCost - pointer.TotalCost));
			}

			while (pointer != null) {
				steps++;

					Debug.Log ("step " + steps + ": " + pointer.LastStep.lane.sumoId + ". Cost=" + pointer.TotalCost);


				pointer = pointer.PreviousSteps;

			}

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
		public float LaneChangeCostWithDensity (VenerisLane start, VenerisLane target)
		{
			//TOOD: decide criteria, we could also check if there are traffic lights on the target lane...

			//Add also occupancy on the target lane
			float cost= target.occupancy;
			if (start.paths [0].totalPathLength < 1.5f * vehicleOccupyPositionSize.z) {
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

		public VenerisLane SelectLessOccupiedTargetLane (VenerisRoad startRoad, VenerisRoad nextRoad)
		{

			//TODO: if all the lanes are equally occupied, it always returns the first one. We should consider other alternatives, it is not the best for 
			//some paths. Maybe lessoccupied or random
			//Debug.Log("startRoad="+startRoad.sumoId+"nextRoad="+nextRoad.sumoId);
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
		public Path SelectLane(List<VenerisRoad> routeRoads) {
			//TODO: SUMO insertion modes not implemented. Note that "best" is not like its SUMO counterpart

				Path path = null;
				//Avoid unrealistically short roads provided by Sumo, to avoid problems due to the length of the vehicle
				//If a road is shorter than the vehicle, there will be indefinition in the current road
				VenerisRoad startRoad  = null;
				int laneIndex = -1;
				for (int i = 0; i <routeRoads.Count; i++) {
								
					startRoad = routeRoads [i];
					int minOccupancy =int.MaxValue;
					laneIndex = -1;
					for (int j= 0; j < startRoad.lanes.Length; j++) {
						if (startRoad.lanes [j].paths [0].totalPathLength >= minLengthToInsert) {
							if (startRoad.lanes [j].registeredVehiclesList.Count < minOccupancy) {
								minOccupancy = startRoad.lanes [j].registeredVehiclesList.Count;
								laneIndex = j;
								//Debug.Log (vehicleGenerationList [insertIndex].id + " selected lane " + i + " with occupancy " + minOccupancy); 
							}
						}

					}
					if (laneIndex >= 0) {
						//Remove previous edges from routes
						if (i > 0) {
							routeRoads.RemoveRange (0, i);
							if (startRoad != routeRoads [0]) {
								Debug.LogError ("First road does not coincide with starting road for vehicle " + vehicleGenerationList [insertIndex].id + " startroad=" + startRoad.sumoId + " first road=" + vehicleGenerationList [insertIndex].routeRoads [0].sumoId);
							}
						}
						break;
					}

				}




				//Now we have the index, get the path
				/*for (int j = 0; j < startRoad.lanes [laneIndex].paths.Count; j++) {
					if (startRoad.lanes [laneIndex].paths [j].pathId == vehicleGenerationList [insertIndex].routes [0]) {
						//It is the same paht, just return it
						//Debug.Log (vehicleGenerationList [insertIndex].id + " Uses the original path " + vehicleGenerationList [insertIndex].routes [0]);
						return idToPathDictionary [vehicleGenerationList [insertIndex].routes [0]];
					}
				}*/
				//TODO:: check this. Just select the first path
				//path = idToPathDictionary[startRoad.lanes [laneIndex].paths [0].pathId];
				//Now we have to add the path to the routes and insert a path change if necessary

				/*if (vehicleGenerationList [insertIndex].strategicLaneChanges [0] == -1) {
					
					long prevId = vehicleGenerationList [insertIndex].routes [0];
					vehicleGenerationList [insertIndex].routes[0]= startRoad.lanes [laneIndex].paths [0].pathId;
					vehicleGenerationList [insertIndex].strategicLaneChanges[0]=prevId;
					//Debug.Log (vehicleGenerationList [insertIndex].id + "Changes first  path to " + startRoad.lanes [laneIndex].paths [0].pathId + " and change to original " + prevId);
				} else {
					//Change the first route path
					vehicleGenerationList [insertIndex].routes [0] = startRoad.lanes [laneIndex].paths [0].pathId;
					//Debug.Log (vehicleGenerationList [insertIndex].id + "changes first path to " + startRoad.lanes [laneIndex].paths [0].pathId);
					if (vehicleGenerationList [insertIndex].strategicLaneChanges [0] == startRoad.lanes [laneIndex].paths [0].pathId) {
						//check if there was already a strategic path change to this lane and remove it, otherwise keep the change
						vehicleGenerationList [insertIndex].strategicLaneChanges [0] = -1;
						//Debug.Log (vehicleGenerationList [insertIndex].id + " removes first change");
					} 
				} */
			//TODO:: check this. Just select the first path
			return startRoad.lanes [laneIndex].paths [0];
				

		}
		public GameObject SelectVehicleWithCycle() {
			if (currentVehicleVisualIndex == vehicleVisuals.Count) {
				currentVehicleVisualIndex = 0;
			}
			//Debug.Log("currentVehicleVisualIndex=Assets/Resources/Prefabs/Vehicles/"+vehicleVisuals[currentVehicleVisualIndex]+".prefab");

			GameObject v;
			#if UNITY_EDITOR
			v= AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Resources/Prefabs/Vehicles/"+vehicleVisuals[currentVehicleVisualIndex]+".prefab");
			currentVehicleVisualIndex++;
			if (v==null) {
				Debug.Log("Null model");
				return null;
			} else {
				return v;
			}
			#else
			v= Resources.Load ("Prefabs/Vehicles/"+vehicleVisuals[currentVehicleVisualIndex]) as GameObject;
			currentVehicleVisualIndex++;
			return v;
			#endif

		}
		public void BuildVisualList() {
			if (vehicleVisuals == null) {
				vehicleVisuals = new List<string> ();
			}
			if (vehicleVisuals.Count == 0) {

				vehicleVisuals.Add (vehiclePrefabName);
				for (int i = 0; i < vehicleVisualSuffixes.Length; i++) {
					vehicleVisuals.Add ("Variations/"+vehiclePrefabName+vehicleVisualSuffixes[i]);
				}

			}
			
		}
		protected AILogic InsertVehicle(Vector3 position, Quaternion orientation, int id, Path initialPath, List<VenerisRoad> routeRoads) {
			GameObject vehicle;
			if (useDifferentVehicleVisual) {
				
				vehiclePrefab = SelectVehicleWithCycle ();

			}
				vehicle = Instantiate (vehiclePrefab, position, orientation);
			
			//Debug.Log (inserted);
			//Debug.Log (vehicleGenerationList [inserted].routes);

			AgentRouteManager routeManager = vehicle.GetComponentInChildren<AgentRouteManager> ();
			//routeManager.SetRoute (vehicleGenerationList [id].routes);
			routeManager.SetIdToPathDictionary (idToPathDictionary);
			routeManager.SetPathToLaneDictionary (pathToLaneDictionary);
			routeManager.SetRouteRoads (routeRoads);
			//routeManager.SetStrategicLaneChanges (vehicleGenerationList [id].strategicLaneChanges);
			routeManager.SetStartPath (initialPath);
			vehicle.GetComponent<VehicleInfo> ().vehicleId = id;
			//Debug.Log ("Inserting Vehicle " + index + " at t=" + Time.time + "(" + Time.time / scaleGenerationFrequency + ")");
			AILogic ailogic = vehicle.GetComponentInChildren<AILogic> ();
			ailogic.SetVehicleManager (this);


			activeVehiclesStat.Collect ((float) activeVehicleDictionary.Count, Time.time - lastCollectedTime);
			lastCollectedTime = Time.time;
			activeVehicleDictionary.Add (id, ailogic);
		
			vehicle.SetActive (true);
			//Now perform actions after awake in the prefab components has been called

			ailogic.SetDisableOnArrivingEndOfRoute ();
			inserted++;

			if (activeVehicleDictionary.Count > maxActiveVehicles) {
				maxActiveVehicles = activeVehicleDictionary.Count ;
			}

			
			return ailogic;
			
		}
		public void AddVehicleGenerationInfo(VehicleGenerationInfo info) {
			if (vehicleGenerationList == null) {
				vehicleGenerationList = new List<VehicleGenerationInfo> ();
			}
			vehicleGenerationList.Add (info);
		}
		protected void BuildDictionaries() {
			Debug.Log ("Building dictionaries in SumoVehicleManager");
			idToPathDictionary = new Dictionary<long, Path> ();
			pathToLaneDictionary = new Dictionary<Path, VenerisLane> ();
			Path[] paths = FindObjectsOfType<Path> ();
			foreach (Path p in paths) {
				if (!p.pathName.Equals ("Director Road Path")) {
					idToPathDictionary.Add (p.pathId, p);
					VenerisLane lane = p.GetComponent<VenerisLane> ();
					if (lane != null) {
						pathToLaneDictionary.Add (p, lane);
					}
				}

			}
		}
	}
}
