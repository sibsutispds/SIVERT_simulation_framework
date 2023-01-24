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
	public class IntersectionBehaviourProvider : AIBehaviourProvider
	{

	
		public IntersectionInfo intersection = null;
		public GameObject junction = null;
		public List<long> affectedPaths = null;
		// Use this for initialization
		void Awake ()
		{
			intersection = GetComponentInParent<IntersectionInfo> ();

		}

		void Start ()
		{
			use = new Usage (0, UseFrequency.Always,int.MaxValue);


	
		}

		public void AddAffectedLane (long laneId)
		{
			if (affectedPaths == null) {
				affectedPaths = new List<long> ();

			}
			affectedPaths.Add (laneId);
		}
		public override void CheckBehaviourValidity (GameObject go)
		{
			base.CheckBehaviourValidity (go);
			IntersectionBehaviour[] istops = go.GetComponents<IntersectionBehaviour> ();


			for (int i = 0; i < istops.Length; i++) {
				if (istops [i].intersection == this.intersection) {
					AILogic ailogic=go.GetComponent<AILogic>();
					//ailogic.Log (10, "found=" +this.intersection.transform.root.name);
					List<VenerisRoad> fr= ailogic.routeManager.GetForwardRoads();

					if (fr != null) {
						//ailogic.Log (10, "forward=" + fr.Count);
						/*foreach (var item in fr) {
							ailogic.Log (10, item.sumoId);
						}*/
						int inroute = CheckIfIntersectionIsInRoute (fr);
						//ailogic.Log (10, "inroute=" + inroute);

						if (inroute < 0) {
							//Not valid at the moment, remove it
							//ailogic.Log (10, transform.root.name + " not valid");
							ailogic.RemoveBehaviour (istops [i]);
						}
					} else {
						//ailogic.Log ("Null roads");
						//Debug.Break ();
					}

				}
			}
		}

		public  bool HasTrafficLight(GameObject go) {

			long priority;
			Transform stopLine;
			ConnectionInfo.PathDirectionInfo pair = GetPlannedPath (go, out priority, out stopLine);
			if (pair != null) {
				if (pair.trafficLight != null) {
					return true;
				}
			}
			return false;
		}

		public bool SetBehaviourWithPlannedPath(GameObject go, out AIBehaviour newBehaviour, Transform stopLine, ConnectionInfo.PathDirectionInfo pair, long priorityPathId, List<Path> intPaths) {
			base.SetBehaviour (go,out newBehaviour);
			if (CheckUseLimit ()) {
				AILogic ailogic = go.GetComponent<AILogic> ();
				IntersectionBehaviour b = CreateBehaviour (go, ailogic, pair, priorityPathId, stopLine, intPaths);
				if (b != null) {

					DoSetBehaviour (go, b);
					newBehaviour = b;
					return true;
					//AILogic ailogic = go.GetComponent<AILogic> ();

				} else {
					ailogic.Log ("Intersection behaviour not available");
				}
			} else {
				Debug.Log ("No more uses");
			}
			newBehaviour = null;
			return false;
		}


		public override bool SetBehaviour (GameObject go, out AIBehaviour newBehaviour)
		{
			//Debug.Log (go.transform.parent.name+ "-- Checking IntersectionStop Behaviour " +transform.name);
			base.SetBehaviour (go,out newBehaviour);
			if (CheckUseLimit ()) {
//			
				long priority;
				Transform stopLine;
				ConnectionInfo.PathDirectionInfo pair = GetPlannedPath (go, out priority, out stopLine);

				AILogic ailogic = go.GetComponent<AILogic> ();

				if (pair != null) {
					IntersectionBehaviour b = CreateBehaviour (go, ailogic, pair, priority, stopLine);
					if (b != null) {
						
						DoSetBehaviour (go, b);
						newBehaviour = b;
						return true;
						//AILogic ailogic = go.GetComponent<AILogic> ();

					} else {
						ailogic.Log ("Intersection behaviour not available");
					}

				} else {
					//ailogic.Log ("Intersection "+intersection.name+" not in forward routes");
				}
			
			} else {
				Debug.Log ("No more uses");
			}
			newBehaviour = null;
			return false;

		}
		IntersectionBehaviour CreateBehaviour(GameObject go, AILogic ailogic, ConnectionInfo.PathDirectionInfo pair, long priorityPathId, Transform stopLine, List<Path> intPaths=null) {
			if (CheckAlreadyInObject (go)) {
				
				ailogic.Log ("Intersection already in component" + this.intersection.name);
				return null;
			}
			IntersectionBehaviourSelector s = new IntersectionBehaviourSelector (go, ailogic);

			s.pathIdForPriority = priorityPathId;
			s.priorityType = AIBehaviour.PriorityType.DistanceInRoute;
			s.Setintersection (intersection);
			s.SetStopLinePosition (stopLine, stopLine.GetComponent<BoxCollider>());
			s.SetPathConnector (stopLine.GetComponentInChildren<PathConnector> ());
			s.SetPlannedPath (pair);
			s.SetInternalPaths (intPaths);

			use.timesUsed += 1;

			return s.SelectAndCreateBehaviour();
		}
		void DoSetBehaviour(GameObject go, IntersectionBehaviour s) {
			go.GetComponent<AILogic> ().AddBehaviourToTaskList (s);
		}
		/*void StackForFutureUseInRoad(GameObject go, IntersectionStop s, Path p ) {
			Debug.Log (p.pathName);
			VenerisRoad road = p.GetComponentInParent<VenerisRoad> ();

			go.GetComponent<AILogic> ().AddBehaviourToRoadTaskList (road,s);
		}
		*/

		public bool CheckAlreadyInObject(GameObject go) {
			//TODO: it may happen that we have to cross several times this intersection and we may have seen a previous crossing. We could add fromRoad and toRoad to differentiate, or we shoudl think another way 
			IntersectionBehaviour[] ist = go.GetComponents<IntersectionBehaviour> ();
			foreach (IntersectionBehaviour s in ist) {
				if (s != null && s.intersection == this.intersection) {
					return true;
				} 
			}
			return false;
		}
		public ConnectionInfo.PathDirectionInfo GetPlannedPath(GameObject go, out long  priorityPathId, out Transform stopLine) {
			ConnectionInfo.PathDirectionInfo pair = null;
			//priority = float.MaxValue;
			priorityPathId =-1;
			stopLine = null;
			AILogic ailogic = go.GetComponent<AILogic> ();

			//Debug.Log ("Intersection seen :" + this.intersection.name+" "+transform.name);
			//First, just assign behaviour is path is in affected lane
			if (CheckPathInAffectedLanes (ailogic.routeManager.trackedPath.pathId)) {
				
				pair= ailogic.routeManager.GetNextPathIfIsInConnector (GetComponentInChildren<PathConnector> ());
				//priority = ailogic.routeManager.GetDistanceToEndOfCurrentPath ();
				priorityPathId=ailogic.routeManager.GetTrackedPath().pathId;
				stopLine = transform;

					//Debug.Log ("Path in affected lanes :" + this.intersection.name+" "+transform.name +"priorityPathId="+priorityPathId );


			} else  {
				//Otherwise, determine if this intersection is going to be traversed in the route
				List<VenerisRoad> fr= ailogic.routeManager.GetForwardRoads();
				if (fr != null) {
					int i = CheckIfIntersectionIsInRoute (fr);

					if (i >= 0) {
						List<PathConnector> cons = intersection.GetPathConnectors (fr [i], fr [i + 1]);

						foreach (PathConnector pc in cons) {
							pair = ailogic.routeManager.GetPathFromRoadIfIsInConnector (fr [i], pc);

							if (pair != null) {
								//priority = ailogic.routeManager.CheckEndDistanceIfPathIsInRouteForward (ailogic.routeManager.GetPathInRouteFromRoad (fr [i]));
								//priorityPathId = ailogic.routeManager.GetPathsInRouteFromRoad (fr [i]);
								priorityPathId = pair.p.pathId;
								stopLine = pc.transform.parent;
								return pair;
								
							}
						}
					
					}
				}
			

			}
			return pair;
		}
		public int CheckIfIntersectionIsInRoute(List<VenerisRoad> route) {
			for (int i = 0; i < route.Count-1; i++) {
				if (intersection.AreRoadsConnectedByIntersection (route [i], route [i + 1])) {
					return i;
				}
			}
			return -1;
		}
		public long CheckIfAffectedLaneIsInRoute(GameObject go) {
			AgentRouteManager rm = go.GetComponent<AILogic> ().routeManager;
			foreach (long l in affectedPaths) {
				if (rm.NumberOfHopsIfPathIsInRouteForward (l)>=0) {
					return l;
				}

			}
			return -1;
		}

		public bool CheckPathInAffectedLanes (long pathId)
		{
			//Check if the vehicle is on a lane affected by this intersection
			foreach (long l in affectedPaths) {
				if (pathId == l) {
					return true;
				}
			}
			return false;
		}
	}
}
