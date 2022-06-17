/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris.Vehicle
{
	public class AdvancedFileLogger : MultiVarFileLogger
	{
		public VehicleInfo vi = null;

		public Transform origin;
		public Vector3 originV;
		public Quaternion originRotInv;
		public Vector3 originRotV;
		private float distance;
		private float prevSpeed = 0f;
		private float prevTime = 0f;
		private float acceleration;
		private Vector3 prevPos;

		public bool traceDistance = true;
		public bool traceSpeed = true;
		public bool traceAccel = true;
		public bool traceThrottle = true;
		public bool traceBrake = true;
		public bool traceSteeringWheel = true;
		public bool traceClutch = true;
		public bool tracePosVector = true;
		public bool traceRotVector = true;
		public bool traceVelocityVector = true;
		public bool traceAccelVector = true;
		public bool traceAngles = true;


		void Awake () {
			if (vi == null) {
				vi = GetComponent<VehicleInfo> ();
				if (vi == null)
					vi = gameObject.AddComponent<Veneris.Vehicle.VehicleInfo> ();
			}
			id = vi.vehicleId;
			Debug.Log ("finished wake vehicleinfo");

		}


		protected override  void Start ()
		{
			base.Start ();
			RecordHeaders ();
			if (origin == null){
				origin = transform;
			}
			originV = origin.position;
			originRotInv = Quaternion.Inverse (transform.rotation);
			originRotV = transform.eulerAngles;

			distance = 0f;
			prevPos = originV;
			prevSpeed = 0f;
			Debug.Log ("finished start");
		}

		public void RecordHeaders ()
		{
			AddValue ("Time");
			if (traceDistance){
				AddValue ("Dist");
			}
			if (traceSpeed){
				AddValue ("Speed");
			}
			if (traceAccel){
				AddValue ("Accel");
			}
			if (traceThrottle){
				AddValue ("Throttle");
			}
			if (traceBrake){
				AddValue ("Brake");
			}
			if (traceSteeringWheel){
				AddValue ("SteerW");
			}
//			if (traceClutch){
//				AddValue ("Clutch");
//			}

			if (tracePosVector){
				AddValue ("Pos.x\tPos.y\tPos.z");
			}
			if (traceVelocityVector){
				AddValue ("Speed.x\tSpeed.y\tSpeed.z");
			}

			if (traceAccelVector) {
				AddValue ("Accel.x\tAccel.y\tAccel.z");
			}

			
			if (traceRotVector){
				AddValue ("Rot.x\tRot.y\tRot.z");
			}

			if (traceAngles){
				AddValue ("Yaw\tPitch\tRoll\t");
			}

			RecordAdded();



		}


		void FixedUpdate ()
		{
			AddValue (Time.time);

			//Vector3 currPos = origin.InverseTransformPoint (transform.position);
			Vector3 currPos = vi.transform.position;
			if (traceDistance){
				distance += (currPos - prevPos).magnitude;
				AddValue (distance);
				//AddValue (transform.position.x);
			}
			if (traceSpeed){
				AddValue (vi.speed);
			}
			if (traceAccel){
				AddValue ((vi.speed -prevSpeed)/(Time.time - prevTime));
			}
			if (traceThrottle){
				AddValue (vi.carController.input.throttle);
			}
			if (traceBrake){
				AddValue (vi.carController.input.brake);
			}
			if (traceSteeringWheel	){
				vi.carController.steerControl.ClampSteeringWheelAngle ();
				AddValue (vi.carController.input.steeringWheelRotation);
			}
//			if (traceClutch){
//				AddValue ("Clutch");
//			}
//
			if (tracePosVector){
				Vector3 relPos = currPos - originV;
				AddValue (relPos.x);
				AddValue (relPos.y);
				AddValue (relPos.z);
			}
				

			if (traceVelocityVector){
				Vector3 velocity = vi.velocityV3;
				AddValue (velocity.x);
				AddValue (velocity.y);
				AddValue (velocity.z);
			}			

			if (traceAccelVector){
				Vector3 accelV3 = vi.accelV3;
				AddValue (accelV3.x);
				AddValue (accelV3.y);
				AddValue (accelV3.z);
			}			


			if (traceRotVector){
				Vector3 carOrientation = originRotInv * vi.transform.forward; 
				AddValue (carOrientation.x);
				AddValue (carOrientation.y);
				AddValue (carOrientation.z);
			}

			if (traceAngles){
				Vector3 signedAngles = AngleSigned (vi.eulerAngles);
				//Debug.Log ("Tracing angles: " + vi.eulerAngles);
				AddValue (signedAngles.y);
				AddValue (signedAngles.x);
				AddValue (signedAngles.z);

			}

			prevPos = currPos;
			RecordAdded ();
		}

		public static Vector3 AngleSigned(Vector3 v1)
		{
			Vector3 output = new Vector3 (v1.x, v1.y, v1.z);
			output.x = output.x < 180f ? output.x : 360f - output.x;
			output.y = output.y < 180f ? output.y : 360f - output.y;
			output.z = output.z < 180f ? output.z : 360f - output.z;

			return output;
		}

	}
}