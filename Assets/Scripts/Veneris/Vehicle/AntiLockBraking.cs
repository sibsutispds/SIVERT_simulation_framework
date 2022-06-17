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
	public class AntiLockBraking : MonoBehaviour
	{
		private CarController _carController;
		private BaseCarInputController input;

		public float maxDeccel = 10f;
		public float absOnSpeedRatio = 1.1f;
		public float absMinSpeed = 1f;
		public float absOffSpeedRatio = 1.0f;
		public float cyclesPerSecond = 16f;
		private float absPeriod;
		protected WaitForSeconds intervalAbs = null;
		private int numWheels;

		//		private Axle _frontAxle;
		//		private Axle _rearAxle;
		private List<Wheel> wheels;
		//private List<float> rpms;
		public List<float> prevAngSpeeds;
		public List<float> angAccels;
		public List<bool> kicked;

		private float timeleft;
		void Awake ()
		{
			_carController = GetComponent<CarController> ();
			wheels = new List<Wheel> ();
			angAccels = new List<float> ();
			prevAngSpeeds = new List<float> ();
			kicked = new List<bool> ();
			absPeriod = 1f / cyclesPerSecond;
			timeleft =	absPeriod;
		}

		// Use this for initialization
		void Start ()
		{
			input = _carController.input;
			numWheels = 0;
			foreach (Axle axle in _carController.axles) {
				wheels.Add (axle.wheels [0]);
				wheels.Add (axle.wheels [1]);
				angAccels.Add (0f);
				angAccels.Add (0f);
				prevAngSpeeds.Add (0f);
				prevAngSpeeds.Add (0f);
				kicked.Add (false);
				kicked.Add (false);
				numWheels += 2;
			}

//			foreach (Axle axle in _carController.axles) {
//				if (axle.wheels [0].transform.localPosition.z > 0)
//					_frontAxle = axle;
//				else if (axle.wheels [0].transform.localPosition.z < 0)
//					_rearAxle = axle;
//			}

			absPeriod = 1f / cyclesPerSecond;
			//intervalAbs = new WaitForSeconds (absPeriod);
			//StartCoroutine (AbsUpdate ());

		}

		IEnumerator AbsUpdate ()
		{
//			float prevSpeed;
//			float prevAccel;
			yield return intervalAbs;
			for (int i = 0; i < numWheels; i++) {
				
				angAccels [i] = (wheels [i].angSpeed - prevAngSpeeds [i]) / absPeriod;
				prevAngSpeeds [i] = wheels [i].angSpeed;
				if (_carController.vLong > 3f && _carController.input.brake > 0.05f) {
					if (angAccels [i] < -maxDeccel) {
						if (wheels [i].brake.enabled) {
							wheels [i].brake.Release ();
							Debug.Log ("ABS kicked in");
							kicked [i] = true;
						}

					}

				}
				if ((angAccels [i] > -maxDeccel) && (!wheels [i].brake.enabled)) {
					//if (wheels [i].enabled == false) {
						Debug.Log ("ABS kicked off");
					//}
					wheels [i].brake.Unrelease ();

					kicked [i] = false;
				}


			}
			StartCoroutine (AbsUpdate ());



		}
		void Update() {
			timeleft -= Time.deltaTime;
			if (timeleft <= 0.0) {
				for (int i = 0; i < numWheels; i++) {

					angAccels [i] = (wheels [i].angSpeed - prevAngSpeeds [i]) / absPeriod;
					prevAngSpeeds [i] = wheels [i].angSpeed;
					if (_carController.vLong > 3f && _carController.input.brake > 0.05f) {
						if (angAccels [i] < -maxDeccel) {
							if (wheels [i].brake.enabled) {
								wheels [i].brake.Release ();
								Debug.Log ("ABS kicked in");
								kicked [i] = true;
							}

						}

					}
					if ((angAccels [i] > -maxDeccel) && (!wheels [i].brake.enabled)) {
						//if (wheels [i].enabled == false) {
						Debug.Log ("ABS kicked off");
						//}
						wheels [i].brake.Unrelease ();

						kicked [i] = false;
					}


				}
				timeleft =	absPeriod;
			}

		}
		//void FixedUpdate ()
		//{
			//if (_frontAxle / 
		//}
	}
}
