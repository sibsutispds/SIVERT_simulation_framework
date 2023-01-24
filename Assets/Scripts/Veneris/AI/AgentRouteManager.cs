/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//using UnityEditor;
namespace Veneris
{

	//TODO: should be redesigned. In addition, some global RouteManager would be useful
	public class AgentRouteManager : MonoBehaviour
	{



		// Use this for initialization
	
		public List<long> routeIds;
		public List<Path> routePaths;
		public List<long> changesList;
		public List<VenerisRoad> routeRoads;
		public Path startPath = null;
		//protected Path currentPath = null;

		[SerializeField]
		protected Path _trackedPath = null;
		public Path trackedPath {
			get { return _trackedPath; }
			protected set { _trackedPath = value; }
		}
		[SerializeField]
		protected Path _lookAtPath = null;
		public Path lookAtPath {
			get { return _lookAtPath; }
			protected set { SetLookAtPath(value); }
		}
		public Dictionary<long,Path> idToPathDictionary = null;
		public Dictionary<Path, VenerisLane> pathToLaneDictionary = null;
		public AILogic ailogic = null;
		public LineRenderer routeRenderer = null;
		public GameObject routeRendererPrefab = null;
		protected int trackedPathIndex;
		//public int currentRoadIndex;

		protected virtual void Awake ()
		{

			if (idToPathDictionary == null) {
				BuildDictionaries ();
			}

		

			//currentPath = idToPathDictionary [routePaths [0]];
			//currentPathIndex = 0;


			//TODO: be careful here, tracked path may not be the path our vehicle is on at the moment...
			SetCurrentPath(StartPath());
			//_trackedPath = StartPath();
			//trackedPathIndex = routePaths.IndexOf(_trackedPath.pathId);
			//VenerisRoad cr = idToPathDictionary [routePaths [0]].GetComponentInParent<VenerisRoad> ();
			/*if (cr != null) {
				currentRoadIndex = routeRoads.IndexOf (cr);
			} else {
			}*/
			if (ailogic == null) {
				ailogic = transform.GetComponent<AILogic> ();
			}
			if (routeRendererPrefab == null) {
				routeRendererPrefab = Resources.Load<GameObject> ("UI/RouteRenderer");
			}

		}

		public void AddPaths(List<Path> l) {
			if (routePaths == null) {
				routePaths = new List<Path> ();
			}
			routePaths.AddRange (l);
			for (int i = 0; i < l.Count; i++) {
				routeIds.Add (l [i].pathId);
			}
		}
		public void AddPath(Path p) {
			routePaths.Add (p);
			routeIds.Add (p.pathId);
		}
		public bool RemovePath(Path p) {
			
			bool removed =	routePaths.Remove (p);
			if (removed) {
				routeIds.Remove (p.pathId);
			}
			return removed;
		}
		public bool ToggleDisplayRoute() {
			ailogic.Log ("ToggleDisplayRoute called");
			if (routeRenderer == null) {
				DisplayRoute ();
				return true;
			} else {
				if (routeRenderer.enabled==true) {
					ailogic.Log ("ToggleDisplayRoute Hide");
					HideRoute ();
					return false;
				} else {
					ailogic.Log ("ToggleDisplayRoute Display");
					DisplayRoute ();
					return true;
				}
			}
		}
		public bool IsDisplayingRoute() {
			if (routeRenderer != null) {
				return routeRenderer.enabled;
			}
			return false;
		}
		public void DisplayRoute() {
			if (routeRenderer == null) {
				GameObject rr = Instantiate (routeRendererPrefab, transform);
				routeRenderer = rr.GetComponent<LineRenderer> ();
			}
			//Update points
				/*
				routeRenderer = gameObject.AddComponent<LineRenderer> ();
				*/
				List<Vector3> points = new List<Vector3> ();
				for (int i = 0; i < routeIds.Count; i++) {
					//if (changesList [i] != -1) {
					//	points.AddRange (idToPathDictionary [changesList [i]].GetInterpolatedPathPositions ());
					//} else {
					points.AddRange (idToPathDictionary [routeIds [i]].GetInterpolatedPathPositions ());
					//	}
					
				
				}
				//ailogic.Log ("Diplay route points " + points.Count);
				/*
				routeRenderer.startColor = Color.blue;
				routeRenderer.endColor = Color.blue;
				*/
				routeRenderer.positionCount = points.Count;
				routeRenderer.SetPositions (points.ToArray ());
				
	

			
				//Update pints

			routeRenderer.enabled = true;

		}


		public void HideRoute() {
			if (routeRenderer != null) {
				routeRenderer.enabled = false;
			}
		}

		public Path GetPathFromId(long pathid) {
			return idToPathDictionary [pathid];
		}
		public VenerisLane GetLaneFromPathId(long pathid) {
			return pathToLaneDictionary [GetPathFromId(pathid)];
		}

		public void SetStartPath(Path s) {
			startPath = s;
			SetLookAtPath (s);
		}

		public Path StartPath() {
			return startPath;
			//return idToPathDictionary [routeIds [0]];
		}

		public void SetLookAtPath(Path p) {
			
			_lookAtPath = p;
		}

		public void SetIdToPathDictionary (Dictionary<long,Path> d)
		{
			idToPathDictionary = d;
		}

		public void SetPathToLaneDictionary (Dictionary<Path, VenerisLane> d)
		{
			pathToLaneDictionary = d;
		}
		public Path GetTrackedPath ()
		{
			return trackedPath;
		}

		public void SetRoute (List<long> r)
		{
			this.routeIds = r;
		}

		public void SetRouteRoads (List<VenerisRoad> ro)
		{
			this.routeRoads = ro;
		}

		public void SetStrategicLaneChanges (List<long> changes)
		{
			changesList = changes;
		}

		public  long GetLaneChangeForPathId (long pathid)
		{
			
			return changesList [routeIds.IndexOf (pathid)];
		}

		public long GetLaneChangeForCurrentPath ()
		{
			
			return GetLaneChangeForPathId (_trackedPath.pathId);
		}

		public float DistanceToEndOfTrackedPath ()
		{
			int index = _trackedPath.FindClosestPointInInterpolatedPath (transform);
			return _trackedPath.GetPathDistanceFromIndexToEnd (index);
		}
		public float DistanceToEndOfLookAtPath ()
		{
			int index = _lookAtPath.FindClosestPointInInterpolatedPath (transform);
			return _lookAtPath.GetPathDistanceFromIndexToEnd (index);
		}

		public bool GetPathDistanceToEndOfPath(long pathid , out float dist) {

			int index = routeIds.IndexOf (pathid);
			//ailogic.Log ( "index=" + index + "pathid" + pathid +"trackedPathIndex="+trackedPathIndex);
			if (index < 0) {
				//Try with changes 
				index=changesList.IndexOf(pathid);
				//ailogic.Log (" change index=" + index + "pathid" + pathid +"trackedPathIndex="+trackedPathIndex +"currentlanepaid=");
			}
			int initindex = trackedPathIndex;
			//ailogic.Log ("initindex=" + initindex);
			//Path cp = GetPathInLane (ailogic.currentLane);
			//if (cp == null) { 
			//	initindex = trackedPathIndex;
			//} else {
			//	initindex = routePaths.IndexOf(cp.pathId);
			//ailogic.Log (5, "index=" + index + "pathid" + pathid +"trackedPathIndex="+trackedPathIndex +"initindex"+initindex);
			//}

			if (index >= 0 && index>=initindex ) {
				
				dist = DistanceToEndOfTrackedPath ();
				//ailogic.Log ("GetDistanceToEndOfCurrentPath=" + dist);
				for (int i = initindex+1; i <= index; i++) {
					/*if (changesList [i] == -1) {
						dist += idToPathDictionary [routeIds [i]].totalPathLength;
					//	ailogic.Log ("dist=" + dist + "p=" +routePaths [i]+"pal="+idToPathDictionary [routePaths [i]].totalPathLength);
					} else {
						dist += idToPathDictionary [changesList [i]].totalPathLength;
					//	ailogic.Log ("dist=" + dist + "p=" +changesList [i]+"pal="+idToPathDictionary [changesList [i]].totalPathLength);
					}*/
					dist += idToPathDictionary [routeIds [i]].totalPathLength;
				

				}

				return true;
			} 

			dist = -1f;
			return false;
		}

		public bool GetPathDistanceFromToEndOfPath(long initpid, long pathid ,out float dist) { 
			int initindex = routeIds.IndexOf (initpid);
			int endindex=routeIds.IndexOf (pathid);
			if (initindex >= 0 && endindex >= 0) {
				if (initindex <= endindex) {
					
					 dist = 0f;
					for (int i = initindex; i <= endindex; i++) {
						if (changesList [i] == -1) {
							dist += idToPathDictionary [routeIds [i]].totalPathLength;
							//ailogic.Log ("dist=" + dist + "p=" +routePaths [i]+"pal="+idToPathDictionary [routePaths [i]].totalPathLength);
						} else {
							dist += idToPathDictionary [changesList [i]].totalPathLength;
							//ailogic.Log ("dist=" + dist + "p=" +changesList [i]+"pal="+idToPathDictionary [changesList [i]].totalPathLength);
						}

					}
					return true;
				}
			} 
			dist = -1f;
			return false;
		}

		protected void BuildDictionaries ()
		{
			//Debug.Log ("Building dictionary in AgentRouteManager");
			idToPathDictionary = new Dictionary<long, Path> ();
			Path[] paths = FindObjectsOfType<Path> ();
			Debug.Log (paths.Length);
			foreach (Path p in paths) {
				if (routeIds.Contains (p.pathId) || changesList.Contains (p.pathId)) {
					//Have to add possible path changes
					Debug.Log ("pad=" + p.pathId);
					idToPathDictionary.Add (p.pathId, p);
					VenerisLane lane = p.GetComponent<VenerisLane> ();
					if (lane != null) {
						pathToLaneDictionary.Add (p, lane);
					}
				}
			}
			Debug.Log (routeIds.Count);
			Debug.Log (idToPathDictionary.Keys.Count);
			/*Debug.Log ("Keys added");
			foreach (long i in idToPathDictionary.Keys) {
				Debug.Log (i);
			}*/
		}

		public bool IsOnForwardRoads(VenerisRoad road) {
			int currentRoadIndex = routeRoads.IndexOf (ailogic.currentRoad);
			if (routeRoads.IndexOf (road, currentRoadIndex) >= 0) {
				return true;
			}
			return false;

		}

		public List<VenerisRoad> GetForwardRoads ()
		{
			
			return GetForwardRoads(ailogic.currentRoad);
		

		}
		public List<VenerisRoad> GetForwardRoads (VenerisRoad start)
		{

			int currentRoadIndex = routeRoads.IndexOf (start);
			if (currentRoadIndex >= 0) {
				return routeRoads.GetRange (currentRoadIndex,routeRoads.Count - currentRoadIndex);
			} else {
				return null;
			}

		}
		public List<VenerisRoad> GetForwardRoads (int n)
		{

			int currentRoadIndex = routeRoads.IndexOf (ailogic.currentRoad);
			if (currentRoadIndex >= 0) {
				if ((currentRoadIndex + n) > routeRoads.Count) {
					return routeRoads.GetRange (currentRoadIndex, routeRoads.Count - currentRoadIndex);
				} else {
					return routeRoads.GetRange (currentRoadIndex, n);
				}
			} else {
				return null;
			}

		}
		public List<VenerisRoad> GetForwardRoads (VenerisRoad start,int n)
		{

			int roadIndex = routeRoads.IndexOf (start);
			if (roadIndex >= 0) {
				if ((roadIndex + n) > routeRoads.Count) {
					return routeRoads.GetRange (roadIndex, routeRoads.Count - roadIndex);
				} else {
					return routeRoads.GetRange (roadIndex, n);
				}
			} else {
				return null;
			}

		}
		public  List<VenerisRoad> GetRoads () {
			return routeRoads;
		}
		public VenerisRoad GetNextRoad ()
		{

			int currentRoadIndex = routeRoads.IndexOf (ailogic.currentRoad);

			if (currentRoadIndex >= 0 && currentRoadIndex<routeRoads.Count -1) {
				return routeRoads [currentRoadIndex + 1];
			} else {
				return null;
			}

		}
		public VenerisRoad GetFollowingRoad(VenerisRoad current) {
			int followingRoadIndex = routeRoads.IndexOf (current);
			if (followingRoadIndex >= 0 && followingRoadIndex<routeRoads.Count -1) {
				return routeRoads [followingRoadIndex + 1];
			} else {
				return null;
			}
			
		}
		public VenerisRoad GetPrecedingRoad(VenerisRoad current) {
			int currentRoadIndex = routeRoads.IndexOf (current);
			if (currentRoadIndex >= 1 && currentRoadIndex<routeRoads.Count) {
				return routeRoads [currentRoadIndex -1];
			} else {
				return null;
			}

		}

		public virtual Path GetLastPathInRoute ()
		{
			return idToPathDictionary [routeIds [routeIds.Count - 1]];
		}
		public VenerisRoad GetLastRoadInRoute ()
		{
			return routeRoads [routeRoads.Count - 1];
		}

		public long NextTrackedPathId ()
		{
			int i = routeIds.IndexOf (trackedPath.pathId);
			if (i >= 0 && i < routeIds.Count - 1) {
				return routeIds [i + 1];
			} else {
				return -1;
			}


		}


		public Path NextTrackedPath ()
		{
			long nextId = NextTrackedPathId ();
			if (nextId != -1) {
				return GetPathInRoute (nextId);
			} else {
				return null;
			}
		}

		public Path FollowingPath (long id)
		{
			int i = routeIds.IndexOf (id);
			if (i >= 0 && i < routeIds.Count - 1) {
				return idToPathDictionary[routeIds [i + 1]];
			} else {
				return null;
			}


		}
		public Path GetLastPath() {
			return routePaths [routePaths.Count - 1];
		}

		public long FollowingPathId (long id)
		{
			int i = routeIds.IndexOf (id);
			if (i >= 0 && i < routeIds.Count - 1) {
				return routeIds [i + 1];
			} else {
				return -1;
			}


		}
		public long PreviousPathId (long id)
		{
			int i = routeIds.IndexOf (id);
			if (i > 0 && i < routeIds.Count ) {
				return routeIds [i -1];
			} else {
				return -1;
			}


		}

		public Path GetPathInRoute (long id)
		{
			if (id < 0) {
				return null;
			}
			if (routeIds.Contains (id)) {
				return idToPathDictionary [id];
			}
			return null;


		}
		public Path GetPathInRoad (VenerisRoad road)
		{
			//TODO: check for multiple occurrences in routePaths
			VenerisLane[]  lanes= road.lanes;
			for (int i = 0; i < lanes.Length; i++) {
				//Path[] paths = lanes [i].GetComponents<Path> ();
				for (int j = 0; j < lanes[i].paths.Count; j++) {
					if (routeIds.Contains(lanes[i].paths[j].pathId)) {
						return lanes[i].paths[j];
					}
					
				}
			}
			return null;
		}
		public Path GetPathInLane (VenerisLane lane)
		{
			//TODO: check for multiple occurrences in routePaths
			/*	Path[] paths = lane.GetComponents<Path> ();
				for (int j = 0; j < paths.Length; j++) {
				
					if (routePaths.Contains(paths[j].pathId)) {
					
						return paths[j];
					}

				}


			return null;
			*/
			Path p = GetPathInLaneFromRoutes (lane);
			if (p == null) {
				return GetPathInLaneFromRouteChanges (lane);
			} else {
				return p;
			}
		}
		public Path GetPathInLaneFromRoutes (VenerisLane lane)
		{
			//TODO: check for multiple occurrences in routePaths
			//Path[] paths = lane.GetComponents<Path> ();

			for (int j = 0; j < lane.paths.Count; j++) {

				if (routeIds.Contains(lane.paths[j].pathId)) {

					return lane.paths[j];
				}

			}


			return null;
		}
		public Path GetPathInLaneFromRouteChanges (VenerisLane lane)
		{
			//TODO: check for multiple occurrences in routePaths
			//Path[] paths = lane.GetComponents<Path> ();
			for (int j = 0; j <lane.paths.Count; j++) {

				if (changesList.Contains(lane.paths[j].pathId)) {

					return lane.paths[j];
				}

			}


			return null;
		}

		public List<Path> GetPathsInJunction (IntersectionInfo info)
		{
			// in Intersections we are going to have multiple paths. Track them if necessary. 

		
			List<Path> paths = info.internalPaths;
			List<Path> found = new List<Path> ();
			for (int j = 0; j < paths.Count; j++) {
				if (routeIds.Contains(paths[j].pathId)) {
					//ailogic.Log (5, "paths[j]=" + paths [j].pathId);
					found.Add (paths [j]);
				}

			}

			return found;
		}
		public Path GetFirstPathInJunction(IntersectionInfo info) {
			List<Path> paths = GetPathsInJunction (info);
			if (paths.Count == 0) {
				return null;
			}
			if (paths.Count == 1) {
				return paths [0];	
			}
			int minindex = int.MaxValue;
			bool found = false;
			for (int i = 0; i < paths.Count; i++) {
				int index = routeIds.IndexOf (paths[i].pathId,trackedPathIndex);
				//ailogic.Log (5, "paths[i]=" + paths [i].pathId+"index="+index);
				if (index>=0 && index < minindex) {
					minindex = index;
					found = true;
				}
			}
			if (found) {
				//ailogic.Log (5, "minindex="+minindex);
				return idToPathDictionary [routeIds [minindex]];
			} else {
				return null;
			}
						
		}
		//Return either -1 if path is not in my route or the number of paths to get to that path from my current one
		public int NumberOfHopsIfPathIsInRouteForward (long pathid)
		{
			if (idToPathDictionary.ContainsKey (pathid)) {
				//int h = route.IndexOf (pathid) - currentPathIndex;
				//We need to take into account all paths we are supposed to travel, includin 
				int h = routeIds.IndexOf (pathid) - trackedPathIndex;
				if (h >= 0) {
					return h;
				}
			}
			return -1;
		}

		public float CheckEndDistanceIfPathIsInRouteForward (long pathid)
		{
			float dist = 0f;
			int hops = NumberOfHopsIfPathIsInRouteForward (pathid);
			if (hops >= 0) {
				dist += DistanceToEndOfTrackedPath ();
				//Lane changes do not count in the distance, otherwise we would be counting the same edge more than once
				for (int h = trackedPathIndex + 1; h <= routeIds.IndexOf (pathid); h++) {
					dist += idToPathDictionary [routeIds [h]].totalPathLength;
				}
				return dist;
			} else {
				return -1f;
			}
		}

		public float CheckBeginDistanceIfPathIsInRouteForward (long pathid)
		{
			float dist = 0f;
			int hops = NumberOfHopsIfPathIsInRouteForward (pathid);
			if (hops >= 0) {
				dist += DistanceToEndOfTrackedPath ();
				for (int h = trackedPathIndex + 1; h < routeIds.IndexOf (pathid); h++) {
					dist += idToPathDictionary [routeIds [h]].totalPathLength;
				}
				return dist;
			} else {
				return -1f;
			}
		}
		//Return either -1 if path is not in my route or the number of hops to get to that path from my current one
		public int NumberOfHopsIfPathIsInRouteRoadsForward (long pathid)
		{
			if (idToPathDictionary.ContainsKey (pathid)) {
				VenerisRoad pr = idToPathDictionary [pathid].GetComponentInParent<VenerisRoad> ();
				int currentRoadIndex = routeRoads.IndexOf (ailogic.currentRoad);
				if (currentRoadIndex >= 0) {
					for (int i = currentRoadIndex; i < routeRoads.Count; i++) {
						if (routeRoads [i] == pr) {
							return (i - currentRoadIndex);
						}
					
					}
				}
			}
			return -1;
			
		}

		//Returns the Path with idf=nextid if it is connected to Path with id=fromId in PathConnector con
		public ConnectionInfo.PathDirectionInfo GetPathIfIsInConnector (PathConnector con, long fromId, long nextId)
		{
			if (nextId == -1) {
				return null;
			}
			if (con != null) {
				ConnectionInfo info = con.GetPathsConnectedTo (fromId);
				if (info == null) {
					//Our vehicle may have stepped into a connector for other path
					//But we may require a previous change of lane
					long changeBefore = GetLaneChangeForPathId (fromId);
					if (changeBefore == -1) {
						return null;
					} else {
						info = con.GetPathsConnectedTo (changeBefore);
					}

				} 
					
				if (info == null) {
					return null;
				}
				//Check if the next path require change of lane
				
				long changeAfter = GetLaneChangeForPathId (nextId);
				if (info.connectedPaths != null) {
					foreach (ConnectionInfo.PathDirectionInfo pair in info.connectedPaths) {
						//Debug.Log ("p.path" + pair.p.pathId);
						if (pair.p.pathId == nextId || pair.p.pathId == changeAfter) { //If next path requires change to some path in the connector, let it change and the vehicle will try to change lane when on the road
							//if (pair.p.pathId == nextId) {
							if (pair.p.pathId == changeAfter) {
								Debug.Log ("Found next path. Required changing lane to" + changeAfter);
								//Create an axu Pair
								//ConnectionInfo.PathDirectionPair aux = new ConnectionInfo.PathDirectionPair(
							}



							return pair;
						}
					}
				}


			}
			return null;
		}

		//Return the next path in our route if it is in connector
		public ConnectionInfo.PathDirectionInfo GetNextPathIfIsInConnector (PathConnector con)
		{
			ConnectionInfo.PathDirectionInfo pair = null;
			//Try first with our current tracked path
			pair=GetPathIfIsInConnector (con, trackedPath.pathId, NextTrackedPathId ());
			if (pair != null) {
				return pair;
			}
			//Try next path, we may have missed a connector
			long nextId=NextTrackedPathId();
			return GetPathIfIsInConnector (con, nextId, FollowingPathId (nextId));
		}

		public ConnectionInfo.PathDirectionInfo GetPathFromRoadIfIsInConnector (VenerisRoad road, PathConnector con)
		{
			//Find the path associated to the road
			List<long> pids = GetPathsInRouteFromRoad (road);
			for (int i = 0; i < pids.Count; i++) {
				
				ConnectionInfo.PathDirectionInfo pair=GetPathIfIsInConnector (con, pids[i], FollowingPathId (pids[i]));
				if (pair != null) {
					return pair;
				}
			}
			return null;

			//return GetPathIfIsInConnector (con, pid, FollowingPathId (pid));

		}

		public List<long> GetPathsInRouteFromRoad (VenerisRoad road)
		{
			//TODO: NOT Including lane changes
			List<long> pids=new List<long>();
			foreach (VenerisRoad r in routeRoads) {
				if (r == road) {
					//Debug.Log ("r==road" + r.name);
					foreach (VenerisLane lane in road.lanes) {
						//Path[] paths=lane.GetComponents<Path>();
						for (int i = 0; i < lane.paths.Count; i++) {
							int index = routeIds.IndexOf (lane.paths[i].pathId);
							if (index >= 0) {
								pids.Add(routeIds [index]);
							}

						}
						/*Path p = lane.GetComponent<Path> ();
						if (p != null) {
							int index = routePaths.IndexOf (p.pathId);
							if (index >= 0) {
								return routePaths [index];
							}
						}*/
					}
				}
			}
			return pids;
		}
		//Set the next path in our route
		public bool SetNextPathFromConnector (PathConnector con)
		{
			ConnectionInfo.PathDirectionInfo pair = GetNextPathIfIsInConnector (con);
			if (pair != null) {

				//ailogic.Log ("Setting tracked path from SetNextPathFromConnector");
				SetCurrentPath(pair.p);
				//_trackedPath = pair.p;
				//trackedPathIndex = routePaths.IndexOf (_trackedPath.pathId);

				return true;
			}
			return false;
		}

		public void SetCurrentPath(Path p) {
			//ailogic.Log ("Setting tracked path from SetCurrentPath");
			trackedPath = p;
			int i = routeIds.IndexOf (trackedPath.pathId);
			if (i>=0) {
				trackedPathIndex = i;
			}
		}

	/*	public void SetCurrentPathFromLaneChange (Path p)
		{
			ailogic.Log ("Setting tracked path from SetCurrentPathFromLaneChange");
			//Insert new path in our route
			routePaths.Insert (trackedPathIndex + 1, p.pathId);
			changesList.Insert (trackedPathIndex + 1, -1);
			_trackedPath = p;

		}
		*/
		public void StartLaneChange(Path startPath, Path target) {
			//ailogic.Log ("StartLaneChange" + startPath.pathId + "target=" + target.pathId);
			int index=routePaths.IndexOf (startPath);
			if (index >= 0) {
				routePaths.Insert (index+1, target);
			}
			int index2 = routeIds.IndexOf (startPath.pathId);
			UnityEngine.Assertions.Assert.AreEqual (index, index2);
			if (index >= 0) {
				routeIds.Insert (index + 1, target.pathId);
			}

			//changesList.Insert (trackedPathIndex + 1, -1);
		}
		public void FinishLaneChange ( long targetId)
		{
			//ailogic.Log ("FinishLaneChange" + trackedPathIndex + "target=" + targetId);
			//int index = routePaths.IndexOf (currentpid);
			//routePaths.Insert (index + 1, target.pathId);
			//changesList.Insert (index + 1, -1);
			//routePaths.Insert (trackedPathIndex + 1, target.pathId);
			//changesList.Insert (trackedPathIndex + 1, -1);
			//We have finished a lane change, we should be on our route 
			//trackedPathIndex = routePaths.IndexOf (_trackedPath.pathId);
			SetCurrentPath(idToPathDictionary[targetId]);

		}
	

		/*	void OnDrawGizmosSelected ()
		{
			if (Application.isPlaying) {
				//Show our route
				List<GameObject> paths = new List<GameObject> ();
				foreach (long l in route) {
					paths.Add (idToPathDictionary [l].gameObject);
				}
				Selection.objects = paths.ToArray ();
			}
		}
		*/

	}
}
