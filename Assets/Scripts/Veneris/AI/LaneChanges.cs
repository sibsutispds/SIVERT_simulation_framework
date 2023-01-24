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


	[System.Serializable]
	public enum LaneChangeDirection {
		Right,
		Left,
		None,
	}
	[System.Serializable]
	public class LaneChangeRequest {
		public enum LaneChangeFollowerPosition { None, InFront, Behind, Parallel};
		public LaneChangeFollowerPosition followerPosition;
		public bool isExecutingManeuver=false;
		public bool isFinished = false;
		public Path targetPath =null;
		public Path startPath=null;
		public VenerisLane targetLane = null;
		public VehicleInfo follower;
		public LaneChangeRequest(VenerisLane startLane, VenerisLane targetLane) {

			startPath = startLane.paths[0];
			targetPath=targetLane.paths[0];
			this.targetLane = targetLane;
			followerPosition = LaneChangeFollowerPosition.None;
		}
	}
	[System.Serializable]
	public class LaneChangeQueueEntry : IEquatable<LaneChangeQueueEntry>{
		public long startPid;
		public long targetPId;
		public VenerisLane startLane;
		public VenerisLane targetLane;
		public LaneChangeQueueEntry(long startPid, long targetPId, VenerisLane startLane) {
			this.startPid=startPid;
			this.targetPId=targetPId;
			this.startLane=startLane;
		}
		public LaneChangeQueueEntry(VenerisLane fromLane, VenerisLane toLane) {
			this.startLane = fromLane;
			this.targetLane = toLane;
			this.startPid = fromLane.paths [0].pathId;
			this.targetPId = toLane.paths [0].pathId;
		}
		public bool Equals(LaneChangeQueueEntry other) {
			return (this.startPid == other.startPid && this.targetPId == other.targetPId && this.startLane == other.startLane && this.targetLane==other.targetLane);
		}
		public delegate void OnLaneChangeCompleted(); 
		public OnLaneChangeCompleted laneChangeCompletedListeners = null;
	}


	//TODO: No multilane change sequences at the moment, just change from one lane to a neighbor one at a time
	[System.Serializable]
	public class SingleLaneChangeSequence
	{
		
		public long targetPathId = -1;
		public long startPathId = -1;
	
		public VenerisLane startLane = null;
		public VenerisLane targetLane = null;
		public VenerisRoad startRoad = null;
		public bool isBackBumperOnLane = false;
		public bool hasFinishedTurning = false;
		public LaneChangeQueueEntry origin = null;

		public void Initialize( VenerisLane startLane, VenerisLane targetLane, LaneChangeQueueEntry or) {
			startPathId = startLane.paths[0].pathId;
			targetPathId=targetLane.paths[0].pathId;
			this.startLane = startLane;
			this.targetLane = targetLane;
			startRoad = startLane.GetComponentInParent<VenerisRoad> ();
			this.origin = or;
			isBackBumperOnLane = false;
			hasFinishedTurning = false;
		}



	}


}
