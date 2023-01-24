/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{


	public class VenerisRoadConnectionEntry {
		public Dictionary<VenerisRoad,VenerisConnectionSummary> roadToSummaryDictionary=null;
		public VenerisRoadConnectionEntry() {
			roadToSummaryDictionary = new Dictionary<VenerisRoad, VenerisConnectionSummary>();
		}
		public void AddConnectionEntry( VenerisRoad toRoad,VenerisLane fromLane, VenerisLane toLane, List<Path> ipaths, List<SumoConnection> iconns) {
			VenerisConnectionSummary s = null;
			if (roadToSummaryDictionary.ContainsKey (toRoad)) {
				
				s = roadToSummaryDictionary [toRoad];
			} else {
				
				s = new VenerisConnectionSummary ();
				roadToSummaryDictionary.Add (toRoad, s);
			}
			for (int j = 0; j < ipaths.Count; j++) {
				

				s.AddInternalPathInfo (fromLane, toLane, ipaths [j], iconns [j]);
			}
		}
		public List<Path> GetPathsFromLaneToLane(VenerisRoad toRoad,VenerisLane fromLane, VenerisLane toLane) {
			
			return  roadToSummaryDictionary [toRoad].GetInternalPaths (fromLane, toLane);
		}
		public List<Path> GetPathsFromLane(VenerisRoad toRoad,VenerisLane fromLane) {

			return  roadToSummaryDictionary [toRoad].GetInternalPaths (fromLane);
		}
		public List<Path> GetPathsFromLane(VenerisRoad toRoad,int fromLaneIndex) {

			return  roadToSummaryDictionary [toRoad].GetInternalPaths (fromLaneIndex);
		}
		public int GetNumberOfConnectedLanes(VenerisRoad toRoad) {
			
			return roadToSummaryDictionary [toRoad].GetNumberOfConnectedLanes ();
		}
		public VenerisLane GetFromLane(VenerisRoad toRoad,int fromLaneIndex ) {
			return roadToSummaryDictionary [toRoad].GetFromLane (fromLaneIndex);
		}
		public List<VenerisLane> GetOutcomingLanes(VenerisRoad toRoad,VenerisLane fromLane) {
			return roadToSummaryDictionary [toRoad].GetOutcomingLanes (fromLane);
		}
	}


	public class VenerisConnectionSummary 
	{

		public class InternalPathInfo
		{
			public VenerisLane fromLane;
			public VenerisLane toLane;
			public List<SumoConnection> internalSumoConnectionList=null;
			public List<Path> internalPaths = null;
			public InternalPathInfo(VenerisLane fromLane, VenerisLane toLane) {
				this.fromLane = fromLane;
				this.toLane = toLane;
				internalPaths = new List<Path>();
				internalSumoConnectionList = new List<SumoConnection>();

			}
		}

		//public VenerisRoad fromRoad;
		//public VenerisRoad toRoad;
		public List<InternalPathInfo> connections =null;
		public VenerisConnectionSummary() {
			//this.fromRoad = fromRoad;
			//this.toRoad = toRoad;
			connections = new List<InternalPathInfo> ();

		}
		public void AddInternalPathInfo (VenerisLane fromLane, VenerisLane toLane, Path internalPath, SumoConnection con)
		{
			if (connections != null) {
				
				InternalPathInfo info = FindInternalPathInfo (fromLane, toLane);
				if (info == null) {
					info = new InternalPathInfo (fromLane, toLane);
					connections.Add (info);

				}
				if (info.internalPaths == null) {
					info.internalPaths = new List<Path> ();
					info.internalSumoConnectionList = new List<SumoConnection> ();
				}
				info.internalPaths.Add (internalPath);
				info.internalSumoConnectionList.Add (con);

	
			}
		}
		public InternalPathInfo FindInternalPathInfo(VenerisLane fromLane, VenerisLane toLane) {
			for (int i = 0; i < connections.Count; i++) {
				if (connections [i].fromLane == fromLane && connections [i].toLane == toLane) {
					return connections [i];
					break;
				}
			}
			return null;
		}
		public int GetNumberOfConnectedLanes() {
			return connections.Count;
		}
		public List<Path> GetInternalPaths( VenerisLane fromLane, VenerisLane toLane) {
			for (int i = 0; i < connections.Count; i++) {
				if (connections [i].fromLane == fromLane && connections [i].toLane == toLane) {
					return connections [i].internalPaths;
				}
			}
			return null;
		}
		public List<Path> GetInternalPaths( VenerisLane fromLane) {
			for (int i = 0; i < connections.Count; i++) {
				if (connections [i].fromLane == fromLane) {
					return connections [i].internalPaths;
				}
			}
			return null;
		}
		public List<Path> GetInternalPaths( int fromLaneIndex) {
			

			return connections [fromLaneIndex].internalPaths;
				
		}
		public VenerisLane GetFromLane(int index) {
			return connections [index].fromLane;
		}
		public List<VenerisLane> GetOutcomingLanes(VenerisLane fromLane) {
			List<VenerisLane> lanes = new List<VenerisLane> ();
			for (int i = 0; i < connections.Count; i++) {
				if (connections [i].fromLane == fromLane) {
					//return connections [i].toLane;
					lanes.Add(connections[i].toLane);
				}
			}
			return lanes;
		}
	}

}