/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Veneris.Vehicle
{
	//[RequireComponent (typeof(PowerTrain))]
	[RequireComponent (typeof(Rigidbody))]
	[RequireComponent (typeof(AeroDrag))]
	//[RequireComponent (typeof(SteerControl))]
	public class CarController : MonoBehaviour
	{
		//car parts
		public List<AxleInfo> axlesInfo;
		//[HideInInspector]
		public List<Axle> axles;

		//geometry
		public Transform centerOfMass;
		private Vector3 _centerofMassV3;

		//[HideInInspector]
		public float wheelBase;
		[HideInInspector]
		public float trackWidth;

		//car behaviour/properties
		public float maxSteeringAngle = 30;
		public float GripForce = 13500f; //13500 N, from our tests
		public float maxDeceleration = 10f; //10 m/s^2 from our tests


		public Rigidbody body;

		[HideInInspector]
		public Axle mainAxle;
		[HideInInspector]
		public SteerControl steerControl;




		//car info
		public float vLong; // longitudinal speed
		public float vLat; // lateral speed
		public float angSpeed; // Not used
		public Vector3 velocityV3 = new Vector3();
		public Vector3 accelV3 = new Vector3 ();

		private Vector3 _prevRotation;
		private Vector3 _curRotation;
		private float prevTime;

		public float distanceTraveled = 0f; //Like an odometer
		private Vector3 _prevPosition;

		//car inputs
		public BaseCarInputController input;
	
		// Changes to axles, input, Rigidbody, geometry, should be done here as they will be accessed from the Start methods of other components
		void Awake(){
			// temporary variables for car geometry calculations
			Wheel wheelFL = null;
			Wheel wheelFR = null;
			Wheel wheelRL = null;

			// Create axles and add differentials to them
			foreach (AxleInfo axleInfo in axlesInfo) {
				Axle axle = gameObject.AddComponent<Axle> ();
				axle.wheels.Add (axleInfo.leftWheel);
				axle.wheels.Add (axleInfo.rightWheel);
				axle.axleInfo = axleInfo;
				axles.Add(axle);

//				Differential differential = gameObject.AddComponent<Differential> ();
//				differential.axle = axle;
			}

			input = GetComponentInChildren<BaseCarInputController> ();
			if (input == null) {
					//input = gameObject.AddComponent<KeyboardCarInputController> ();
				throw new UnityException("Vehicle does not have a controller");
			}


//			if (GetComponent<SteerControl> () == null) {
//				gameObject.AddComponent<SteerControl> ();
//			}

			body = GetComponent<Rigidbody> ();

			// Add SteerControl component for steerable axles
			// and get wheel base and tracj width of the vehicle. Assume that one axle is steeraable and other is not

			foreach (Axle axle in axles) {
				if (axle.axleInfo.isSteerable) {
					wheelFL = axle.wheels [0];
					wheelFR = axle.wheels [1];
					//					SteerControl steerControl = gameObject.AddComponent<SteerControl> ();
					steerControl = gameObject.AddComponent<SteerControl> ();
					steerControl.wheelFL = axle.wheels [0];
					steerControl.wheelFR = axle.wheels [1];
					//steerControl.axle = 
				} else {
					wheelRL = axle.axleInfo.leftWheel;
				}
			}

			//TODO enter wheelbase and trackwidth as inputs?
			wheelBase = Mathf.Abs(wheelFL.transform.localPosition.z  - wheelRL.transform.localPosition.z) *  wheelFL.transform.lossyScale.z;
			trackWidth = Mathf.Abs(wheelFR.transform.localPosition.x - wheelFL.transform.localPosition.x) *  wheelFL.transform.lossyScale.x; 

			if (GetComponent<PowerTrain>() == null){
				gameObject.AddComponent<PowerTrain> ();
			}

			if (centerOfMass != null){
				_centerofMassV3 = Vector3.Scale(centerOfMass.localPosition, transform.lossyScale);
				//body.inertiaTensor = new Vector3 (1520f, 1750f, 305f);
				//body.mass = body.mass + 1f;
//				Debug.Log ("Inertia tensor = " + body.inertiaTensor);
//				Debug.Log ("New Center of mass");
			} else {
				_centerofMassV3 = body.centerOfMass;
			}

			body.centerOfMass = _centerofMassV3;

//			Debug.Log ("Wheelbase");
//			Debug.Log (wheelBase);
//			Debug.Log ("trackWidth");
//			Debug.Log (trackWidth);

			// inform wheels on which axle they are
			foreach (Axle axle in axles) {
				foreach (Wheel wheel in axle.wheels) {
					if (wheel.transform.localPosition.z >= 0f) {
						wheel.inAxle = Wheel.axleEnum.FRONT;
//						Debug.Log("Front wheel reference load = " + wheel.wCollider.sprungMass);
					} else {
						wheel.inAxle = Wheel.axleEnum.REAR;
//						Debug.Log("Rear wheel reference load = " + wheel.wCollider.sprungMass);
					}
					//Debug.Log("Wheel reference load = " + wheel.wCollider.sprungMass);
				}
			}

		}

		// Use this for initialization
		void Start ()
		{
			// change default Rigidbody mass (if not set to other value)
			if (body.mass == 1f)
				body.mass = 1335f;

			if (GetComponent<AeroDrag>() == null){
				gameObject.AddComponent<AeroDrag> ();
			}


			if (GetComponent<AntiRollBar> () == null) {
				foreach (Axle axle in axles) {
					AntiRollBar bar = gameObject.AddComponent<AntiRollBar> ();
					bar.wheelL = axle.wheels [0].wCollider;
					bar.wheelR = axle.wheels [1].wCollider;
					bar.body = body;
					//bar.antilRollCoef = axle.wheels [0].wCollider.suspensionSpring.spring/10f;
					if (axle.wheels [0].inAxle == Wheel.axleEnum.FRONT) {
						//bar.antilRollCoef = 708f * Mathf.Atan2(1f, trackWidth)*Mathf.Rad2Deg * (trackWidth/2f) / 2f; // from Nm/deg to N/m
						bar.antilRollCoef = 1000f; // * Mathf.Atan2(1f, trackWidth)*Mathf.Rad2Deg * (trackWidth/2f) / 2f; // from Nm/deg to N/m
					} else {
						//bar.antilRollCoef = 450f * Mathf.Atan2(1f, trackWidth)*Mathf.Rad2Deg * (trackWidth/2f) / 2f;
						bar.antilRollCoef = 600f;// * Mathf.Atan2(1f, trackWidth)*Mathf.Rad2Deg * (trackWidth/2f) / 2f;
					}
				}
			}

			if (GetComponent<BrakingSystem> () == null) {
				gameObject.AddComponent<BrakingSystem> ();
			}

			prevTime = Time.time;
			velocityV3 = transform.InverseTransformDirection (body.velocity);
			distanceTraveled = 0f;
			_prevPosition = body.position;
//			if (GetComponent<AntiLockBraking> () == null) {
//				gameObject.AddComponent<AntiLockBraking> ();
//			}

			//Time.fixedDeltaTime = 0.005f;

		}
	
		// Update is called once per frame
	/*	void Update ()
		{
//			Debug.Log ("transform.eulerAngles = " + transform.eulerAngles);
		}
		*/

		void FixedUpdate(){
			//updateSpeeds (Time.fixedDeltaTime);
			Vector3 prevVelo = velocityV3;

			velocityV3 = transform.InverseTransformDirection (body.velocity);
			vLong = velocityV3.z;
			vLat = velocityV3.x;

			accelV3 = (velocityV3 - prevVelo) / Time.deltaTime;

			//Debug.DrawRay (body.transform.position, body.velocity * 100.0f, Color.cyan);

			_prevRotation = _curRotation;
			_curRotation= transform.localRotation.eulerAngles;
			angSpeed = Mathf.Deg2Rad*(_curRotation.y - _prevRotation.y) / Time.deltaTime;

			distanceTraveled += (body.position - _prevPosition).magnitude;
			_prevPosition = body.position;
		}

	/*	public void updateSpeeds(float elapsedTime){
			Vector3 prevVelo = velocityV3;

			velocityV3 = transform.InverseTransformDirection (body.velocity);
			vLong = velocityV3.z;
			vLat = velocityV3.x;

			accelV3 = (velocityV3 - prevVelo) / elapsedTime;
				
			//Debug.DrawRay (body.transform.position, body.velocity * 100.0f, Color.cyan);

			_prevRotation = _curRotation;
			_curRotation= transform.localRotation.eulerAngles;
			angSpeed = Mathf.Deg2Rad*(_curRotation.y - _prevRotation.y) / elapsedTime;

			distanceTraveled += (body.position - _prevPosition).magnitude;
			_prevPosition = body.position;
		}
		*/

	}

}