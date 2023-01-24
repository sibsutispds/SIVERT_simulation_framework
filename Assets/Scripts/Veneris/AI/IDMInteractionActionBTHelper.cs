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

	//The IDM model only controls acceleration, so this can be used as a throttle behaviour function
	//This vehicle is only following someone if it is inside its vision box. There are other alternatives
	public class IDMInteractionActionBTHelper : ThrottleProportionalControllerActionBTHelper
	{

		//public float speedGoal =34f;//[m/s] v_o in the model or "desired speed"
		public float idmSafetyGap = 1.5f;
		// [s], T in the model, the main contribution in stationary traffic, make follow the leader with a constant vT safety time gap
		public float idmJamDistance = 2f;
		//[m], d
		public float idmA = 1f;
		public float idmB = 1f;
		public float idmAccelerationExponent = -1f;

		public VehicleVisionPerceptionModel vision = null;

		//Show it on the inspector

		public int leadingVehicleId = -1;

		public bool keepStopped=false;

	
		public Dictionary<int,VehicleInfo> frontVehiclesInLane = null;
		public Dictionary<int,VehicleInfo> frontVehiclesInSight = null;
	
		public TrackedVehicleInfo leadingVehicle = null;
		//private Transform frontBumper = null;
		//private AILogic myAI = null;
	
		public class TrackedVehicleInfo
		{
			public VehicleInfo vehicle;
			public float sqrDistance;
			public TrackedVehicleInfo(VehicleInfo v, float sqrDistance) {
				this.vehicle=v;
				this.sqrDistance=sqrDistance;
			}

		}








		public bool logDeltaPosition = false;
		public bool logIDMAcc = false;

	
		protected FileLogger deltaPosLog;
		protected FileLogger IDMAcc;

		protected AccelerationTracker actracker;
		[SerializeField]
		private float _checkFrontTime =0.1f; //Time to check for leader vehicle: Could be considered a reaction time
		public float checkFrontTime {
			get { return _checkFrontTime; }
			set {
				_checkFrontTime = value;
				waitCheckFrontTime = new WaitForSeconds (checkFrontTime);
			}
		}
		protected Coroutine checkFrontCoroutine = null;
		protected WaitForSeconds waitCheckFrontTime = null;
		protected bool checkFrontCoroutineRunning=false;

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
			frontVehiclesInLane = new Dictionary<int,VehicleInfo> ();
			frontVehiclesInSight = new Dictionary<int, VehicleInfo> ();

			vision.AddEnterListener (HandleVisionTriggerEnter);
			vision.AddExitListener (HandleVisionTriggerExit);
			if (leadingVehicle == null) {
				Debug.Log (ailogic.vehicleInfo.vehicleId + " start : leadingVehicle==null");
			} else {
				Debug.Log ("start:" + leadingVehicle);
			}
			waitCheckFrontTime = new WaitForSeconds (checkFrontTime);
			SetUpLogs ();
			if (ailogic.vehicleManager != null) {
				ailogic.vehicleManager.AddRemoveListener (HandleDestroyTrigger);
			}
		}


		protected void HandleDestroyTrigger(VehicleInfo info) {
			frontVehiclesInLane.Remove (info.vehicleId);
			frontVehiclesInSight.Remove(info.vehicleId);
			if (leadingVehicle != null) {
				if (leadingVehicle.vehicle.vehicleId == info.vehicleId) {
					leadingVehicle = null;
					leadingVehicleId = -1;
				}
			}
		
		}

		protected void SetUpLogs() {
			if (logDeltaPosition) {

				deltaPosLog = gameObject.AddComponent<FileLogger> ();
				deltaPosLog.name = "deltapos";
				deltaPosLog.id = ailogic.vehicleInfo.vehicleId;


			}
			if (logIDMAcc) {

				IDMAcc = gameObject.AddComponent<FileLogger> ();
				IDMAcc.name = "idm";
				IDMAcc.id = ailogic.vehicleInfo.vehicleId;
				actracker.lastTime = Time.time;
				actracker.lastSpeed = 0.0f;

			}
		}

		protected void SetLeadingVehicle(TrackedVehicleInfo v) {
			leadingVehicle = v;
		}

		protected void IsLeadingVehicle (VehicleInfo v)
		{
			
			if (leadingVehicle == null) {
				
				SetLeadingVehicle (new TrackedVehicleInfo (v, Vector3.SqrMagnitude (v.backBumper.position - ailogic.vehicleInfo.frontBumper.position)));


			} else {
				float d = Vector3.SqrMagnitude (v.backBumper.position - ailogic.vehicleInfo.frontBumper.position);
				if (d < leadingVehicle.sqrDistance) {
					leadingVehicle.vehicle = v;
					leadingVehicle.sqrDistance = d;
					//Debug.Log (myAI.vehicleInfo.vehicleId + " front vehicle=" + v.vehicleId);


				}
				if (leadingVehicle.vehicle.vehicleId == v.vehicleId) {
					//Update distance
					leadingVehicle.sqrDistance = d;
				}
			}
		}
		protected void HandleVisionTriggerExit(Collider other) {
			
			if (other.tag == "CarCollider") {
				VehicleInfo vi = other.gameObject.GetComponentInParent<VehicleInfo> ();
				//Debug.Log (ailogic.vehicleInfo.vehicleId + "OnTriggerExit leaves vehicle " + vi.vehicleId);
				if (vi != null) {
					if (frontVehiclesInLane.Remove (vi.vehicleId)) {
						//Debug.Log (ailogic.vehicleInfo.vehicleId+ " Removing vehicle from lane " + vi.vehicleId);
						if (leadingVehicle.vehicle.vehicleId == vi.vehicleId) {
							leadingVehicle = null;
							leadingVehicleId = -1;
						}
						//Now check who is the leader among the ones in front of me


						foreach (VehicleInfo info in frontVehiclesInLane.Values) {
							//Vehicles may have been destroyed because they have finished the route, this is handled in HandleDestroyTrigger by the manager
							//Otherwise, we need to handle it here

								IsLeadingVehicle (info);
						
						}
					} else if (frontVehiclesInSight.Remove (vi.vehicleId)) {
						//Debug.Log (ailogic.vehicleInfo.vehicleId+ " Removing vehicle from sight " + vi.vehicleId +"frontVehiclesInSight.Count="+frontVehiclesInSight.Count );
						if (leadingVehicle!=null && leadingVehicle.vehicle.vehicleId == vi.vehicleId) {
							leadingVehicle = null;
							leadingVehicleId = -1;
						}
						//disabled and destroyed vehicles do not trigger OnTriggerExit, we can chek if the values of frontVehiclesInSight are null to remove them

						if (frontVehiclesInSight.Count == 0) {
							//No more tracked vehicles
							if (checkFrontCoroutineRunning) {
								//Debug.Log ("STOP Coroutine " + ailogic.vehicleInfo.vehicleId);
								StopCoroutine (checkFrontCoroutine);
								checkFrontCoroutineRunning = false;
							}
						} 
					}
				}
			}
		}
		protected void HandleVisionTriggerEnter (Collider other)
		{
			//Debug.Log ("Vision, other collider=" + other.name);

			//Everything that enters our collider is in our field of view
			//TODO: check performance when using a direct raycast to, it is actually more general, and we do not need to check for lanes, etc...
			if (other.tag == "CarCollider") {

				//We should check if it is on our lane
				//if (Vector3.Angle (other.transform.position-transform.position, transform.forward) <= halfFrontFieldOfViewAngle) {
				//Some "visual" check may be implemented, at the moment, just use laneID
				VehicleInfo vi = other.gameObject.GetComponentInParent<VehicleInfo> ();
				if (vi == null) {
					Debug.Log ("other vehicle info null");
				} else if (vi.vehicleId == ailogic.vehicleInfo.vehicleId) {
					return;
				}
				if (CheckIfOtherVehicleInMyLane (vi)) {
					IsLeadingVehicle (vi);
					frontVehiclesInLane [vi.vehicleId] = vi;
				} else {
					//Enable boxcasting to avoid collisions
					if (!checkFrontCoroutineRunning) {
						//Debug.Log ("START COROUTINE " + ailogic.vehicleInfo.vehicleId);

						checkFrontCoroutine = StartCoroutine (CheckFront ());
						checkFrontCoroutineRunning = true;
					}

					frontVehiclesInSight [vi.vehicleId] = vi;
				}

			}
		}

		protected bool CheckIfOtherVehicleInMyLane(VehicleInfo i) {
			if (i.laneId == ailogic.currentLane.laneId && i.roadId == ailogic.currentRoad.roadId && i.roadEdgeId == ailogic.currentRoad.edgeId) {
				return true;
			} else {
				return false;
			}
		}

		protected float ApplyIDM ()
		{
			float speed = ailogic.vehicleInfo.speed;
			float dec = 0.0f;
			if (leadingVehicle != null) {
				//Do this just to show it in the inspector
				leadingVehicleId = leadingVehicle.vehicle.vehicleId;
				//Braking decceleration

				float deltaSpeed = speed - leadingVehicle.vehicle.speed;
				float s_star = idmJamDistance + speed * idmSafetyGap + (speed * deltaSpeed) / (2 * Mathf.Sqrt (idmA * idmB));
				s_star = s_star * s_star;
				dec = -idmA * (s_star / leadingVehicle.sqrDistance);
			}

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





			if (logIDMAcc) {
				IDMAcc.RecordWithTimestamp<string> (freeA + "\t" + dec);
			}

			/*
			if (logResults) {
				Debug.Log (ailogic.vehicleInfo.vehicleId + ": throttle=" + ailogic.throttle);

				//SHOW HERE REAL ACCELERATUION

				float acc = (speed - lastSpeed) / Time.deltaTime;
				lastSpeed = speed;
				Debug.Log (ailogic.vehicleInfo.vehicleId + "acceleration=" + acc);
				accLog.RecordWithTimestamp (acc);
				Debug.Log (ailogic.vehicleInfo.vehicleId + ": speed=" + ailogic.vehicleInfo.speed);
				Debug.Log (ailogic.vehicleInfo.vehicleId + ": freeA=" + freeA);
				speedLog.RecordWithTimestamp (speed);
				posLog.RecordWithTimestamp (transform.position.z);
				if (vision.frontVehicle != null) {
					Debug.Log (ailogic.vehicleInfo.vehicleId + ": dec=" + dec);
					Debug.Log (ailogic.vehicleInfo.vehicleId + ": s=" + vision.frontVehicle.sqrDistance);
					deltaPosLog.RecordWithTimestamp (Mathf.Sqrt (vision.frontVehicle.sqrDistance));
					Debug.Log (ailogic.vehicleInfo.vehicleId + ": vT=" + (safetyGap * speed));
					Debug.Log (ailogic.vehicleInfo.vehicleId + ": se=" + ((safetyGap * speed + jamDistance) * (safetyGap * speed + jamDistance) / (1f - Mathf.Pow (speed / speedGoal, accelerationExponent))));
				}
			}
			*/

			if (leadingVehicle != null && logDeltaPosition) {
				deltaPosLog.RecordWithTimestamp<float> (Mathf.Sqrt (leadingVehicle.sqrDistance));
			}
			//Debug.Log (Time.time + "  " + ailogic.vehicleInfo.vehicleId + ": speed=" + ailogic.vehicleInfo.speed);
			//Debug.Log (Time.time + "  " + ailogic.vehicleInfo.vehicleId + ": freeA=" + freeA);
			//Debug.Log (Time.time+"freeA" + freeA);
			//Debug.Log (Time.time+"dec" + dec);
			return Mathf.Clamp (freeA + dec, -1.0f, 1.0f);
		}

		protected override float StopAtPoint()
		{
			float t = 0f;
			if (goalForPoint.stoppedAtPoint) {
				return -1f;;
			}
			//Smoothly reduce distance until stop
			float prjDistanceToStop=0f;
			//freeSpeed =  GetLinearSpeedForSpeedAtPoint (ref prjDistanceToStop);

			freeSpeed =  GetSpeedForSpeedAtPoint (ref prjDistanceToStop);
			if (goalForPoint.useAreaTrigger) {
				if (goalForPoint.areaReached && ailogic.vehicleInfo.speed > 0) {
					//Dont want to go reverse
					ailogic.Log ("stopped");
					goalForPoint.stoppedAtPoint = true;
					t = -1f;
				}else {
					t = ApplyIDM ();
				}
			} else { 
				if ((prjDistanceToStop <= goalForPoint.distanceMargin) && ailogic.vehicleInfo.speed > 0) {
					//Force break and set stop
					//Dont want to go reverse
					ailogic.Log ("stopped at " + prjDistanceToStop);
					goalForPoint.stoppedAtPoint = true;
					t = -1f;
				} else {
					t = ApplyIDM ();
				}
			}

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
			//freeSpeed = GetLinearSpeedForSpeedAtPoint ();

			freeSpeed = GetSpeedForSpeedAtPoint ();

			t = ApplyIDM ();
			return t;
		}
		protected override float AdaptToCurvature ()
		{
			float t = 0f;

			//float maxCorneringSpeed = GetMaxCorneringSpeedAtPoint (ref speedLookAheadPoint);


			/*if (maxCorneringSpeed <= freeSpeed) {
				freeSpeed = maxCorneringSpeed;
			} */


			//Proportional controller with Kp_speed=1
			t= ApplyIDM ();
			//Debug.Log ("lookAhead=" + lookAhead);
			//			Debug.Log ("goal.curvature=" + speedLookAheadPoint.curvature + "maxCorneringSpeed=" + maxCorneringSpeed + " freeSpeed=" + freeSpeed + " throttle=" + t + " deltav=" + (freeSpeed - ailogic.vehicleInfo.speed));
			return t;

		}
		public void SetViewDistanceToSpeed() {
			//So-called "square rule"...set distance to (s/10)^2 m, with s in Km/h increased in 50%
			//Debug.Log(Time.time.ToString("F5")+"---v"+ailogic.vehicleInfo.vehicleId+":"+ailogic.vehicleInfo.sqrSpeed+" "+ailogic.vehicleInfo.speed+" "+ailogic.vehicleInfo.sqrSpeed*0.25f);
			vision.SetViewDistance (ailogic.vehicleInfo.sqrSpeed*0.25f);
		}
		public FluentBehaviourTree.BehaviourTreeStatus IDMThrottleControl ()
		{
			
			float ver = 0f;
			//Keep track of  distances to other vehicles


			foreach (VehicleInfo vi in frontVehiclesInLane.Values) {
				//TODO:Vehicles may have been destroyed because they have finished the route, igonore them at the moment
				if (vi != null) {
					IsLeadingVehicle (vi);
				}
			}


			if (throttleMode == ThrottleMode.StopAtPoint) {
				ver = StopAtPoint ();

			}  else if (throttleMode == ThrottleMode.AdaptToCurvature) {
				ver =AdaptToCurvature ();

			}	else if (throttleMode == ThrottleMode.SpeedAtPoint) {
				ver = SpeedAtPoint();
				//ver = SpeedAtPoint ();

			} else {
				if (leadingVehicle == null) {


					if (throttleMode != ThrottleMode.ApplyBehaviour) {
						Debug.Log ("Proprotional Throttle");
						ailogic.throttle = ProportionalThrottleControl ();
						return FluentBehaviourTree.BehaviourTreeStatus.Success;
					}
				}
				ver = ApplyIDM ();
			}
			//ver = 0.5f;
			if ( keepStopped) {
				ailogic.throttle = 0f;
				ailogic.brake = 0.5f;
				//Debug.Log (Time.time + "throttle=" + ailogic.throttle);
				//Debug.Log (Time.time + "brake=" + ailogic.brake);
			} else {
				SetViewDistanceToSpeed ();
				ailogic.throttle = ver * (ver > 0 ? 1 : 0);
				ailogic.brake = ver * (ver < 0 ? -1 : 0);
			}
			//Debug.Log (ailogic.vehicleInfo.vehicleId+"--"+ Time.time + "--throttle=" + ailogic.throttle);
			//Debug.Log (ailogic.vehicleInfo.vehicleId+"--"+ Time.time + "--brake=" + ailogic.brake);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

		IEnumerator CheckFront() {
			while (true) {
				RaycastHit[] hits=ailogic.vision.CheckFrontForEntitiesWithTag ("CarCollider");
				if (hits!=null) {
					//Debug.Log ("hits left:"+hits.Length);

					foreach (RaycastHit h in hits) {
						//Debug.Log (ailogic.vehicleInfo.vehicleId+ " -- There is a car in front: "+h.collider.tag + " name="+h.transform.name);
						VehicleInfo vi = h.collider.gameObject.GetComponentInParent<VehicleInfo> ();

					    if (vi.vehicleId == ailogic.vehicleInfo.vehicleId) {
							continue;
						}

						IsLeadingVehicle (vi);



					}

				}
				yield return waitCheckFrontTime;
			}
		}

		void OnDestroy() {
			if (ailogic != null) {
				ailogic.vehicleManager.RemoveRemoveListener (HandleDestroyTrigger);
			}
		}
	}
}
