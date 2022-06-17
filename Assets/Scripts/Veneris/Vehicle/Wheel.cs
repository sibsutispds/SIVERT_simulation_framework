/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

namespace Veneris.Vehicle
{
	// [RequireComponent (typeof(WheelCollider))]
	public class Wheel : MonoBehaviour
	{
		//[HideInInspector]
		public WheelCollider wCollider;
		public Brake brake;

		//[HideInInspector]
		public float radius;
		public float mass = 40f;

		public float brakeTorque;
		public float motorTorque;
		public float steerAngle;
		public float diffTorque; // torque applied by a differential
		//public bool isPowered;
		//public bool isSteerable;

		//outputs
		public bool isGrounded;
		public float rpm;
		public float sprungMass;

		private float totalRotInertia;
		private float prevRpm;
		public float angSpeed;
		private float prevAngSpeed;
		public float angAccel;
		private float prevAngAccel;
		public float netTorque; // net torque (includes traction, drive, brake and others) that make wheels spin

		public enum axleEnum {FRONT, REAR};
		public axleEnum inAxle;
		[HideInInspector]
		public bool mustSetDefaults = false;

		public AnimationCurve forwardCurve;
		public AnimationCurve sidewaysCurve;

		public Transform visualWheel =null;

		// Use this for initialization
		void Awake(){
			wCollider = GetComponent<WheelCollider> ();
			if (wCollider == null) {
				wCollider = gameObject.AddComponent<WheelCollider> ();
				mustSetDefaults = true;

			} else {
				mustSetDefaults = false;
			}

			// Add brake, if no component was found create a new one with the default values
			brake = GetComponent<Brake> ();
			if (brake == null) {
				brake = gameObject.AddComponent<Brake> ();
				brake.mustSetDefaults = true;
			} else {
				brake.mustSetDefaults = false;
			}
		}



		void Start ()
		{
			//wCollider = GetComponent<WheelCollider> ();

			wCollider.ConfigureVehicleSubsteps (1f, 20, 20);
			//Time.fixedDeltaTime = 0.005f;

			// get wheel radius in local space

			Renderer renderer = gameObject.GetComponentInChildren<Renderer> ().GetComponent<Renderer> ();
			radius = renderer.bounds.size.y / 2f;
			wCollider.radius = radius; // check if local or world space

			if (mustSetDefaults) {
				SetDefaults ();
			}

			totalRotInertia = 0.5f * mass * radius * radius;


			steerAngle = 0f;
			brakeTorque = 0f;
			motorTorque = 0f;
			diffTorque = 0f;

			prevRpm = 0f;
			prevAngSpeed = 0f;
			prevAngAccel = 0f;

			forwardCurve = createAnimationCurve (wCollider.forwardFriction);
			sidewaysCurve = createAnimationCurve (wCollider.sidewaysFriction);
			visualWheel = wCollider.transform.GetChild (0);
		}


		public void SetDefaults(){
			JointSpring suspension = wCollider.suspensionSpring;
			wCollider.forceAppPointDistance = 0.32f; // from the botton of the wheel, empirically modify to improve car stability
			wCollider.wheelDampingRate =  1f;
			if (inAxle == Wheel.axleEnum.FRONT) {
				suspension.spring = 17200f;
				suspension.damper = 1820f;
				wCollider.suspensionDistance = 0.07f;//0.083f;
				suspension.targetPosition = 0.6f;
				//wCollider.suspensionDistance = 0.035f;
			} else {
				suspension.spring = 20200f;
				suspension.damper = 1650f;		
				wCollider.suspensionDistance = 0.07f;//0.093f;
				suspension.targetPosition = 0.45f;
				//wCollider.suspensionDistance = 0.025f;
			}
			//wCollider.suspensionDistance += 0.05f; // test
			//suspension.spring *= 1.5f; // OJO
			wCollider.suspensionSpring = suspension;


			WheelFrictionCurve forwardCurve = wCollider.forwardFriction;
			forwardCurve.stiffness = 0.9f;
			forwardCurve.extremumSlip = 0.15f;
			forwardCurve.extremumValue = 1f;
			forwardCurve.asymptoteSlip = 0.5f;
			forwardCurve.asymptoteValue = 0.9f;
			wCollider.forwardFriction = forwardCurve;

			WheelFrictionCurve sidewaysCurve = wCollider.forwardFriction;
			sidewaysCurve.stiffness = 0.8f;
			sidewaysCurve.extremumSlip = 0.03f; //0.05f;
			sidewaysCurve.extremumValue = 1f;
			sidewaysCurve.asymptoteSlip = 0.5f;
			sidewaysCurve.asymptoteValue = 0.9f;
			wCollider.sidewaysFriction = sidewaysCurve;

			mass = 32f;
			wCollider.mass = mass;


		}
	
		// Update is called once per frame
		/*void Update ()
		{
			wCollider.steerAngle = steerAngle;
			//ApplyLocalPositionToVisuals ();
//			Debug.Log ("SprungMass = " + wCollider.sprungMass);
		}
		*/

		void FixedUpdate(){
			wCollider.steerAngle = steerAngle;
			wCollider.motorTorque = motorTorque + diffTorque;
			wCollider.brakeTorque = brakeTorque;
			prevRpm = rpm;
			rpm = wCollider.rpm;
			isGrounded = wCollider.isGrounded;

			prevAngSpeed = angSpeed;
			angSpeed = rpm / 60f * 2 * Mathf.PI;
			angAccel = (angSpeed - prevAngSpeed) / Time.fixedDeltaTime;
			netTorque = angAccel * totalRotInertia;
		}

		public void ApplyLocalPositionToVisuals ()
		{
			if (wCollider == null) {
				return;
			}
		
			//Transform visualWheel = wCollider.transform.GetChild (0);
		
			Vector3 position;
			Quaternion rotation;
			wCollider.GetWorldPose (out position, out rotation);
		
			visualWheel.transform.position = position;
			visualWheel.transform.rotation = rotation;
		}

		//TODO
//		public float getReactionForce(){
//		//	float slipRatio = ()
//		}

		public AnimationCurve createAnimationCurve(WheelFrictionCurve frictCurve){
			AnimationCurve curve = new AnimationCurve ();
			Keyframe key = new Keyframe ();
			key.time = -frictCurve.asymptoteSlip;
			key.value = -frictCurve.asymptoteValue * frictCurve.stiffness;
			key.inTangent = 0f;
			key.outTangent = 0f;
			curve.AddKey (key);

			key.time = -frictCurve.extremumSlip;
			key.value = -frictCurve.extremumValue * frictCurve.stiffness;
			key.inTangent = 0f;
			key.outTangent = 0f;
			curve.AddKey (key);

			key.time = 0f;
			key.value = 0f;
			int origin_index = curve.AddKey (key);
			//AnimationUtility.SetKeyLeftTangentMode (curve, origin_index, AnimationUtility.TangentMode.Linear);
			//AnimationUtility.SetKeyRightTangentMode (curve, origin_index, AnimationUtility.TangentMode.Linear);
			curve.SmoothTangents (origin_index, 0f);

			key.time = frictCurve.extremumSlip;
			key.value = frictCurve.extremumValue * frictCurve.stiffness;
			key.inTangent = 0f;
			key.outTangent = 0f;
			curve.AddKey (key);

			key.time = frictCurve.asymptoteSlip;
			key.value = frictCurve.asymptoteValue * frictCurve.stiffness;
			key.inTangent = 0f;
			key.outTangent = 0f;
			curve.AddKey (key);

			return curve;
		}
	}

	//public 
}