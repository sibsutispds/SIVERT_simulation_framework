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
	public class Brake : MonoBehaviour
	{
		public Wheel wheel;

		public bool enabled = true;
		public float hydraulicPreassure;
		public float brakePressure;
		public float brakeFrictionCoefficient = 0.33f;
		public float brakePadArea = 0.005f;
		//public float slaveCylinderPressureGain = 4.0f; // area of the slave cylinder / area of the master cylinder
		public float slaveCylinderArea = 0.0018f * 2f; // area of the slave cylinder, x2 if two cylinders
		public float brakeRadius = 0.105f;
		// distante of the brake pad to the center of the wheel, find realistic value

		public float brakeTorque = 0.0f;
		private float actualBrakePressure;
		// brake pressure after ABS, etc.
		[HideInInspector]
		public bool mustSetDefaults = false;

		void Awake ()
		{
			wheel = GetComponent<Wheel> ();
		}

		void Start ()
		{

			brakeTorque = 0.0f;
			hydraulicPreassure = 10.0f;

			if (mustSetDefaults) {
				SetDefaults ();
				mustSetDefaults = false;
			}
		}

		void FixedUpdate ()
		{
			if (enabled)
				BrakeFixedUpdate ();
			else 
				wheel.brakeTorque = 0f;	
		}

		// uncomment FixedUpdate or call from the braking system component in an ordered way
		public void BrakeFixedUpdate ()
		{
			brakePressure = hydraulicPreassure * slaveCylinderArea / brakePadArea;
			actualBrakePressure = brakePressure; // will be used with ABS?
			brakeTorque = brakePadArea * brakeFrictionCoefficient * brakeRadius * actualBrakePressure; // * Mathf.Sign (wheel.rpm);
			wheel.brakeTorque = brakeTorque;

		}

		public void Unrelease(){
			enabled = true;
		}

		public void Release(){
			enabled = false;
			brakePressure = 0f;
			brakeTorque = 0f;
			wheel.brakeTorque = 0f;
		}

		/// <summary>
		/// Sets the defaults values of the brake. Takes into account if it belongs to the FRONT or REAR axles
		/// The inAxle must be set to Wheel.axleEnum.FRONT or Wheel.axleEnum.REAR
		/// </summary>
		public void SetDefaults(){
			brakeFrictionCoefficient = 0.33f;
			brakePadArea = 0.005f; // this value doesn't really matter for the brake torque

			if (wheel.inAxle == Wheel.axleEnum.FRONT) {
				slaveCylinderArea = 0.0018f * 2f;
				brakeRadius = 0.105f;
			} else {
				slaveCylinderArea = 0.000855f * 2f;
				brakeRadius = 0.113f;
			}

			mustSetDefaults = false;

		}

		/*void Update ()
		{

		}*/
	}

}
