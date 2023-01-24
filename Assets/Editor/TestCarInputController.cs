using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris.Vehicle
{
	public class TestCarInputController : BaseCarInputController
	{
		public AnimationCurve throttleCurve = null;
		public AnimationCurve steeringCurve = null;
		public AnimationCurve brakeCurve = null;
		public AnimationCurve speedCurve = null;

		public enum TestTypes {ACCEL_DECEL, CURVE};
		public TestTypes type = TestTypes.CURVE;


		[HideInInspector]
		public float startTime;
		public float testDuration = 0f;

		PID pid;
		VehicleInfo info;

		// Curve test only
		// PID coefficients
		public float Kp = 1f;
		public float Ki = 3.5f;
		public float Kd = 0f;
		public float cruiseSpeed = 40f; // km/h
		public float steerAngle = 100f; // steeringWheel rotation (at end)
		public float steerRate = 300f; // sterringWheel rotation speed


		// Use this for initialization
		void Start ()
		{
			startTime = Time.time;
			info = GetComponent<Veneris.Vehicle.VehicleInfo> ();
			// Acceleration & deceleration test
			if (type == TestTypes.ACCEL_DECEL) {
				if (testDuration == 0)
					testDuration = 46.2f;
				requestNeutralGear = true;
				if (throttleCurve == null || throttleCurve.length == 0) {
					throttleCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (1f, 1f), new Keyframe (1.1f, 1f), new Keyframe (2f, 1.0f), new Keyframe (testDuration - 10.2f, 1f), new Keyframe (testDuration- 10.1f, 0f), new Keyframe (testDuration, 0f)); 
					for (int i = 0; i < throttleCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (throttleCurve, i, AnimationUtility.TangentMode.Linear);
						AnimationUtility.SetKeyRightTangentMode (throttleCurve, i, AnimationUtility.TangentMode.Linear);
					}
				}


				if (steeringCurve == null || steeringCurve.length == 0) {
					steeringCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (testDuration, 0.0f)); 
					for (int i = 0; i < steeringCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (steeringCurve, i, AnimationUtility.TangentMode.Linear);
						AnimationUtility.SetKeyRightTangentMode (steeringCurve, i, AnimationUtility.TangentMode.Linear);
					}
				}

				if (brakeCurve == null || brakeCurve.length == 0) {
					brakeCurve = new AnimationCurve (new Keyframe (0f, 1f),new Keyframe (1f, 1f), new Keyframe (1.01f, 0f), new Keyframe (testDuration - 10.2f, 0.0f), new Keyframe (testDuration - 10.1f, 1f), new Keyframe (testDuration - 0.1f, 1f), new Keyframe (testDuration, 0f)); 
					for (int i = 0; i < brakeCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (brakeCurve, i, AnimationUtility.TangentMode.Linear);
						AnimationUtility.SetKeyRightTangentMode (brakeCurve, i, AnimationUtility.TangentMode.Linear);
					}
				}


				//testDuration = 40.2f;
			} else if (type == TestTypes.CURVE) {
				// replicating steer step test (VeDYNA)
				requestNeutralGear = true;
				float t_steady = 3f;
				float t_acc = cruiseSpeed / 3.6f /1.5f; // /1.5f depending on acceleration capability
				float t_steer = steerAngle / steerRate;
				float t_man = t_acc + t_steady + t_steer + t_steady;

				if (testDuration == 0)
					testDuration = t_man;
				
				pid = new PID (Kp, Ki, Kd);

				if (speedCurve == null || speedCurve.length == 0) {
					Debug.Log ("Creating speed curve");
					speedCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (0.99f, 0f), new Keyframe (1f, cruiseSpeed/3.6f), new Keyframe (t_man, cruiseSpeed/3.6f)); 
//					speedCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (1f, cruiseSpeed/3.6f), new Keyframe(t_acc + t_steady - 0.1f, cruiseSpeed/3.6f), new Keyframe(t_acc + t_steady - 0.0999f, -1f), new Keyframe (t_man, -1f));  // val > 1 --> hold throttle
//					speedCurve = new AnimationCurve (new Keyframe (0f, 40f/3.6f), new Keyframe (t_man, cruiseSpeed/3.6f)); 
					for (int i = 0; i < speedCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (speedCurve, i, AnimationUtility.TangentMode.Constant);
						AnimationUtility.SetKeyRightTangentMode (speedCurve, i, AnimationUtility.TangentMode.Constant);
					}
				}


				if (steeringCurve == null || steeringCurve.length == 0) {
					steeringCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (t_acc + t_steady, 0f), new Keyframe(t_acc + t_steady + t_steer, steerAngle/495f), new Keyframe(t_man, steerAngle/495f)); 
					for (int i = 0; i < steeringCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (steeringCurve, i, AnimationUtility.TangentMode.Linear);
						AnimationUtility.SetKeyRightTangentMode (steeringCurve, i, AnimationUtility.TangentMode.Linear);
					}
				}

				if (brakeCurve == null || brakeCurve.length == 0) {
					brakeCurve = new AnimationCurve (new Keyframe (0f, 0f), new Keyframe (t_man, 0f)); 
					for (int i = 0; i < brakeCurve.keys.Length; i++) {
						AnimationUtility.SetKeyLeftTangentMode (brakeCurve, i, AnimationUtility.TangentMode.Linear);
						AnimationUtility.SetKeyRightTangentMode (brakeCurve, i, AnimationUtility.TangentMode.Linear);
					}
				}

			}
		
		}
	
		// Update is called once per frame
		void FixedUpdate ()
		{
			float relativeTime = Time.time - startTime;
			if (type == TestTypes.CURVE) {
				if (Time.deltaTime != 0) {
					if (relativeTime >= 1.0f && relativeTime <= 1.2f) {
						gearUp = true;
						pid.Restart ();
					}else
						gearUp = false;
//					if (speedCurve.Evaluate (relativeTime) >= 0f) {
						throttle = pid.Update (speedCurve.Evaluate (relativeTime), info.speed, Time.fixedDeltaTime);
//					} else {
						// do nothing, keep throttle
//					}
					Debug.Log ("throttle = " + throttle);
				}
//				Debug.Log ("throttle" + throttle + ", speedCurve.Evaluate(relativeTime) = " +speedCurve.Evaluate(relativeTime)+", info.speed = " +info.speed + ", relativeTime = " + relativeTime + ", Time.deltaTime =" + Time.deltaTime);

				steeringWheelRotation = steeringCurve.Evaluate (relativeTime) * maxSteerWheelRotation;
				brake = brakeCurve.Evaluate (relativeTime);
			} else {
				if (relativeTime >= 1.0f && relativeTime <= 1.2f)
					gearUp = true;
				else
					gearUp = false;
				throttle = throttleCurve.Evaluate (relativeTime);
				steeringWheelRotation = steeringCurve.Evaluate (relativeTime) * maxSteerWheelRotation;
				brake = brakeCurve.Evaluate (relativeTime);


			}
			if (relativeTime > testDuration) {
				//Application.Quit ();
				UnityEditor.EditorApplication.isPlaying = false;
			}
		}
	}
}