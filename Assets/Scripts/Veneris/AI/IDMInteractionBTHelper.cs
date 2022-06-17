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
	public class IDMInteractionBTHelper : ThrottleProportionalControllerActionBTHelper
	{
		/*[System.Serializable]
		public class TrackedVehicleInfo
		{
			public VehicleInfo vehicle;
			public bool activeLeadingVehicle = false;
			public TrackedVehicleInfo (VehicleInfo v, float sqrDistance)
			{
				this.vehicle = v;

				activeLeadingVehicle =false;
			}
		}*/
		//public float speedGoal =34f;//[m/s] v_o in the model or "desired speed"
		public float idmSafetyGap = 0.1f;
		// [s], T in the model, the main contribution in stationary traffic, make follow the leader with a constant vT safety time gap
		public float idmJamDistance = 0.5f;
		//[m], d
		public float idmA = 7f;
		public float idmB = 8f;
		public float idmAccelerationExponent = 8f;
		public float leaderFarAway=3f;
		public bool showLog = false;

		/*public bool useCoroutineFrontCheck = true;
		public float lastCheckFrontTime = 0f;


		[SerializeField]
		private float _checkFrontTime =0.1f; //Time to check for leader vehicle: Could be considered a reaction time
		public float checkFrontTime {
			get { return _checkFrontTime; }
			set {
				_checkFrontTime = value;
				waitCheckFrontTime = new WaitForSeconds (_checkFrontTime);
			}
		}

		[SerializeField]
		private float _checkLockTime =6f; 
		public float checkLockTime {
			get { return _checkLockTime; }
			set {
				_checkLockTime = value;
				waitCheckLockTime = new WaitForSeconds (_checkLockTime);
			}
		}

		//Time to wait until two vehicles are considered locked
		public int maxAttempsBeforeTeleporting = 3;
		public int maxTeleportAttempsBeforeRemoving= 3;
		public int numberOfUnlockAttemps=0;
		public int numberOfTeleportAttemps = 0;



		protected Coroutine checkFrontCoroutine = null;
		protected Coroutine checkLockCoroutine = null;
		protected WaitForSeconds waitCheckFrontTime = null;
		protected WaitForSeconds waitCheckLockTime = null;
		protected bool checkFrontCoroutineRunning = false;
		protected bool checkLockCoroutineRunning = false;

*/	

		public VehicleVisionPerceptionModel vision = null;



		protected LeadingVehicleSelector leadingVehicleSelector = null;


		//For debug, mainly. Remove at release
		public float computedFreeA;
		public float computedDec;
		public float computedAcc;
		public float computedSqrd;
		public float computedVectoDeltaSpeed;
		public float computedSstar;

		// Use this for initialization
		void Start ()
		{
			Init ();
		
		}

		protected override void Init ()
		{
			base.Init ();

			//frontVehicle = vision.frontVehicle;
			if (vision == null) {
				vision = ailogic.vision;
			}




			/*if (ailogic.vehicleManager != null) {
				ailogic.vehicleManager.AddRemoveListener (HandleDestroyTrigger);
			}*/

			//leadingVehicleSelector = new LeadingVehicleSelector (ailogic, idmSafetyGap);

			//leadingVehicle = new TrackedVehicleInfo (null, 0f);
			//leadingVehicle.activeLeadingVehicle = false;
			/*waitCheckFrontTime = new WaitForSeconds (_checkFrontTime);
			//Add a small random time to the waiCheckLockTime
			waitCheckLockTime = new WaitForSeconds (_checkLockTime+System.Convert.ToSingle(SimulationManager.Instance.GetRNG().NextDouble()));
			if (useCoroutineFrontCheck) {
				checkFrontCoroutine = StartCoroutine (CheckFrontPeriodically ());
			}

			lastCheckFrontTime =Time.time;
			*/

		}


		/*protected void HandleEnterCollisionTrigger(Collider other) {
			if (transform.root.InverseTransformPoint(other.transform.root.position).z>=0)  {
				//It is in front of me, track it
				trajectoryIntersectors.Add(other);
			}
		}
		protected void HandleExitCollisionTrigger(Collider other) {
			trajectoryIntersectors.RemoveAll (other.Equals);
		}
*/

		/*protected void HandleDestroyTrigger (int vid)
		{


			if (leadingVehicleSelector.leader != null) {
				if (leadingVehicleSelector.leader.vehicleId == vid) {
					UnsetLeadingVehicle ();
				}
			}

		}*/



		public void SetLeadingVehicleSelector(LeadingVehicleSelector selector) {
			leadingVehicleSelector = selector;
		}

		/*protected void SetLeadingVehicle (VehicleInfo i)
		{
			
			//leadingVehicle.activeLeadingVehicle = true;
			//leadingVehicle.vehicle = i;

			ailogic.vehicleInfo.leadingVehicle = i;
		}
		protected void UnsetLeadingVehicle ()
		{
			//leadingVehicle.activeLeadingVehicle = false;
			//leadingVehicle.vehicle = null;
			leadingVehicleSelector.UnsetLeadingVehicle();
			ailogic.vehicleInfo.leadingVehicle = null;
		}

		protected void CheckFrontPeriodicallyNoCoroutine() {
			UnsetLeadingVehicle();
			VehicleInfo i = leadingVehicleSelector.DecideLeadingVehicle ();
			if (i!=null) {
				SetLeadingVehicle (i);
			}
		}

		IEnumerator CheckFrontPeriodically ()
		{
			while (true) {
				//CheckFront ();
				//CheckFrontAndDecideActiveLeadingVehicle();
				//CheckFrontAndDecideActiveLeadingVehicleWithCollisionCheck();
				//CheckFrontAndDecideActiveLeadingVehicleWithTrajectoryIntersection();
				UnsetLeadingVehicle();
				VehicleInfo i = leadingVehicleSelector.DecideLeadingVehicle ();
				if (i!=null) {
					SetLeadingVehicle (i);
				}
				//CheckSphereAndDecideActiveLeadingVehicleWithTrajectoryIntersection();
				yield return waitCheckFrontTime;
			}
		}

		IEnumerator CheckLockTimer ()
		{
			
			checkLockCoroutineRunning = true;
			yield return waitCheckLockTime;
			if (ResolveLock ()) {
				numberOfUnlockAttemps = 0;
			} else {
				numberOfUnlockAttemps++;
				if (numberOfTeleportAttemps>=maxTeleportAttempsBeforeRemoving) {
					SimulationManager.Instance.RecordVariableWithTimestamp (ailogic.vehicleInfo.vehicleId + " removed because numberOfTeleportAttemps >= maxTeleportAttempsBeforeRemoving",ailogic.routeManager.lookAtPath.pathId);
					ailogic.RemoveVehicleFromSimulation();
					
				}
			}
			StopCoroutine (checkLockCoroutine);
			checkLockCoroutineRunning = false;

		}


		protected bool ResolveLock() {
			
			//Check again to know if it has been resolved
			if (CheckLock ()) {
				//Speed up process in intersections
				if (ailogic.vehicleInfo.currentActionState == VehicleInfo.VehicleActionState.CrossingIntersection || ailogic.currentIntersection!=null) {
					if (numberOfUnlockAttemps >= 1) {
						return Teleport ();
					}
				}
				//First, try to advance if no one is in front

				if (!ResolveLockTryToAdvanceFront ()) {
					//Try right
					if (!ResolveLockTryToAdvanceAngle (5f, 30f)) {
						//Try left
						if (!ResolveLockTryToAdvanceAngle (5f, -30f)) {
							//Teleport
							if (numberOfUnlockAttemps >= maxAttempsBeforeTeleporting) {
								return Teleport ();
							} else {
								return false;
							}
							
						}
					}
				}
				return true;

			} else {
				return true;
			}

		}

		protected bool ResolveLockTryToAdvanceFront() {
			RaycastHit hit;
			//if (vision.CheckPositionForVehicle(out hit, vision.steerLookAtPoint)) {
			if (vision.CheckFrontForVehicle (out hit, 5f)) {
				//ailogic.Log ("Resolving lock: vehicle in front =" + hit.transform.root.name);
				return false;
			} else {
				//Direct translation
				//ailogic.Log ("Resolving lock: translate forward "+ailogic.vehicleInfo.carBody.position);
				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, vision.steerLookAtPoint,Color.yellow);
				ailogic.vehicleInfo.carBody.Translate(Vector3.forward*2f);//Just two meters
				//Try to recover path

			
				ailogic.GetPath ();

				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, vision.steerLookAtPoint, Color.green);
				//ailogic.vehicleInfo.carBody.position=vision.steerLookAtPoint;
				return true;
			}
		}
		protected bool ResolveLockTryToAdvanceAngle(float distance, float angle) {
			RaycastHit hit;
			Vector3 vlocal = new Vector3(distance*Mathf.Sin(Mathf.Deg2Rad*angle),0.0f,distance*Mathf.Cos(Mathf.Deg2Rad*angle));
			Vector3 vworld = ailogic.vehicleInfo.carBody.TransformPoint (vlocal);
			if (vision.CheckPositionForVehicle (out hit, vworld,5f)) {
				//ailogic.Log ("Resolving lock angle="+angle+" vehicle in front =" + hit.transform.root.name);
				return false;
			} else {
				//Direct translation
				//ailogic.Log ("Resolving lock: translate angle with vector :"+vlocal + "position="+ailogic.vehicleInfo.carBody.position);

				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, vision.steerLookAtPoint,Color.yellow);
				//First rotate, then translate
				ailogic.vehicleInfo.carBody.LookAt(vworld);
				ailogic.vehicleInfo.carBody.Translate(Vector3.forward*2f);//Just two meters
				ailogic.GetPath ();

				Debug.DrawLine (ailogic.vehicleInfo.carBody.position, vision.steerLookAtPoint,Color.green);
				return true;
			}
		}
		public bool Teleport() {

			Path next = ailogic.routeManager.FollowingPath (ailogic.routeManager.lookAtPath.pathId);
			if (next == null) {
				if (ailogic.routeManager.lookAtPath == ailogic.routeManager.GetLastPath ()) {
					//We are at the end of the route, just end this
					ailogic.RemoveVehicleFromSimulation();
					return true;
				}
			}
			if (vision.CheckVehicleFitInPosition (next.interpolatedPath [0].position)) {
				//Try again later
				ailogic.Log("Teleport position occupied");
				numberOfTeleportAttemps++;
				return false;
			} else {
				numberOfUnlockAttemps = 0;
				numberOfTeleportAttemps = 0;
				//ailogic.Log ("Teleporting to start of path" + next.pathId);
				Vector3 newpos = ailogic.vehicleInfo.carBody.position;
				newpos.x = next.interpolatedPath [0].position.x;
				newpos.z = next.interpolatedPath [0].position.z;

				ailogic.vehicleInfo.carBody.position = newpos;
				ailogic.vehicleInfo.carBody.rotation.SetLookRotation(next.interpolatedPath [0].tangent);
				ailogic.routeManager.SetLookAtPath (next);
				ailogic.GetPath ();
				SimulationManager.Instance.RecordVariableWithTimestamp (ailogic.vehicleInfo.vehicleId + ".teleported", next.pathId);
				return true;
			}

		}
		*/
		protected bool CheckLock() {
			/*if (leadingVehicle.activeLeadingVehicle && leadingVehicle.vehicle.leadingVehicle == ailogic.vehicleInfo) {
				//Both vehicles are tracking each other
				//if (leadingVehicle.vehicle.currentActionState != VehicleInfo.VehicleActionState.WaitingForClearance) {
					if (leadingVehicle.vehicle.sqrSpeed <= 0.25f && ailogic.vehicleInfo.sqrSpeed <= 0.25) {
						//ailogic.Log ("Possible lock between " + leadingVehicle.vehicle.vehicleId + " and me");
						return true;

					}
				//}
			}*/


			if (leadingVehicleSelector.leaderInfo.leader != null && leadingVehicleSelector.leaderInfo.leader.leadingVehicle == ailogic.vehicleInfo) {
				if (leadingVehicleSelector.leaderInfo.leader.sqrSpeed <= 0.25f && ailogic.vehicleInfo.sqrSpeed <= 0.25) {
					return true;
				}
			}
			return false;

		}


	

	/*	void OnDestroy ()
		{
			if (ailogic != null) {
				if (ailogic.vehicleManager != null) {
					ailogic.vehicleManager.RemoveRemoveListener (HandleDestroyTrigger);
				}
			}
		}
	*/

		public  float ComputeIDMBrakingDeceleration( VehicleInfo leader) {
			//Compute IDM braking deceleration for a given leader
			float speed = ailogic.vehicleInfo.speed;


			Vector3 cpoint = leader.carCollider.ClosestPoint (ailogic.vehicleInfo.frontBumper.position);
			float sqrd = Vector3.SqrMagnitude (ailogic.vehicleInfo.frontBumper.position- cpoint);

			Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, cpoint, Color.black);

			//TODO:We use the magnitude of the other velocity in the direction of our forward vector. Check other possibilities
			float vectordeltaspeed = speed-Vector3.Dot (transform.forward, leader.velocity);

			//Braking decceleration

			//TODO: adapt s_star to angle....

			float s_star = idmJamDistance + (speed * idmSafetyGap) + ((speed * vectordeltaspeed) / (2 * Mathf.Sqrt (idmA * idmB)));
			//float s_star = idmJamDistance + speed * idmSafetyGap + (speed * deltaSpeed) / (2 * Mathf.Sqrt (idmA * idmB));
			s_star = s_star * s_star;

			computedSqrd = sqrd;
			computedVectoDeltaSpeed = vectordeltaspeed;
			computedSstar = s_star;
			return (-idmA * (s_star / sqrd));

			
		}

		public  float ComputeIDMBrakingDeceleration() {

			//Compute IDM braking deceleration for my leader
			float speed = ailogic.vehicleInfo.speed;
			float sqrd = 0f;
			//float srqdL = leadingVehicleSelector.leaderDistance * leadingVehicleSelector.leaderDistance;
			if (leadingVehicleSelector.leaderInfo.distance > leaderFarAway) {
				/*float angle = Vector3.Angle (ailogic.vehicleInfo.carBody.forward,  leadingVehicleSelector.leader.carBody.forward);

				//float prosqrd = 0f;
				//float bbangle = 0;
				if (angle <= 90) {
					//It is heading forward with respect to me (it is moving away)
					//Depends on the orientation
					//ailogic.Log("forward");
					sqrd = Vector3.SqrMagnitude ( leadingVehicleSelector.leader.backBumper.position - ailogic.vehicleInfo.frontBumper.position);

				} else {
					//It is heading me, approaching

					//Even if it is approaching me, IDM will keep us at idmJamDistance from each other
					sqrd = Vector3.SqrMagnitude ( leadingVehicleSelector.leader.frontBumper.position - ailogic.vehicleInfo.frontBumper.position);

				}

				ailogic.Log ("sqrd=" + sqrd + "distance=" + srqdL);
				*/
				sqrd= leadingVehicleSelector.leaderInfo.distance * leadingVehicleSelector.leaderInfo.distance;
			
					ailogic.Log ("sqrd=" + sqrd,showLog);

			} else {
				Vector3 cpoint = leadingVehicleSelector.leaderInfo.leader.carCollider.ClosestPoint (ailogic.vehicleInfo.frontBumper.position);
				sqrd = Vector3.SqrMagnitude (ailogic.vehicleInfo.frontBumper.position- cpoint);
				Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, cpoint, Color.black);

					ailogic.Log ("close leader"+leadingVehicleSelector.leaderInfo.leader+" dist="+Mathf.Sqrt(sqrd),showLog);
			
			}




			//TODO:We use the magnitude of the other velocity in the direction of our forward vector. Check other possibilities
			float vectordeltaspeed = speed-Vector3.Dot (transform.forward,  leadingVehicleSelector.leaderInfo.leader.velocity);

			//Braking decceleration

			//TODO: adapt s_star to angle....

			float s_star = idmJamDistance + (speed * idmSafetyGap) + ((speed * vectordeltaspeed) / (2 * Mathf.Sqrt (idmA * idmB)));
			//float s_star = idmJamDistance + speed * idmSafetyGap + (speed * deltaSpeed) / (2 * Mathf.Sqrt (idmA * idmB));
			s_star = s_star * s_star;
		
			computedSqrd = sqrd;
			computedVectoDeltaSpeed = vectordeltaspeed;
			computedSstar = s_star;

			return (-idmA * (s_star / sqrd));


		}


		public float ComputeIDMFreeAcceleration() {
			float speed = ailogic.vehicleInfo.speed;

			//Free acceleration a[1-(v(vo)^delta] or -b[1-(vo/v)^delta]
			float freeA = 0.0f;

			if (freeSpeed > 0f) { 
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

			}

			return freeA;
			
		}
		protected float ApplyIDM ()
		{
			float speed = ailogic.vehicleInfo.speed;

			float dec = 0.0f;
			//float decp = 0.0f;


		

			if (leadingVehicleSelector.leaderInfo.leader!=null) {
				/*if (CheckLock ()) {


					if (checkLockCoroutineRunning == false) {
						

						checkLockCoroutine = StartCoroutine (CheckLockTimer ());
					
					}
				}*/
				dec = ComputeIDMBrakingDeceleration ();

			}

			float freeA = ComputeIDMFreeAcceleration ();
		

			//To show it on inspector
			computedAcc=freeA + dec;
			computedDec = dec;
			computedFreeA = freeA;

				ailogic.Log ("acc=" + (freeA + dec)+"freeA="+freeA+"dec="+dec +"speed="+ailogic.vehicleInfo.speed, showLog);

			return Mathf.Clamp (freeA + dec, -1.0f, 1.0f);
		}

		/*protected void DecideCloseVehicleAction() {
			if (leadingVehicleSelector.insideList.Count == 1) {
				VehicleInfo i = leadingVehicleSelector.insideList [0];
				if (leadingVehicleSelector.IsCloseVehicleInFront (i)) {
					
					reason = LeaderReason.InsideAndFront;
					leader = i;
					leaderDistance = ttcInfo.distance;
					SetLeader (i, ttcInfo.distance, LeaderReason.InsideAndFront);
					ailogic.Log ("is leader" + i.vehicleId);
					//ailogic.Log ( "is leader" + i.vehicleId);
				} else {
					//If it is changing lane, let it pass
					if (i.currentActionState == VehicleInfo.VehicleActionState.ChangingLane || i.currentActionState == VehicleInfo.VehicleActionState.WantToChangeLane) {
						if (i.targetLaneChange == ailogic.currentLane) {
							SetLeader (i, ttcInfo.distance, LeaderReason.InsideAndChangingLane);
							ailogic.Log ("changing lane leader" + i.vehicleId);

						}
					} else {
						if (leader == null) {
							reason = LeaderReason.InsidePassedMe;
						}
					}
				}
			}
		}*/




		protected override float StopAtPoint ()
		{
			float t = 0f;
			/*if (goalForPoint.stoppedAtPoint) {
				return -1f;

			}*/


			//freeSpeed = GetSpeedForSpeedAtPoint (ref prjDistanceToStop);
			 //GetSpeedForSpeedAtPoint (ref prjDistanceToStop);
			if (goalForPoint.useAreaTrigger) {
				//if (goalForPoint.areaReached && ailogic.vehicleInfo.speed > 0) {
				if (goalForPoint.areaReached) {
					goalForPoint.stoppedAtPoint = true;
					t = -1f;
				} else {
					t = ApplyIDM ();
				}
			} 


			/*else { 
				//Smoothly reduce distance until stop
				float prjDistanceToStop = 0f;

				if ((prjDistanceToStop <= goalForPoint.distanceMargin) && ailogic.vehicleInfo.speed > 0) {
					//Force break and set stop
					//Dont want to go reverse
					//ailogic.Log ("stopped at " + prjDistanceToStop);
					goalForPoint.stoppedAtPoint = true;
					t = -1f;
				} else {
					t = ApplyIDM ();
				}
			}*/

			/* Alternative
			float prjDistanceToStop = Mathf.Abs (Vector3.Dot (ailogic.vehicleInfo.frontBumper.position - stopAtPointInfo.stopPoint.position, ailogic.vehicleInfo.frontBumper.forward));
			float speed = ailogic.vehicleInfo.speed;
			float dec = (at.lastSpeed - speed) / (Time.time - at.lastTime);
			at.lastSpeed = speed;
			at.lastTime = Time.time;
			if ((prjDistanceToStop <= stopAtPointInfo.distanceMargin) && speed > 0) {
					Debug.Log ("prjDistanceToStop="+ prjDistanceToStop);
					Debug.Log ("pr=" + Vector3.Dot (stopAtPointInfo.stopPoint.position-ailogic.vehicleInfo.frontBumper.position, ailogic.vehicleInfo.frontBumper.forward));
					Debug.Log ("ailogic.vehicleInfo.speed="+ ailogic.vehicleInfo.speed);
					Debug.Log (dec);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position, stopAtPointInfo.stopPoint.position, Color.black);
					Debug.DrawLine (ailogic.vehicleInfo.frontBumper.position,ailogic.vehicleInfo.frontBumper.position+ prjDistanceToStop*ailogic.vehicleInfo.frontBumper.forward, Color.cyan);


		
					//Dont want to go reverse
					Debug.Log ("stopped");
					stopAtPointInfo.stoppedAtStopPoint = true;
					t = -1f;
				
			} else {

				float ebd=GetEstimatedBrakingDistance (dec);
				float dif = prjDistanceToStop - ebd;
				if (dif < 0) {
					t = Mathf.Clamp (prjDistanceToStop -ebd, -1, 1);
				} else {
					t = ApplyIDM ();
				}

			}
			*/

			return t;
		}



		protected override float SpeedAtPoint ()
		{
			float t = 0f;

			//freeSpeed = GetSpeedForSpeedAtPoint ();

			t = ApplyIDM ();
			return t;
		}

		protected override float AdaptToCurvature ()
		{
			float t = 0f;
			t = ApplyIDM ();
			return t;

		}

		public void SetViewDistanceToSpeed ()
		{
			//So-called "square rule"...set distance to (s/10)^2 m, with s in Km/h increased in 50%
			//Debug.Log(Time.time.ToString("F5")+"---v"+ailogic.vehicleInfo.vehicleId+":"+ailogic.vehicleInfo.sqrSpeed+" "+ailogic.vehicleInfo.speed+" "+ailogic.vehicleInfo.sqrSpeed*0.25f);
			vision.SetViewDistance (ailogic.vehicleInfo.sqrSpeed * 0.25f);
		}

		public FluentBehaviourTree.BehaviourTreeStatus IDMThrottleControl ()
		{

			float ver = 0f;
			//Keep track of  distances to other vehicles


	
		
			//Keep deciding the leading vehicle
			/*if (useCoroutineFrontCheck == false) {
				if ((Time.time - lastCheckFrontTime) > checkFrontTime) {
					CheckFrontPeriodicallyNoCoroutine ();
					lastCheckFrontTime = Time.time;
					//ailogic.DisplaceSteerTo (avoidanceVector);
				}
			}*/
			

				//ailogic.Log ("Throthle Mode=" + throttleMode, showLog);
					if (throttleMode == ThrottleMode.StopAtPoint) {
						ver = StopAtPoint ();

					} else if (throttleMode == ThrottleMode.AdaptToCurvature) {
						ver = AdaptToCurvature ();

					} else if (throttleMode == ThrottleMode.SpeedAtPoint) {
						ver = SpeedAtPoint ();
						//ver = SpeedAtPoint ();
					} else if (throttleMode == ThrottleMode.Accelerate) {
						ver = 1f;
						//ver = SpeedAtPoint ();

					} else if (throttleMode == ThrottleMode.Brake) {
						ver = -1f;
						//ver = SpeedAtPoint ();
				 
					} else {
				
						ver = ApplyIDM ();
					}
					//ver = 0.5f;

					SetViewDistanceToSpeed ();
					///if (float.IsNaN (ver)) {
					//	ailogic.Log("freeSpeed"+freeSpeed.ToString("f6"));
					//	ailogic.Log("ver is NAN"+ver.ToString("f6"));
					//	throw new UnityException ();
					//}
					ailogic.throttle = ver * (ver > 0f ? 1f : 0f);
					ailogic.brake = ver * (ver < 0f ? -1f : 0f);

					//ailogic.Log(ver.ToString("f6")+"throttle="+ailogic.throttle.ToString("f6")+"brake="+ailogic.brake.ToString("f6"));
					//ailogic.throttle = Mathf.Clamp01(ailogic.throttle);
					//ailogic.brake = Mathf.Clamp01(ailogic.brake);
					//ailogic.Log(ver.ToString("f6")+"2throttle="+ailogic.throttle.ToString("F6")+"brake="+ailogic.brake.ToString("F6"));
					/*if (ailogic.vehicleInfo.vehicleId == 2) {
				ailogic.throttle = 0f;
				ailogic.brake =1f;
			}*/
				
			
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		//Compute but does not apply
		public float ComputeCurrentThrottle() {
			float ver = 0f;
			//Keep track of  distances to other vehicles




			//Keep deciding the leading vehicle
			/*if (useCoroutineFrontCheck == false) {
				if ((Time.time - lastCheckFrontTime) > checkFrontTime) {
					CheckFrontPeriodicallyNoCoroutine ();
					lastCheckFrontTime = Time.time;
					//ailogic.DisplaceSteerTo (avoidanceVector);
				}
			}*/


			//ailogic.Log ("Throthle Mode=" + throttleMode, showLog);
			if (throttleMode == ThrottleMode.StopAtPoint) {
				ver = StopAtPoint ();

			} else if (throttleMode == ThrottleMode.AdaptToCurvature) {
				ver = AdaptToCurvature ();

			} else if (throttleMode == ThrottleMode.SpeedAtPoint) {
				ver = SpeedAtPoint ();
				//ver = SpeedAtPoint ();
			} else if (throttleMode == ThrottleMode.Accelerate) {
				ver = 1f;
				//ver = SpeedAtPoint ();

			} else if (throttleMode == ThrottleMode.Brake) {
				ver = -1f;
				//ver = SpeedAtPoint ();

			} else {

				ver = ApplyIDM ();
			}
			return ver;
		}

	

		public FluentBehaviourTree.BehaviourTreeStatus ApplyThrottle (float t)		{
			SetViewDistanceToSpeed ();
			ailogic.throttle = t * (t > 0f ? 1f : 0f);
			ailogic.brake = t * (t < 0f ? -1f : 0f);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
	}

}
