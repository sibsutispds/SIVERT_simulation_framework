/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris.Vehicle
{
	public  class BaseCarInputController : MonoBehaviour
	{
		public float throttle; // 0..1
		/*public float throttle{
			get { return _throttle; }
			set { _throttle = Mathf.Clamp01 (value); }
		}*/
		public float clutch = 0;

		public float steeringWheelRotation; //degrees
		[SerializeField]
		public float _brake; // 0..1
		public float brake {
			get { return _brake; }
			set {if (float.IsNaN(value)) {
				throw new UnityException();
			} else {_brake = value;
			}
				}
		}
		public bool requestReverseGear;

		public bool gearUp;
		public bool gearDown;
		public bool requestNeutralGear = false;
		public float maxSteerWheelRotation = 30f;


		// Use this for initialization, Start is not called in an abstract class
		public void Init ()
		{
			//Debug.Log ("start basecar");
			throttle = 0.0f;
			steeringWheelRotation = 0.0f;
			brake = 0.0f;
			gearUp = false;
			gearDown = false;

			requestReverseGear = false;
		}

		/*void Update ()
		{
			readInput ();
		}


		// Set thtottle, steer and brake values;
		public virtual void readInput ()
		{

		}
		*/

	}
}