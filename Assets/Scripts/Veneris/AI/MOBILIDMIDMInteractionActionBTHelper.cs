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
	public class MOBILIDMIDMInteractionActionBTHelper : IDMInteractionBTHelper
	{

		public bool changingLane=false;

		public float mobilBsafe = -4f; //MOBIL bsafe parameter with changed sign
	

		public LaneChangeRequest pendingRequest = null;

		public Transform vehicleTrigger;
		public Collider myCarCollider =null;
		public Collider[] followerBuffer = null;


		private RaycastHit hit;
		void Start ()
		{
			Init ();

		
		}

		protected override void Init ()
		{
			base.Init ();

			foreach (Collider c in ailogic.vehicleInfo.vehicleColliders) {
				if (c.transform.name.Equals ("VehicleTrigger")) {
					vehicleTrigger = c.transform;
					break;
				}
			}
			//vehicleLayer = LayerMask.NameToLayer ("Vehicle");
			myCarCollider = ailogic.vehicleInfo.FindColliderByTagName("CarCollider");
			followerBuffer = new Collider[1];

		}

		public void SetLaneChangeRequest(LaneChangeRequest request) {
			pendingRequest = request;
			
		}
		public void CancelLaneChangeRequest(LaneChangeRequest request) {
			if (pendingRequest == request && pendingRequest!=null) {
				if (pendingRequest.isExecutingManeuver==false) {
					ailogic.DeactivateSignalTrigger ();
				}
				FinishLaneChangeManeuver ();
				pendingRequest = null;
			}

		}


		public bool IsLaneChangePossible () {
			if (changingLane) {
				return true;
			} else if (pendingRequest != null) {
				Vector3 center;
				LaneChangeDirection direction = SelectAndActivateSignalTrigger (out center);
				if (direction == LaneChangeDirection.Left) {
					if (ailogic.vision.GetVehiclesInLeftSafetyArea ().Count > 0) {
						//Not possible now
						//return false;
						VehicleSafetyAreaDetector detector = ailogic.vision.GetVehiclesInLeftSafetyArea () [0].GetComponent<VehicleSafetyAreaDetector> ();
						pendingRequest.follower = detector.info;
						return ComputeSafetyCriterion (detector.info);
					}
				} else if (direction == LaneChangeDirection.Right) {
					if (ailogic.vision.GetVehiclesInRightSafetyArea ().Count > 0) {
						//Not possible now
						//return false;
						VehicleSafetyAreaDetector detector = ailogic.vision.GetVehiclesInRightSafetyArea () [0].GetComponent<VehicleSafetyAreaDetector> ();
						pendingRequest.follower = detector.info;
						return ComputeSafetyCriterion (detector.info);
					}
					
				} else {
					return true;
				}

				VehicleInfo info = FindFollowerOnTargetLane (center);
				if (info == null) {
					//No follower, just change
					pendingRequest.follower = info;

					pendingRequest.followerPosition = LaneChangeRequest.LaneChangeFollowerPosition.None;


					return true;
				} else {
					pendingRequest.follower = info;
					return ComputeSafetyCriterion (info);
				
				}
			}
			return false;
		}
		public FluentBehaviourTree.BehaviourTreeStatus IsLaneChangeFinished () {
			if (changingLane) {
				if (pendingRequest.targetLane.IsOnLane (ailogic.vehicleInfo.backBumper)) {
					

					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				} else {
					return FluentBehaviourTree.BehaviourTreeStatus.Running;
				}
			} 
			return FluentBehaviourTree.BehaviourTreeStatus.Failure;
		}

		protected bool ComputeSafetyCriterion(VehicleInfo info) {

			//Other backbumper is in front of my frontBumper

			if (ailogic.vehicleInfo.frontBumper.InverseTransformPoint (info.backBumper.position).z > 0) {
				//ailogic.Log ("In front of me, not safe ");
				pendingRequest.followerPosition = LaneChangeRequest.LaneChangeFollowerPosition.InFront;
				return false;
			} 
			//Other frontbumper is behind my backBumper
			if (ailogic.vehicleInfo.backBumper.InverseTransformPoint (info.frontBumper.position).z < 0) {
				pendingRequest.followerPosition = LaneChangeRequest.LaneChangeFollowerPosition.Behind;
				//Compute the IDM acceleration the other vehicle would experience
				//TODO: we should use our position and velocity in the new lane after the change but, in practice, there seems to be little difference, since
				// for short distances d is approx equal, and for large distances with the follower, the result is the same

				float speed = info.speed;
				float dec = 0.0f;
				//float decp = 0.0f;

				//float deltaSpeed = speed - ailogic.vehicleInfo.speed;
				float vectordeltaspeed = speed - Vector3.Dot (transform.forward, info.velocity);
				//TODO: here we are assuming that the follower has the same IDM parameters, consider changing this
				float d = Vector3.SqrMagnitude (info.frontBumper.position - ailogic.vehicleInfo.backBumper.position);
				//float prosqrd = 0.0f;
				//prosqrd = Vector3.Dot (transform.forward, (info.backBumper.position - ailogic.vehicleInfo.frontBumper.position));
				float s_star = idmJamDistance + (speed * idmSafetyGap) + ((speed * vectordeltaspeed) / (2 * Mathf.Sqrt (idmA * idmB)));
				//float s_star = idmJamDistance + speed * idmSafetyGap + (speed * deltaSpeed) / (2 * Mathf.Sqrt (idmA * idmB));
				s_star = s_star * s_star;

				dec = -idmA * (s_star / d);
				//decp = -idmA * (s_star / (prosqrd * prosqrd));

				//Free acceleration a[1-(v(vo)^delta] or -b[1-(vo/v)^delta]
				float freeA = 0.0f;
				if (idmAccelerationExponent < 0) {
					//Use 4 as defautl
					freeA = (speed / freeSpeed) * (speed / freeSpeed) * (speed / freeSpeed) * (speed / freeSpeed);
				} else {
					freeA = Mathf.Pow (speed / freeSpeed, idmAccelerationExponent);
				}
				if (speed <= freeSpeed) {

					freeA = idmA * (1f - freeA);

				} else {
					freeA = -idmB * (1f - (1f / freeA));
				}
				float nac = freeA + dec;

				if (nac >= mobilBsafe) {
					//ailogic.Log ("Safety criterion: safe " + nac+ " for "+ info.vehicleId);
					return true;
				} else {
					//ailogic.Log ("Safety criterion:  not safe " + nac +" for "+ info.vehicleId);
					return false;
				}

			}
			//Otherwise consider it is parallel

			pendingRequest.followerPosition = LaneChangeRequest.LaneChangeFollowerPosition.Parallel;
			return false;


		}



		public void StartLaneChangeManeuver() {
			//Make our vehicle look at the new path in order to turn to it
			//ailogic.Log("StartLaneChangeManeuver");
			//ailogic.routeManager.SetCurrentPathFromLaneChange(pendingRequest.targetPath);
			ailogic.routeManager.SetLookAtPath(pendingRequest.targetPath);
			ailogic.routeManager.StartLaneChange (pendingRequest.startPath, pendingRequest.targetPath);
			pendingRequest.isExecutingManeuver = true;
			changingLane = true;
			ailogic.DeactivateSignalTrigger ();
			ailogic.vehicleInfo.SetChangingLane ();
		
		}

		public void FinishLaneChangeManeuver() {
			
			changingLane = false;
			pendingRequest.isExecutingManeuver = false;
			pendingRequest.isFinished = true;
			ailogic.vehicleInfo.UnsetChangingLane ();
			ailogic.TurnOffSignalLaneChange ();




			
		}
		public LaneChangeDirection SelectAndActivateSignalTrigger(out Vector3 center) {
			if (pendingRequest.targetLane.laneId > ailogic.currentLane.laneId) {
				//Target lane is on our left
				//We have to advance the box, because BoxCast does not return a hit if the target is inside the box a the beginning (or maybe the raycast is checked starting from the box border, who knows...)

				center = ailogic.vehicleInfo.frontBumper.TransformPoint (new Vector3 (-ailogic.currentLane.laneWidth, 0, 5f));
				ailogic.ActivateSignalTrigger(ailogic.vehicleInfo.carBody.TransformPoint(new Vector3 (-ailogic.currentLane.laneWidth, 0, 2.5f)));
				ailogic.TurnOnSignalLaneChange (LaneChangeDirection.Left, pendingRequest.targetLane);
				return LaneChangeDirection.Left;
				//center = vehicleTrigger.TransformPoint (new Vector3 (-ailogic.currentLane.laneWidth,0,0));
			} else if (pendingRequest.targetLane.laneId < ailogic.currentLane.laneId) {
				center = ailogic.vehicleInfo.frontBumper.TransformPoint (new Vector3 (ailogic.currentLane.laneWidth, 0, 5f));
				ailogic.ActivateSignalTrigger(ailogic.vehicleInfo.carBody.TransformPoint(new Vector3 (ailogic.currentLane.laneWidth, 0, 2.5f)));
				ailogic.TurnOnSignalLaneChange (LaneChangeDirection.Right,pendingRequest.targetLane);
				return LaneChangeDirection.Right;
				//center = vehicleTrigger.TransformPoint (new Vector3 (ailogic.currentLane.laneWidth,0,0));
			} else {
				//I am already on the same lane
				//TODO: try change and let IDM?
				//ailogic.Log("already on the same lane");

				center = ailogic.vehicleInfo.frontBumper.TransformPoint(new Vector3 (0, 0, 5f));
				return LaneChangeDirection.None;
			}
		}
		protected VehicleInfo FindFollowerOnTargetLane(Vector3 center ) {
			//Cast a box to find our followers and leaders in the lane
			//Simulate looking at the rearviewmirror
			//TODO: change to nonalloc when a number of expected results is tested. Put an appropriate physics layer here

			//Just raycast back from our position translated to the corresponding lane

			int clindex=pendingRequest.targetLane.paths[0].FindClosestPointInInterpolatedPath(center);
		


			//First, a boxcast does not detect a collider if it starts inside it, so we have first to check the advanced position

			if (vision.CheckPositionOccupiedByVehicle (center, followerBuffer) > 0) {
				//ailogic.Log ("ChangeLane position occupied " + followerBuffer [0].transform.GetComponentInParent<VehicleInfo> ().vehicleId);
				if (followerBuffer [0] != myCarCollider) {
					return followerBuffer [0].transform.GetComponentInParent<VehicleInfo> ();
				}
			} 

			if (vision.CheckLineForVehicle(out hit, center,-pendingRequest.targetLane.paths[0].interpolatedPath[clindex].tangent, ailogic.vehicleInfo.carBody.rotation)) {
				if (hit.collider == myCarCollider) {
					//ailogic.Log ("I am hitting my own collider when looking for followers");
					return null;
				}
			


					
					return (hit.transform.GetComponentInParent<VehicleInfo>());

				
			} 

			//ailogic.Log ("no follower");

			if (vision.CheckPositionOccupiedByVehicleSignal (center, followerBuffer) > 0) {
				//ailogic.Log ("ChangeLane position occupied " + followerBuffer [0].transform.GetComponentInParent<VehicleInfo> ().vehicleId);
				if (followerBuffer [0] != myCarCollider) {
					return followerBuffer [0].transform.GetComponentInParent<VehicleInfo> ();
				}
			} 


			if (vision.CheckLineForVehicleSignal(out hit, new Vector3 (ailogic.currentLane.laneWidth, 0, 2.5f),-pendingRequest.targetLane.paths[0].interpolatedPath[clindex].tangent, ailogic.vehicleInfo.carBody.rotation)) {
	
				return (hit.transform.GetComponentInParent<AILogic>().vehicleInfo);

			} 
			//ailogic.Log ("No signal detected");
			//Activate now to avoid collisions with CheckLineForVehicleSignal, or boxcastall
					
			return null;

		}
		protected bool CheckIfOtherVehicleOnTargetLane(VehicleInfo i) {
			if (i.laneId == ailogic.currentLane.laneId && i.roadId == ailogic.currentRoad.roadId && i.roadEdgeId == ailogic.currentRoad.edgeId) {
				return true;
			} else {
				return false;
			}
		}



	}
}
