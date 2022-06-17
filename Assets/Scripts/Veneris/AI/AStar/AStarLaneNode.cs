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
	//Uncomment to show and debug on editor, but comment to build. Otherwise you will probably get recursive serialization errors
	//[System.Serializable]  
	public class AStarLaneNode
	{
		public VenerisRoad road;
		public VenerisLane lane;
		public List<AStarLaneNode> neighbors;
		public AStarLaneNode(VenerisRoad road, VenerisLane lane) {
			
			this.road = road;
			this.lane = lane;
			neighbors = new List<AStarLaneNode> ();
		}
		public void AddNeighbor(AStarLaneNode node) {
			
			neighbors.Add (node);
		}
		public  string ToString ()
		{
			return road.sumoId + ":" + lane.sumoId;
		}
		public  bool IsEqualNode (AStarLaneNode other)
		{
			
			return (this.road == other.road && this.lane == other.lane);
		}
	}
}
