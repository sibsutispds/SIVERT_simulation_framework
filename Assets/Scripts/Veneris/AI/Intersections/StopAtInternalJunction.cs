/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluentBehaviourTree;


namespace Veneris
{
	public class StopAtInternalJunction : Stop
	{
		//TODO: assuming only one internal stop is possible. Check if there are intersections with chains of several internal stops following each other

		public bool internalStopReached = false; //true when we reach an internal stop
		public List<Path> internalPaths=null;
		public Transform internalStopPosition = null;
		public GameObject sphere = null;
		public bool showLog = false;
		public Collider[] proximityBuffer = null;
	
		public float maxTimeAfterInternalStop = 90f;//These intersections are very problematic. If we are waiting too long, there is probably some block
		public float internalStopTimerStart=-1f;



		public float disR = 0.0f;
		public Average crossingSpeed;
		public float initCrossing=0.0f;
		public override void ActivateBehaviour ()
		{
			base.ActivateBehaviour ();

			//Recover state
			if (intersectionStopLineReached == false) {
				//Make sure we are not already on the intersection
				if (ailogic.currentIntersection != intersection) {
					float distanceToStopLine; 
					float desiredSpeed ;
					if (ComputeDistanceToStopLine ( out distanceToStopLine)) {
						desiredSpeed = Mathf.Clamp (1.4f * throttleHelper.ComputeMaxCorneringSpeed (internalPath.maxCurvature), 0.5f, ailogic.currentLane.speed);

					} else {
						ailogic.Log("cannot find a path to the stop line " + stopLinePosition.name + " of " + stopLinePosition.parent.name);
						desiredSpeed = 0.5f;
						distanceToStopLine = (ailogic.vehicleInfo.carBody.position - stopLinePosition.position).magnitude;
						//throw new UnityException ();
					}
					throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, desiredSpeed, ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled, stopLineCollider);
					throttleHelper.SetSpeedAtPoint (throttleGoal); 
					throttleHelper.SetSpeedLimit (ailogic.currentLane.speed);
				} else {
					intersectionStopLineReached = true;
				}

			} else if ( internalStopReached==false) {
				SetNextPathSpeedLimit ();
				SetCrossingIntersectionState();
			} else if (internalLaneEndReached == false) {
				SetNextPathSpeedLimit ();
				SetCrossingIntersectionState();
			}
			CheckColliders ();
		}
		public override void DeactivateBehaviour ()
		{
			base.DeactivateBehaviour ();

		}

		public IBehaviourTreeNode StopAtInternalJunctionTree() {
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
			Parallel("do stop at internal junction",3,1).
				Do("Anti-blocking timer", ()=>CheckMaximumTimeAtIntersection()).
				Sequence("Execute crossing sequence").
					ExecuteUntilSuccessNTimes("check priority positions once",1). //Only run succesfully once the following sequence
						Sequence("check priority positions").
				//Condition(" has reached intersection stop?",t=>{return (throttleHelper.HasReachedStopPoint() && intersectionStopLineReached==true);}). //Do not need to stop at the stop line
							Condition(" has reached intersection stop line?",()=>{return (intersectionStopLineReached==true);}). //Do not need to stop at the stop line							
							Do("Wait if jammed", ()=>WaitIfJammed()).
							Do("Check traffic light again", ()=>ReCheckTrafficLight()).
							Do("start no-block intersection timer",()=>StartIntersectionTimer()).
							Do("Set throttle mode to stop at internal position", ()=>SetStopAtInternalPosition()).
//							Do("change to adapt to curvature",t=>SetAdaptToCurvature()).
							Do("keep checking for vehicles with priority in the intersection",()=>SetDrivingCheckPositions(priorityCheckPositions)).
							Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
							Do("set current action in vehicle info",()=>SetCrossingIntersectionState()).
						End().
					End().
					ExecuteUntilSuccessNTimes("check priority positions at internal stop once",1).//Once we have decide to cross, we go on
						Sequence("check priority positions at internal stop").
							Condition("has reached internal stop and has stopped?",()=>HasReachedInternalStopAndStopped()).
							Do("WaitUntilClearedWithCollisionPrediction",()=>WaitUntilClearedWithCollisionPrediction()).
							Do("keep checking for vehicles with priority in the intersection",()=>SetDrivingCheckPositions(priorityCheckPositions)).
							Do("change to adapt to curvature",()=>SetAdaptToCurvature()).
							Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
							Do("set current action in vehicle info",()=>SetCrossingIntersectionState()).
				//Do("set next path speed limit",t=>SetNextPathSpeedLimit()).
				//Do ("change to default-behaviour", t => SetDefault ()).
						End().
					End().
					Sequence("drive until end of intersection").
						Condition("reached end of intersection?",()=>{//crossingSpeed.Collect(ailogic.vehicleInfo.speed);
																		return internalLaneEndReached==true;}).
						//Do("Collect speed", ()=>{ailogic.Log("speed="+ailogic.vehicleInfo.speed+"delatt="+(Time.time-initCrossing)+"deltad="+(ailogic.vehicleInfo.totalDistanceTraveled-disR)+"avSpe="+crossingSpeed.Mean());return FluentBehaviourTree.BehaviourTreeStatus.Success;}).
						Do("set next path speed limit",()=>SetNextPathSpeedLimit()).
				//Do("Log behaviour",t=>{ailogic.Log(11,"drive until end of intersection " +intersection.name); return FluentBehaviourTree.BehaviourTreeStatus.Success;}).
						Do("Apply behaviour",()=>SetApplyBehaviour()).
						Do("stop checking for vehicles with priority in the intersection",()=>UnsetDrivingCheckPositions()).
						Do ("change to default-behaviour", () => SetDefault ()).
					End().
				End().
				Splice(ailogic.defaultBehaviour.mainBehaviour). //Drive with default behaviour until the end
			End (). //parallel
			Build ();
		}


		public override void Prepare ()
		{
			base.Prepare ();
			if (internalPath == null) {
				Debug.LogError ("internalPath is null");
			}
			if (stopLinePosition == null) {
				Debug.LogError ("stopLinePosition is null");
			}


			action = IntersectionAction.StopAtInternalJunction;

			behaviourName="StopAtInternalJunction at intersection "+intersection.name;
			mainBehaviour = StopAtInternalJunctionTree();
			proximityBuffer = new Collider[2];
			/*float distanceToStopLine; 
			if (ComputeDistanceToStopLine (stopLinePosition, plannedPath, out distanceToStopLine)) {
				throttleGoal = new ThrottleGoalForPoint (stopLinePosition.position, Mathf.Clamp (1.4f * throttleHelper.ComputeMaxCorneringSpeed (internalPath.maxCurvature), 0.5f, ailogic.currentLane.speed), ailogic.currentLane.speed, distanceToStopLine, ailogic.vehicleInfo.totalDistanceTraveled);

			} else {

				throw new UnityException ();
			}
			throttleHelper.SetStopAtPoint(throttleGoal); 
			throttleHelper.SetSpeedLimit(ailogic.currentLane.speed);
			*/
			ailogic.vehicleTrigger.AddEnterListener (HandleEnterVehicleTrigger);
			//Call at the end to let traffic light tracker work
			//base.Prepare ();
			SetApproachActionAndPriority();
		}

		public bool HasReachedInternalStopAndStopped() {
			if (throttleHelper.HasStoppedAtGoalPoint () && internalStopReached == true) {
				//Start timer right now 
				internalStopTimerStart = Time.time;
				return true;
			}
			return false;
		}

	

		public FluentBehaviourTree.BehaviourTreeStatus SetStopAtInternalPosition() {

			throttleGoal = new ThrottleGoalForPoint (internalStopPosition.position, 0.5f, ailogic.currentLane.speed, internalPath.totalPathLength, ailogic.vehicleInfo.totalDistanceTraveled, internalStopPosition.GetComponent<Collider>());

			throttleHelper.SetStopAtPoint(throttleGoal);
			throttleHelper.SetSpeedLimit(ailogic.currentLane.speed);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}
		public FluentBehaviourTree.BehaviourTreeStatus CheckMaximumTimeAtIntersection() {
			if (intersectionTimerStart >=0f) {
				
				if ((Time.time - intersectionTimerStart) > maxTimeAtIntersection) {
					//Teleport
					ailogic.Log ("StopAtInternalJunction::maxTimeAtIntersection " + Time.time + "intersectionTimerStart=" + intersectionTimerStart + "diff=" + (Time.time - intersectionTimerStart));
					//Debug.Break ();

					//if (!ailogic.Teleport ("maxTimeAtIntersection "+intersection.sumoJunctionId,ailogic.routeManager.lookAtPath.pathId, out nextPath)) {
					ailogic.RemoveAndReinsert ("StopAtInternalJunction::maxTimeAtIntersection=" + Time.time + ":intersectionTimerStart=" + intersectionTimerStart + ":Intersection="+intersection.sumoJunctionId);
					//}
				}
			}
			if (internalStopTimerStart >=0f) {
				//Start timer after internal stop, where problems usually occur
				if ((Time.time - internalStopTimerStart) > maxTimeAfterInternalStop) {
					//Teleport
					ailogic.Log ("StopAtInternalJunction::maxTimeAfterInternalStop " + Time.time + "internalStopTimerStart=" + internalStopTimerStart + "diff=" + (Time.time - internalStopTimerStart));
					//Debug.Break ();

					//if (!ailogic.Teleport ("maxTimeAtIntersection "+intersection.sumoJunctionId,ailogic.routeManager.lookAtPath.pathId, out nextPath)) {
					ailogic.RemoveAndReinsert ("StopAtInternalJunction::maxTimeAfterInternalStop=" + Time.time + ":internalStopTimerStart=" + internalStopTimerStart + ":Intersection="+intersection.sumoJunctionId);
					//}
				}
			}
			return FluentBehaviourTree.BehaviourTreeStatus.Success;

		}

		public FluentBehaviourTree.BehaviourTreeStatus WaitUntilClearedWithCollisionPrediction() {

		
			ailogic.vehicleInfo.SetWaitingForClearance ();
			//Debug.Log ("Enter WaitUntilCleared");
			//ailogic.Log( "Enter WaitUntilCleared");


			//First, check if someone is crossing the intersection in our path
			int midindex=Mathf.FloorToInt(internalPath.interpolatedPath.Length*0.5f);
			Vector3 center = internalPath.interpolatedPath [midindex].position;

			/*if (ailogic.vision.CheckVehiclesInSphere(center,Vector3.Distance(ailogic.vehicleInfo.frontBumper.position,center)*0.97f)) {
				//sphereCheck = true;
				GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				sphere.transform.position = center;
				SphereCollider sc = sphere.GetComponent<SphereCollider> ();
				sc.radius=Vector3.Distance(ailogic.vehicleInfo.frontBumper.position,center)*0.97f;
				sc.isTrigger = true;
				//bool sphereCheck = false;
				ailogic.Log("Someone crossing our path. Wait");
				return FluentBehaviourTree.BehaviourTreeStatus.Running;
			}*/
			//Collider[] crossing = ailogic.vision.VehiclesInSphere (center,Vector3.Distance(ailogic.vehicleInfo.frontBumper.position,center)*0.97f);
			//for (int i = 0; i < crossing.Length; i++) {
			//	VehicleInfo ci = crossing [i].gameObject.GetComponentInParent<VehicleInfo> ();
			//	if (ci != ailogic.vehicleInfo) {
					/*GameObject sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
					sphere.transform.position = center;
					SphereCollider sc = sphere.GetComponent<SphereCollider> ();
					sc.radius=Vector3.Distance(ailogic.vehicleInfo.frontBumper.position,center)*0.97f;
					sc.isTrigger = true;*/
					//ailogic.Log("Someone crossing our path. Wait " + ci.vehicleId);

			//		return FluentBehaviourTree.BehaviourTreeStatus.Running;
			//	}
			//}

			/*if (sphereCheck) {
				ailogic.Log ("sphereCheck2=" + sphereCheck);
				return FluentBehaviourTree.BehaviourTreeStatus.Running;
			}*/
			float estimatedTimeToCrossAtFullThrottle = CollisionPrediction.InverseDistanceFullThrottleFittedModel (internalPath.totalPathLength);
			float estimatedAvSpeedAtFullThrottle = CollisionPrediction.AverageSpeedFullThrottleFittedModel (estimatedTimeToCrossAtFullThrottle);
			ailogic.Log ("estimatedTimeToCrossAtFullThrottle=" + estimatedTimeToCrossAtFullThrottle, showLog);


			Collider[] approaching = ailogic.vision.CollisionPredictionInSphere (center,internalPath.totalPathLength*0.5f);
		
			//Create extended sphere and check potential collision
			for (int j = 0; j < approaching.Length; j++) {
				VehicleInfo i = approaching [j].gameObject.GetComponentInParent<VehicleInfo> ();
				if (i.vehicleId == ailogic.vehicleInfo.vehicleId) {
					continue;
				}
				if (i.currentActionState == VehicleInfo.VehicleActionState.WaitingAtRedLight) {
					//Ignore this vehicle, since it is not going to cross
					continue;
				}
				/*if (i.sqrSpeed <= 0.02) {
					//Consider this vehicle stopped and try to go
					continue;
				}*/
				bool inminent;
				Vector3 estimatedVelocity = (center - ailogic.vehicleInfo.carBody.position).normalized * estimatedAvSpeedAtFullThrottle;

				float ttc = CollisionPrediction.ComputeTimeToCollision (estimatedVelocity, i.velocity, ailogic.vehicleInfo.carBody.position,i.carBody.position, 0.7f*(ailogic.vehicleInfo.vehicleLength+i.vehicleLength),out inminent);
				//ailogic.Log ("estimated velocity" + estimatedVelocity + "mag=" + estimatedVelocity.magnitude, showLog);
				if (showLog) {
					CollisionPrediction.ComputeVelocityObstacle (ailogic.vehicleInfo, i, estimatedVelocity, 10);
				}

				//Simulate crossing
				//float ttc2 = CollisionPrediction.ComputeTimeToCollision ((center- ailogic.vehicleInfo.carBody.position)*3f, i.velocity, ailogic.vehicleInfo.carBody.position,i.carBody.position, 0.7f*(ailogic.vehicleInfo.vehicleLength+i.vehicleLength),out inminent);
				//float ttc3 = CollisionPrediction.ComputeTimeToCollision ((center- ailogic.vehicleInfo.carBody.position)*1f, i.velocity, ailogic.vehicleInfo.carBody.position,i.carBody.position, 0.7f*(ailogic.vehicleInfo.vehicleLength+i.vehicleLength),out inminent);
				//ailogic.Log (75, i.vehicleId+"--ttc=" + ttc +"--speed="+i.speed+"dist="+(i.carBody.position-ailogic.vehicleInfo.carBody.position).magnitude + "predictedTime="+CollisionPrediction.InverseDistanceFullThrottleFittedModel ((i.carBody.position-ailogic.vehicleInfo.carBody.position).magnitude));
				if (ttc >= 0 && ttc <=estimatedTimeToCrossAtFullThrottle) {
					if (i.currentActionState == VehicleInfo.VehicleActionState.WaitingForClearance) {
						for (int k = 0; k < i.waitingForVehicleLock.Count; k++) {
							if (i.waitingForVehicleLock [k].vehicleId == ailogic.vehicleInfo.vehicleId) {
								//Waiting for me
								if (!ApplyRightBeforeLeftRule (i.carBody)) {
									ailogic.Log ( "waiting resolve lock for " + i.vehicleId,showLog);


									ailogic.vehicleInfo.waitingForVehicleLock.Add (i);
									return FluentBehaviourTree.BehaviourTreeStatus.Running;
								}
							}
						}
					} else {
						ailogic.Log ( "waiting for ttc " +i.vehicleId + "ttc="+ttc +"velocity="+i.velocity, showLog);
						if (i.sqrSpeed < 0.02f) {
							//Checking more accurate
							Vector3 oldpos = i.carCollider.transform.position;
							//Advance
							i.carCollider.transform.position = oldpos + (i.velocity) * ttc;

							int hits = ailogic.vision.CheckPositionOccupiedByVehicle (ailogic.vehicleInfo.carBody.position + (estimatedVelocity) * ttc, proximityBuffer);
							//int hits = ailogic.vision.CheckSteerPositionOccupiedByVehicle (center, leaderBuffer);
							if (hits > proximityBuffer.Length) {
								hits = proximityBuffer.Length;
								ailogic.Log ("Leader buffer short");
							}
							for (int h = 0; h < hits; h++) {
								if (proximityBuffer [h].gameObject.GetComponentInParent<VehicleInfo> () == i) {
									ailogic.Log ("Vehicle actually will collide " + i.vehicleId, showLog);
									i.carCollider.transform.position = oldpos;
									return FluentBehaviourTree.BehaviourTreeStatus.Running;
									break;
								}


							}
							i.carCollider.transform.position = oldpos;
						} else {
							return FluentBehaviourTree.BehaviourTreeStatus.Running;
						}
					}
				} else if (inminent) {
					
					Vector3 ne = ailogic.vehicleInfo.carBody.TransformPoint (new Vector3 (0f, 0.6f, ailogic.vehicleInfo.vehicleLength*0.5f));
					//Vector3 center =  ailogic.vehicleInfo.carBody.position + 5f*Time.fixedDeltaTime*ailogic.vehicleInfo.velocity;
					//Vector3 center = CollisionPrediction.GetCollisionPoint(ailogic.vehicleInfo,i,ttc);

					int hits = ailogic.vision.CheckPositionOccupiedByVehicle (ne, proximityBuffer);
					if (hits > proximityBuffer.Length) {
						hits = proximityBuffer.Length;
						ailogic.Log ("Leader buffer short");
					}
					for (int h = 0; h < hits; h++) {


						if (proximityBuffer [h].gameObject.GetComponentInParent<VehicleInfo> () == i) {
							ailogic.Log ( i.vehicleId+"--inminent", showLog);
							return FluentBehaviourTree.BehaviourTreeStatus.Running;
						}

					}



					
				}
			}

			//foreach (Transform t in priorityCheckPositions) {
			/*for (int i = 0; i < priorityCheckPositions.Count; i++) {

				//Debug.Log ("Checking "+t.position);
				RaycastHit[] hits = ailogic.vision.CheckPositionForVehicles (priorityCheckPositions[i].position, Vector3.Distance(transform.position,priorityCheckPositions[i].position)*1.2f);
				if (hits != null) {
					//Debug.Log ("hits left:"+hits.Length);
					//Debug.Log ("There is a car waiting: "+hits[0].collider.tag + " name="+hits[0].transform.name);

					ailogic.Log("There is a car waiting. name="+hits[0].transform.name, showLog);
				
					bool cleared = CheckVehiclesWaitingForClearance (hits);
					//ailogic.Log("cleared=" + cleared);
					if (cleared==false ) {
						ailogic.Log ( "not cleared", showLog);

						return FluentBehaviourTree.BehaviourTreeStatus.Running;
					}

				}
			}*/
			//ailogic.Log ("estimatedTimeToCrossAtFullThrottle=" + estimatedTimeToCrossAtFullThrottle +". estimatedAvSpeedAtFullThrottle)="+estimatedAvSpeedAtFullThrottle);
			ailogic.Log ("Exit WaitUntilCleared with success", showLog);
			ailogic.vehicleInfo.waitingForVehicleLock.Clear ();
			initCrossing = Time.time;
			disR = ailogic.vehicleInfo.totalDistanceTraveled;
			crossingSpeed = new Average ();
			crossingSpeed.Init ();

			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}


		public bool CheckVehiclesWaitingForClearance(RaycastHit[] hits) {
			int cleared = 0;
			int waiting = 0;
			for (int i = 0; i < hits.Length; i++) {
				VehicleInfo info = hits [i].transform.GetComponent<VehicleInfo> ();
				if (info.vehicleId != ailogic.vehicleInfo.vehicleId) {
					
				
					ailogic.Log ( "hit in clearance " + info.vehicleId, showLog);
					Stop[] istops = hits [i].transform.GetComponentsInChildren<Stop> ();
					ailogic.Log ( "intersection stops " + istops.Length, showLog);
					foreach (Stop s in istops) {
						if (s.intersection == this.intersection) {
							//ailogic.Log ("intersection coincident " + this.intersection.name);

							if (info.currentActionState == VehicleInfo.VehicleActionState.WaitingForClearance) {
								//Get me the list of vehicles it is waiting for

								bool waitForMe = s.CheckIfVehiclesAtPriorityPositions (transform);
							//	ailogic.Log (" waitForMe =" + waitForMe); 

								if (waitForMe) {
									waiting++;
									if (ApplyRightBeforeLeftRule (hits [i].transform)) {
										++cleared;
									}
								}
							}
						}
					}
				}

			}
			if (waiting == cleared) {
				return true;
			} else {
				return false;
			}
		}

		public Path GetNextInternalPath() {
			int i = internalPaths.IndexOf (internalPath);
			if (i >= 0 && i<internalPaths.Count-1) {
				return internalPaths [i + 1];
			}
			return null;
		}

		protected   void HandleEnterVehicleTrigger (Collider other)
		{
			

			if (other.CompareTag ("InternalStopTrigger")) {
				//Debug.Log ("IntersectionStop: internal stop reached");
				if (internalStopPosition == other.transform) {
					internalStopReached = true;
					if (throttleGoal != null) {
						if (throttleGoal.areaTrigger == other) {
							throttleGoal.areaReached = true;

						}
					}
					//Debug.Log (internalPath);
					//Debug.Log (ailogic.routeManager);
					//Debug.Log (other.GetComponentInChildren<PathConnector> ());

					//ConnectionInfo.PathDirectionInfo n = ailogic.routeManager.GetPathIfIsInConnector (other.GetComponentInChildren<PathConnector> (), internalPath.pathId, ailogic.routeManager.FollowingPathId (internalPath.pathId));
					//if (n != null) {

					//Path nextInternalPath=ailogic.routeManager.FollowingPath(internalPath.pathId);
					Path nextInternalPath = GetNextInternalPath();
					if (nextInternalPath!=null) {
						//Debug.Log ("IntersectionStop: Connector of internal stop reached, setting new internal path ");
						internalPath = nextInternalPath;
						internalLaneEndReached = false;

						IntersectionPriorityInfo priority = nextInternalPath.GetComponent<IntersectionPriorityInfo> ();
						if (priority.HasHigherPriorityLanes ()) {
							//Debug.Log ("Getting new priority positions");
							//Actually they should be the same...Anyway, in case they have been changed..
							priorityCheckPositions = priority.GetCheckPositions ();
						}
					}
				} else if (internalStopPosition != null) {
					//Debug.Log ("myid="+ailogic.vehicleInfo.vehicleId);
					//Debug.Log (internalStopPosition);
					//Debug.Log (internalStopPosition.position);
					//Debug.Log(internalStopPosition.position+" other="+other.transform.position);

				}
			}
		}

		void OnDestroy() { 
			//Remove listeners
			throttleGoal=null;
			ailogic.vehicleTrigger.RemoveEnterListener(HandleEnterVehicleTrigger);
			mainBehaviour = null;
			tlBehaviour = null;
			//ailogic.vehicleTrigger.RemoveExitListener (HandleExitVehicleTrigger);
		}


	}

}
