/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



//TODO: works but we should find another way. Try and change if simulated physics become available 


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris
{
	[System.Serializable]
	public enum LeaderReason
	{
		None,
		NoneFound,
		Front,
		SignalingCloserThanFront,
		SignalingAndNoFront,
		ApproachingWithLowTTC,
		ApproachingNotDangerous,
		Inside,
		InFrontNotBlocking,
		InsideAndChangingLane,
		InsidePassedMe,
		IgnoreBlocking,
		WaitEvasiveManeuver,

	}
	[System.Serializable]
	public class LeaderInfo {
		public VehicleInfo leader=null;
		public float distance=-1f;
		public LeaderReason reason; 
		public LeaderInfo() {
			leader=null;
			distance=-1f;
			reason = LeaderReason.None;
			
		}
		public void SetLeader(VehicleInfo l, float d, LeaderReason r) {
			leader = l;
			distance = d;
			reason = r;
		}
		public void Clear() {
			leader=null;
			distance=-1f;
			reason = LeaderReason.None;
		}
		public string ToString() {
			if (leader == null) {
				return "No leader. Reason=" + reason;
			} else {
				return leader.vehicleId + ". Reason=" + reason + ". Distance=" + distance;
			}
		}
	}
	[System.Serializable]
	public class LeadingVehicleSelector
	{
		


		//public VehicleInfo leader;
		//public LeaderReason reason;
		//public float leaderDistance = -1f;
		public bool showLog=false;
		public LeaderInfo leaderInfo = null;


		protected AILogic ailogic = null;
		public float maxCollisionTimeToReact = 2f;
		//Max TTC until we consider another vehicle a risk and adapt to it
		public float angleForApproaching = 80f;
		//Angle used to decide if a vehicle is approaching, should be 90, but we can be more conservative to improve safety
		public float angleForHeading = 20f;
		public VehicleInfo insideFrontVehicle;
		public VehicleInfo insideWantChangeVehicle;
		public float giveWaySafetyGap = 2f;

		public Collider[] leaderBuffer = null;


		public List<VehicleInfo> approaching = null;
		public List<VehicleInfo> insideList = null;
		public List<CollisionPrediction.TTCInfo> insideTTCList = null;
		public VehicleInfo minTTCVehicle = null;
		public VehicleInfo frontVehicle = null;
		public VehicleInfo signalingVehicle = null;
		public float signalingVehicleDistance=0.0f;


		public LeadingVehicleSelector (AILogic l, float safety, LeaderInfo info=null)
		{
			
			//giveWaySafetyGap = safety * safety;
			giveWaySafetyGap = 0.8f*safety;
			ailogic = l;
			maxCollisionTimeToReact = 2f;
			approaching = new List<VehicleInfo> (8);
			insideList = new List<VehicleInfo> (8);
			insideTTCList = new List<CollisionPrediction.TTCInfo> (8);
			leaderBuffer = new Collider[2];
			if (info == null) {
				leaderInfo = new LeaderInfo ();
			} else {
				leaderInfo = info;
			}
		}
		public CollisionPrediction.TTCInfo GetTTCInfoFromInsideList(VehicleInfo i) {
			int index = insideList.IndexOf (i);
			if (index >= 0) {
				return insideTTCList [index];
			} else {
				return new CollisionPrediction.TTCInfo();
			}
		}
		public 	void DecideLeadingVehicle ()
		{
			
			 DecideLeaderWithTrajectoryIntersection ();
		}
		public 	bool SelectLeadingVehicleWithoutCloseVehicles ()
		{
			
			return  SelectLeaderWithoutCloseVehiclesWithTrajectoryIntersection ();
		}
		public void UnsetLeadingVehicle ()
		{
			leaderInfo.Clear ();
		}


		public void SetLeader (VehicleInfo l, float distance, LeaderReason r)
		{
			leaderInfo.leader = l;
			leaderInfo.distance = distance;
			leaderInfo.reason = r;
		}
	
	
		public bool FindSignalingVehiclesInFront() {

			//First, check for overlapping signals (not detected by boxcast)
			bool found=false;
			signalingVehicle = null;
			signalingVehicleDistance = 0f;
			int hits = ailogic.vision.CheckPositionOccupiedByVehicleSignal (ailogic.vehicleInfo.carBody.position, leaderBuffer);

			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short. FindSignalingVehiclesInFront");
			}
			for (int j = 0; j < hits; j++) {

				VehicleInfo info = leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> ();
				if (info != ailogic.vehicleInfo) {
					Vector3 p = leaderBuffer [j].ClosestPoint (ailogic.vehicleInfo.frontBumper.position);
					if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (p).z >= 0) {
						//It is actually in front of us, at least partially, consider it
						signalingVehicle=info;
						signalingVehicleDistance = (ailogic.vehicleInfo.frontBumper.position - p).magnitude;
						return true;
					}
				}

			}


			RaycastHit frontVehicleSignalHit;
			if ( ailogic.vision.CheckFrontForVehicleSignal (out frontVehicleSignalHit)) {

				signalingVehicle = frontVehicleSignalHit.transform.GetComponentInParent<AILogic> ().vehicleInfo;
				signalingVehicleDistance = frontVehicleSignalHit.distance;
				return true;
			}
			return false;
			
		}


		protected void DecideLeaderWithTrajectoryIntersection ()
		{

			//Collect surrounding information

			//Get vehicles from  CollisionPredictonTrigger
			List<Collider> surrounding = ailogic.vision.GetVehiclesInPotentialIntersection ();

			//Additionally, always check front
			RaycastHit frontHit;
			bool front = ailogic.vision.CheckFrontForVehicle (out frontHit);

			//Check signaling vehicles
		
			bool frontVehicleSignal = FindSignalingVehiclesInFront ();

			//Take a look at where we are heading
			RaycastHit h;
			bool hit;
			hit = ailogic.vision.CheckPositionForVehicle (out h, ailogic.vision.steerLookAtPoint);
			LeaderReason reason = LeaderReason.None;
			if (hit == false && frontVehicleSignal == false && surrounding.Count == 0 && front == false && ailogic.vision.checkVehiclePositions == false) {
				//No one close to me
				//ailogic.Log("unset no one close");

				SetLeader (null, -1f, LeaderReason.NoneFound);

			}

			VehicleInfo info;

			float angle = -1f;

			//Find the approaching vehicles
			insideFrontVehicle = null;
			insideWantChangeVehicle = null;
			approaching.Clear ();
			insideList.Clear ();
			minTTCVehicle = null;
			frontVehicle = null;
			signalingVehicle = null;


			if (hit) {
				info = h.collider.gameObject.GetComponentInParent<VehicleInfo> ();
				int index = approaching.IndexOf (info);
				if (index == -1) {
					approaching.Add (info);
				}
			}


			for (int i = 0; i < surrounding.Count; i++) {
				info = surrounding [i].gameObject.GetComponentInParent<VehicleInfo> ();
				//float angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, info.carBody.forward);



				int index = approaching.IndexOf (info);
				if (index == -1) {
					approaching.Add (info);
				}

			}

			//Someone has warned us to check some positions for vehicles
			if (ailogic.vision.checkVehiclePositions) {

				for (int i = 0; i < ailogic.vision.checkVehiclePositionsList.Count; i++) {


					//Check only those in front of me
					if (ailogic.vehicleInfo.carBody.InverseTransformPoint (ailogic.vision.checkVehiclePositionsList [i].position).z >= 0) {


						hit = ailogic.vision.CheckPositionForVehicle (out h, ailogic.vision.checkVehiclePositionsList [i].position);


						if (hit) {
							info = h.collider.gameObject.GetComponentInParent<VehicleInfo> ();
							angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, info.carBody.forward);
							//ailogic.Log(0,  "checking approaching=" +h.transform.root.name+"with angle"+angle);
							if (angle >= angleForApproaching) {
								//It is heading me, approaching
								int index = approaching.IndexOf (info);
								if (index == -1) {
									approaching.Add (info);
								}
								//ailogic.Log( " approaching=" +h.transform.root.name);
							}


						}
					}
				}
			}




			if (front) {

				frontVehicle = frontHit.collider.gameObject.GetComponentInParent<VehicleInfo> ();

				angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, frontVehicle.carBody.forward);
				if (angle > angleForHeading) {

					//Use trajectory intersection
					int index = approaching.IndexOf (frontVehicle);
					if (index == -1) {
						approaching.Add (frontVehicle);

					}
					frontVehicle = null;
					front = false;

				}
			}





			//Get the minTTC and  or inside caution sphere vehicles


			CollisionPrediction.TTCInfo minTTCInfo;
			minTTCInfo.ttcSphere = float.MaxValue;
			minTTCInfo.ttcCenter = float.MaxValue;
			minTTCInfo.distance = -1f;
			minTTCInfo.inside = false;



			for (int j = 0; j < approaching.Count; j++) {



				VehicleInfo i = approaching [j].gameObject.GetComponentInParent<VehicleInfo> ();

				
				CollisionPrediction.TTCInfo ttcInfo = CollisionPrediction.ComputeTimeToCollision (ailogic.vehicleInfo, i);
				//	ailogic.Log ("ttc=" + ttc + "inside="+inside);


				if (ttcInfo.ttcSphere < 0) {
					if (ttcInfo.inside) {
		

						insideList.Add (i);
						insideTTCList.Add (ttcInfo);
						//SetLeader (i, ttcInfo.distance, LeaderReason.Inside);
						if (IsCloseVehicleInFront (i)) {
							//Select the vehicle with us as leader to facilitate resolving locks
							if (insideFrontVehicle == null) {
								insideFrontVehicle = i;
								SetLeader (i, ttcInfo.distance, LeaderReason.InFrontNotBlocking);
							} else {
								if (i.leadingVehicle == ailogic.vehicleInfo) {
									insideFrontVehicle = i;
									SetLeader (i, ttcInfo.distance, LeaderReason.InFrontNotBlocking);
								}
							}

						} else {
							//If it is changing lane, let it pass
							if (i.currentActionState == VehicleInfo.VehicleActionState.ChangingLane || i.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane) {
								if (i.targetLaneChange == ailogic.currentLane) {
									if (insideFrontVehicle == null) {
										SetLeader (i, ttcInfo.distance, LeaderReason.InsideAndChangingLane);

									}

								}
							} 
						}
							
					}
				} else {

					if (ttcInfo.ttcSphere <= maxCollisionTimeToReact) {

						
						if (minTTCInfo.ttcSphere > ttcInfo.ttcSphere) {
									
							//ailogic.Log (75, "set leadin closest by ttc " + i.vehicleId + "frontTTC="+frontTTC+ "ttc="+ttc);
							//ailogic.Log ( "set leadin closest by ttc " + i.vehicleId + "ttc="+ttc);
							//ailogic.Log ( "set leadin closest by ttc " + i.vehicleId);
							//if (IsCollisionInFront (i, ttc)) {
							Vector3 cpoint = CollisionPrediction.GetCollisionPoint (ailogic.vehicleInfo, i, ttcInfo.ttcCenter);
							if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (cpoint).z >= 0) {
								Debug.DrawLine (ailogic.vehicleInfo.carBody.position, cpoint, Color.red);
								minTTCInfo = ttcInfo;
								minTTCVehicle = i;
								//SetLeader (i, ttcInfo.distance, LeaderReason.ApproachingWithLowTTC);
							}




						}

					} else {
						//if (leader == null) {
						//	reason = LeaderReason.ApproachingNotDangerous;
						//}
					}
				} 



			}


	


			//Now decide

			if (insideFrontVehicle != null || insideWantChangeVehicle != null) {
				//Already selected 

			}

			VehicleInfo temptativeLeader = null;
			reason = LeaderReason.None;
			float distance = -1f;
			//First, select front as temptaive
			if (front) {
				temptativeLeader = frontVehicle;
				reason = LeaderReason.Front;
				distance = frontHit.distance;
			}

			//Second, change between front and signaling
			//Second, change between front and signaling
			if (frontVehicleSignal) {



				//Check if the target is my lane 
				if (signalingVehicle.targetLaneChange == ailogic.currentLane) {
					//TODO: a politeness factor may be included here
					if (front) {


						if (frontHit.distance > giveWaySafetyGap) {
							if (frontHit.distance > signalingVehicleDistance) {
								temptativeLeader = signalingVehicle;
								reason = LeaderReason.SignalingCloserThanFront;
								distance = signalingVehicleDistance;
							} 
						}


					} else {
						temptativeLeader = signalingVehicle;
						reason = LeaderReason.SignalingAndNoFront;
						distance = signalingVehicleDistance;

					}
				}
			}


			//Now compare with the minTTC
			if (approaching.Count > 0) {
				if (temptativeLeader == null) {
					temptativeLeader = minTTCVehicle;
					reason = LeaderReason.ApproachingWithLowTTC;
					distance = minTTCInfo.distance;
				} else {
					if (minTTCVehicle != null) {
						if (minTTCVehicle != temptativeLeader) {
							//Compute TTC  if not computed for signaling and front
							CollisionPrediction.TTCInfo ttcInfo = CollisionPrediction.ComputeTimeToCollision (ailogic.vehicleInfo, temptativeLeader);

							if (ttcInfo.ttcSphere < 0) {
								if (ttcInfo.inside) {
									//insideList.Add (temptativeLeader);
									ailogic.Log ("signaling or front inside" + temptativeLeader.vehicleId + " minTTC=" + minTTCVehicle.vehicleId);

								}
							} else {
								if (ttcInfo.ttcSphere > minTTCInfo.ttcSphere) {
									temptativeLeader = minTTCVehicle;
									reason = LeaderReason.ApproachingWithLowTTC;
									distance = minTTCInfo.distance;
								} else {
									//Change to new minTTC;
									minTTCInfo = ttcInfo;
								}
							}
						}
					}
				}
			}

			//What to do with inside list?
			if (insideList.Count > 0 && temptativeLeader == null) {
				//Change the reason to inform
				reason =LeaderReason.InsidePassedMe;
			}
			SetLeader (temptativeLeader, distance, reason);



		}


		protected bool SelectLeaderWithoutCloseVehiclesWithTrajectoryIntersection ()
		{

			//Collect surrounding information

			//Get vehicles from  CollisionPredictonTrigger
			List<Collider> surrounding = ailogic.vision.GetVehiclesInPotentialIntersection ();

			//Additionally, always check front
			RaycastHit frontHit;
			bool front = ailogic.vision.CheckFrontForVehicle (out frontHit);

			//Check signaling vehicles
			bool frontVehicleSignal=FindSignalingVehiclesInFront();

			//Take a look at where we are heading
			RaycastHit h;
			bool hit;
			hit = ailogic.vision.CheckPositionForVehicle (out h, ailogic.vision.steerLookAtPoint);
			LeaderReason reason = LeaderReason.None;
			bool insideVehicles = false;

			if (hit == false && frontVehicleSignal == false && surrounding.Count == 0 && front == false && ailogic.vision.checkVehiclePositions == false) {
				//No one close to me
				//ailogic.Log("unset no one close");

				SetLeader (null, -1f, LeaderReason.NoneFound);
				return insideVehicles;
			}

			VehicleInfo info;

			float angle = -1f;

			//Find the approaching vehicles
			insideFrontVehicle = null;
			insideWantChangeVehicle = null;
			approaching.Clear ();
			insideList.Clear ();
			minTTCVehicle = null;
			frontVehicle = null;



			if (hit) {
				info = h.collider.gameObject.GetComponentInParent<VehicleInfo> ();
				int index = approaching.IndexOf (info);
				if (index == -1) {
					approaching.Add (info);
				}
			}


			for (int i = 0; i < surrounding.Count; i++) {
				info = surrounding [i].gameObject.GetComponentInParent<VehicleInfo> ();
				//float angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, info.carBody.forward);



				int index = approaching.IndexOf (info);
				if (index == -1) {
					approaching.Add (info);
				}

			}

			//Someone has warned us to check some positions for vehicles
			if (ailogic.vision.checkVehiclePositions) {

				for (int i = 0; i < ailogic.vision.checkVehiclePositionsList.Count; i++) {


					//Check only those in front of me
					if (ailogic.vehicleInfo.carBody.InverseTransformPoint (ailogic.vision.checkVehiclePositionsList [i].position).z >= 0) {


						hit = ailogic.vision.CheckPositionForVehicle (out h, ailogic.vision.checkVehiclePositionsList [i].position);


						if (hit) {
							info = h.collider.gameObject.GetComponentInParent<VehicleInfo> ();
							angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, info.carBody.forward);
							//ailogic.Log(0,  "checking approaching=" +h.transform.root.name+"with angle"+angle);
							if (angle >= angleForApproaching) {
								//It is heading me, approaching
								int index = approaching.IndexOf (info);
								if (index == -1) {
									approaching.Add (info);
								}
								//ailogic.Log( " approaching=" +h.transform.root.name);
							}


						}
					}
				}
			}




			if (front) {

				frontVehicle = frontHit.collider.gameObject.GetComponentInParent<VehicleInfo> ();

				angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward, frontVehicle.carBody.forward);
				if (angle > angleForHeading) {

					//Use trajectory intersection
					int index = approaching.IndexOf (frontVehicle);
					if (index == -1) {
						approaching.Add (frontVehicle);

					}
					frontVehicle = null;
					front = false;

				}
			}




			//Get the minTTC and  or inside caution sphere vehicles


			CollisionPrediction.TTCInfo minTTCInfo;
			minTTCInfo.ttcSphere = float.MaxValue;
			minTTCInfo.ttcCenter = float.MaxValue;
			minTTCInfo.distance = -1f;
			minTTCInfo.inside = false;



			for (int j = 0; j < approaching.Count; j++) {



				VehicleInfo i = approaching [j].gameObject.GetComponentInParent<VehicleInfo> ();
				/*if (ailogic.vehicleInfo.vehicleId == 15 || ailogic.vehicleInfo.vehicleId == 9) {
					CollisionPrediction.ComputeVelocityObstacle (ailogic.vehicleInfo, i, 5f);

				}*/

			

				CollisionPrediction.TTCInfo ttcInfo = CollisionPrediction.ComputeTimeToCollision (ailogic.vehicleInfo, i);
				//	ailogic.Log ("ttc=" + ttc + "inside="+inside);


				if (ttcInfo.ttcSphere < 0) {
					if (ttcInfo.inside) {
						insideVehicles = true;
						//Just add to list but do not decide
						insideList.Add (i);
						insideTTCList.Add (ttcInfo);
						ailogic.Log ("Vehicle=" + i.vehicleId + ". ttcInfo.ttcSphere=" + ttcInfo.ttcSphere + "ttcCenter=" + ttcInfo.ttcCenter, showLog);
				

					}
				} else {

					if (ttcInfo.ttcSphere <= maxCollisionTimeToReact) {


						if (minTTCInfo.ttcSphere > ttcInfo.ttcSphere) {

							//ailogic.Log (75, "set leadin closest by ttc " + i.vehicleId + "frontTTC="+frontTTC+ "ttc="+ttc);
							//ailogic.Log ( "set leadin closest by ttc " + i.vehicleId + "ttc="+ttc);
							//ailogic.Log ( "set leadin closest by ttc " + i.vehicleId);
							//if (IsCollisionInFront (i, ttc)) {
							Vector3 cpoint = CollisionPrediction.GetCollisionPoint (ailogic.vehicleInfo, i, ttcInfo.ttcCenter);
							if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (cpoint).z >= 0) {
								Debug.DrawLine (ailogic.vehicleInfo.carBody.position, cpoint, Color.red);
								minTTCInfo = ttcInfo;
								minTTCVehicle = i;
								//SetLeader (i, ttcInfo.distance, LeaderReason.ApproachingWithLowTTC);
							}



							//ailogic.Log (75, "is leader" + i.vehicleId);
							//ailogic.Log ( "is leader" + i.vehicleId);
							//}
						}

					} 
				} 



			}



			//Now decide


			VehicleInfo temptativeLeader = null;
			reason = LeaderReason.None;
			float distance = -1f;
			//First, select front as temptaive
			if (front) {
				temptativeLeader = frontVehicle;
				reason = LeaderReason.Front;
				distance = frontHit.distance;
			}

			//Second, change between front and signaling
			if (frontVehicleSignal) {

			

				//Check if the target is my lane 
				if (signalingVehicle.targetLaneChange == ailogic.currentLane) {
					//TODO: a politeness factor may be included here
					if (front) {


						if (frontHit.distance > giveWaySafetyGap) {
							if (frontHit.distance > signalingVehicleDistance) {
								temptativeLeader = signalingVehicle;
								reason = LeaderReason.SignalingCloserThanFront;
								distance = signalingVehicleDistance;
							} 
						}


					} else {
						temptativeLeader = signalingVehicle;
						reason = LeaderReason.SignalingAndNoFront;
						distance = signalingVehicleDistance;

					}
				}
			}


			//Now compare with the minTTC
			if (approaching.Count > 0) {
				if (temptativeLeader == null) {
					if (minTTCVehicle != null) {
						temptativeLeader = minTTCVehicle;
						reason = LeaderReason.ApproachingWithLowTTC;
						distance = minTTCInfo.distance;
					}
				} else {
					if (temptativeLeader == frontVehicle || temptativeLeader == signalingVehicle) {
						//Compute TTC  if not computed for signaling and front
						CollisionPrediction.TTCInfo ttcInfo = CollisionPrediction.ComputeTimeToCollision (ailogic.vehicleInfo, temptativeLeader);
						if (ttcInfo.ttcSphere < 0) {
							if (ttcInfo.inside) {
								insideList.Add (temptativeLeader);
							}
						} else {
							if (ttcInfo.ttcSphere <= maxCollisionTimeToReact) {


								if (minTTCInfo.ttcSphere > ttcInfo.ttcSphere) {

									Vector3 cpoint = CollisionPrediction.GetCollisionPoint (ailogic.vehicleInfo, temptativeLeader, ttcInfo.ttcCenter);
									if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (cpoint).z >= 0) {
										Debug.DrawLine (ailogic.vehicleInfo.carBody.position, cpoint, Color.red);
										minTTCInfo = ttcInfo;
										minTTCVehicle = temptativeLeader;
									}



								}

							} 
						}
					}
					if (minTTCVehicle != null) {

						temptativeLeader = minTTCVehicle;
						reason = LeaderReason.ApproachingWithLowTTC;
						distance = minTTCInfo.distance;
								
					}
				}
			}

		
			SetLeader (temptativeLeader, distance, reason);
			return insideVehicles;


		}



		public bool IsCollisionInFront (VehicleInfo i, float ttc)
		{
			//Determine collision point
			Vector3 cp = ailogic.vehicleInfo.frontBumper.InverseTransformPoint (CollisionPrediction.GetCollisionPoint (ailogic.vehicleInfo, i, ttc));
		
			if (cp.z > 0) {
				return true;
			} else {
				return false;
			}


		}

		public bool IsVehicleCollidingInTheFuture(VehicleInfo i, int steps) {
			Vector3 oldpos = i.carCollider.transform.position;
			//Advance
			i.carCollider.transform.position += i.velocity*(steps*Time.fixedDeltaTime);
			Vector3 center=ailogic.vehicleInfo.carBody.position + ailogic.vehicleInfo.velocity*(steps*Time.fixedDeltaTime);
			int hits = ailogic.vision.CheckPositionOccupiedByVehicle (center, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {
				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == i) {
					i.carCollider.transform.position = oldpos;
					return true;
				}


			}
			i.carCollider.transform.position = oldpos;
			return false;

		}
		public bool IsVehicleCollidingAtPostionInTheFuture(VehicleInfo i, Vector3 otherPosition, Vector3 myPosition) {
			Vector3 oldpos = i.carCollider.transform.position;
			//Advance
			i.carCollider.transform.position = otherPosition;

			int hits = ailogic.vision.CheckPositionOccupiedByVehicle (myPosition, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {
				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == i) {
					i.carCollider.transform.position = oldpos;
					return true;
				}


			}
			i.carCollider.transform.position = oldpos;
			return false;

		}

		public bool IsCloseVehicleBlocking (VehicleInfo i)
		{

			//Check if it is facing us first

			if (Vector3.Angle(ailogic.vehicleInfo.carBody.forward,i.carBody.forward)<=90f) {
				return false;
			}
			//Assume we already know it is in front of us
			//Now check the other 
			Vector3 center = i.carBody.TransformPoint (new Vector3 (0f, 0.6f, i.vehicleLength*0.5f));
		
			//TODO: should we clear the array first

			int hits = i.aiLogic.vision.CheckPositionOccupiedByVehicle (center, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {
				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == ailogic.vehicleInfo) {
					
					return true;
				}
					

			}

			return false;


		}

		public bool IsSteeringPointSafe(Vector3 steerPoint) {
			
			//int hits = ailogic.vision.CheckPositionOccupiedByVehicle (steerPoint, leaderBuffer);
			int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (steerPoint, leaderBuffer);
			if (hits == 0) {
				return true;
			}
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {
				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () != ailogic.vehicleInfo) {

					return false;
				}


			}

			return true;
		}

		public bool IsCloseVehicleInFront (VehicleInfo i)
		{

			//Vehicles are very close (inside sphere of collision). 
	

			//Check if vehicle is in front of us

			Vector3 center = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (0f, 0.6f, ailogic.vehicleInfo.vehicleLength*0.5f));
			//Vector3 center =  ailogic.vehicleInfo.carBody.position + 5f*Time.fixedDeltaTime*ailogic.vehicleInfo.velocity;
			//Vector3 center = CollisionPrediction.GetCollisionPoint(ailogic.vehicleInfo,i,ttc);
			//TODO: should we clear the array first

			int hits = ailogic.vision.CheckPositionOccupiedByVehicle (center, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {
				
			
				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == i) {
					return true;
				}

			}
			return false;


		}

		public bool IsCloseVehicleInVelocityTrajectory (VehicleInfo i,float turningRadius, float steps)
		{
			float angle;
			if (float.IsInfinity(turningRadius)) {
				angle = 0.0f;

			} else {

				angle = ailogic.vehicleInfo.carController.vLat * steps * Time.deltaTime / Mathf.Abs (turningRadius);
			}
			Vector3 position = ailogic.vehicleInfo.carBody.position + ailogic.vehicleInfo.velocity * steps * Time.deltaTime;
			ailogic.Log ("turningRadius=" + turningRadius + "vLat=" + ailogic.vehicleInfo.carController.vLat + "angle=" + Mathf.Rad2Deg*angle + "lpos"+position, showLog);
			//Quaternion relRotation = Quaternion.AngleAxis (Mathf.Rad2Deg*angle, Vector3.up);

			int hits = ailogic.vision.CheckRotatedPositionOccupiedByVehicle (position,Mathf.Rad2Deg*angle, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {


				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == i) {
					return true;
				}

			}
			return false;
		}
		public bool IsCloseVehicleInFront (VehicleInfo i, out Collider col)
		{

			//Vehicles are very close (inside sphere of collision). 
			//Decide if it is in front of me and heading me
		

			//Check if vehicle is in front of us

			Vector3 center = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (0f, 0.6f, ailogic.vehicleInfo.vehicleLength*0.5f));
			//Vector3 center =  ailogic.vehicleInfo.carBody.position + 5f*Time.fixedDeltaTime*ailogic.vehicleInfo.velocity;
			//Vector3 center = CollisionPrediction.GetCollisionPoint(ailogic.vehicleInfo,i,ttc);
			//TODO: should we clear the array first

			int hits = ailogic.vision.CheckPositionOccupiedByVehicle (center, leaderBuffer);
			//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
			if (hits > leaderBuffer.Length) {
				hits = leaderBuffer.Length;
				ailogic.Log ("Leader buffer short");
			}
			for (int j = 0; j < hits; j++) {


				if (leaderBuffer [j].gameObject.GetComponentInParent<VehicleInfo> () == i) {
					col = leaderBuffer [j];
					return true;
				}

			}
			col = null;
			return false;


		}
	}


}
