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
	public class IntersectionInfo : MonoBehaviour
	{
		
		[System.Serializable]
		public class RoadAdjacencyInfo
		{
			public VenerisRoad toRoad = null;
			public List<Transform> stopLines = null;
			public List<PathConnector> connectors = null;

			public RoadAdjacencyInfo (VenerisRoad to, Transform stop, PathConnector con)
			{
				toRoad = to;
				stopLines = new List<Transform>();
				connectors= new List<PathConnector>();
				stopLines.Add(stop);
				connectors.Add(con);
			}
			public void AddStopLine(Transform t) {
				stopLines.Add (t);
			}
			public void AddPathConnector(PathConnector con) {
				connectors.Add (con);
			}
		}

		[System.Serializable]
		public class RoadAdjacencyEntry
		{
			public VenerisRoad fromRoad = null;
			public List<RoadAdjacencyInfo> adjacency = null;

			public RoadAdjacencyEntry (VenerisRoad road)
			{
				fromRoad = road;
			}

			public void AddAdjacencyInfo (RoadAdjacencyInfo info)
			{
				if (adjacency == null) {
					adjacency = new List<RoadAdjacencyInfo> ();
				}
				adjacency.Add (info);
			}
			public RoadAdjacencyInfo GetInfoToRoad (VenerisRoad toRoad)
			{
				if (adjacency == null) {
					return null;
				}
				foreach (RoadAdjacencyInfo info in adjacency) {
					if (info.toRoad == toRoad) {
						return info;
					}
				}
				return null;
			}
		}

		public long intersectionId;
		public string sumoJunctionId="";
		public List<PathConnector> connectors = null;
		public List<RoadAdjacencyEntry> roads = null;
		public List<Transform> stopLines = null;
		public List<Path> internalPaths = null;
		public List<TrafficLight> trafficLights = null;
		public Transform junction = null;

		// Use this for initialization
		void Start ()
		{
			//Debug.Log ("IntersectionInfo Start:" + sumoJunctionId); 
			stopLines = new List<Transform> ();
			internalPaths = new List<Path> ();
			for (int i = 0; i < transform.childCount; i++) {
				if (transform.GetChild(i).name.Contains("StopLine")) {
					stopLines.Add(transform.GetChild(i));
				}
				if (transform.GetChild (i).name.Contains ("InternalPath")) {
					internalPaths.Add (transform.GetChild (i).GetComponent<Path> ());
				}
				if (transform.GetChild (i).CompareTag ("Junction")) {
					junction = transform.GetChild (i);
				}
			}

				
		}

		public void SetTrafficLight(TrafficLight t) {
			if (trafficLights == null) {
				trafficLights = new List<TrafficLight> ();
			}
			if (trafficLights.Contains (t)) {
				return;
			} else {
				trafficLights.Add (t);
			}
			
		}

		public void SetRoad(VenerisRoad r) {
			if (roads == null) {
				roads = new List<RoadAdjacencyEntry> ();
			}
			if (GetRAEntry (r) == null) {
				roads.Add (new RoadAdjacencyEntry(r));
			}
		}
		public RoadAdjacencyEntry GetRAEntry(VenerisRoad r) {
			//foreach (RoadAdjacencyEntry entry in roads) {
			for (int i = 0; i < roads.Count; i++) {
					
				
				if (roads[i].fromRoad == r) {
					return roads[i];
				}
			}
			return null;
		}

		public void AddAjacencyInfo(VenerisRoad fromRoad, VenerisRoad toRoad,Transform stop, PathConnector pc) {
			RoadAdjacencyEntry entry = GetRAEntry (fromRoad);
			if (entry == null) {
				Debug.LogError ("Attempt to add adjacency info to non-existent road " + fromRoad.roadName);
			} else {
				RoadAdjacencyInfo info = entry.GetInfoToRoad (toRoad);
				if ( info== null) {
					entry.AddAdjacencyInfo (new RoadAdjacencyInfo (toRoad, stop, pc));
				} else {
					info.AddStopLine (stop);
					info.AddPathConnector(pc);
				}
			}
			
		}
		public bool AreRoadsConnectedByIntersection(VenerisRoad fromRoad, VenerisRoad toRoad) {
			if (GetAdjacencyInfo (fromRoad, toRoad) == null) {
				return false;
			} else {
				return true;
			}
		}
		public List<Transform> GetStopLines(VenerisRoad fromRoad, VenerisRoad toRoad) {
			return GetAdjacencyInfo (fromRoad, toRoad).stopLines;
		}
		public List<PathConnector> GetPathConnectors(VenerisRoad fromRoad, VenerisRoad toRoad) {
			return GetAdjacencyInfo (fromRoad, toRoad).connectors;
		}
		public RoadAdjacencyInfo GetAdjacencyInfo(VenerisRoad fromRoad, VenerisRoad toRoad) {
			//foreach (RoadAdjacencyEntry entry in roads) {
			for (int i = 0; i < roads.Count; i++) {
				if (roads[i].fromRoad == fromRoad) {
					//foreach (RoadAdjacencyInfo info in entry.adjacency) {
					for (int j = 0; j< roads[i].adjacency.Count; j++) {
						if (roads[i].adjacency[j].toRoad == toRoad) {
							return roads[i].adjacency[j];
						}
					}
				}
			}
			return null;
		}
		public VenerisRoad GetFromRoadFromStopLine(Transform stopLine) {
			for (int i = 0; i < roads.Count; i++) {
				for (int j = 0; j < roads[i].adjacency.Count; j++) {
					for (int k = 0; k < roads[i].adjacency[j].stopLines.Count; k++) {
						if (roads [i].adjacency [j].stopLines[k]==stopLine) {
							return roads [i].fromRoad;
						}
					}

				}
			}
			return null;
		}
	}
}
