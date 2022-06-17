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
	public class TrafficLightSet : MonoBehaviour
	{
		public TrafficLight trafficLight = null;
		public int index = 0;
		public Light red;
		public Light yellow;
		public Light green;
		protected Coroutine blinkCoroutine =null;
		protected WaitForSeconds intervalBlink = null;
		protected bool blinking = false;
		public float blinkTime = 1f;

		public void SetTrafficLight (TrafficLight t)
		{
			trafficLight = t;
			trafficLight.RegisterPhaseListener (PhaseChange);
			GetLights ();
			PhaseChange ();
		}
		void Start() {
			intervalBlink=new WaitForSeconds (blinkTime);
			trafficLight.RegisterPhaseListener (PhaseChange);
			PhaseChange ();

		}

		protected void GetLights ()
		{
			Light[] lights = transform.GetComponentsInChildren<Light> ();
			for (int i = 0; i < lights.Length; i++) {
				if (lights [i].name.Equals ("redlight")) {
					red = lights [i];
				} else if (lights [i].name.Equals ("greenlight")) {
					green = lights [i];
				} else {
					yellow = lights [i];
					;
				}
			}
		}

		public void SetIndex (int i)
		{
			index = i;
		}

		public void PhaseChange ()
		{
			SetCurrentLight(trafficLight.GetState (index));
		}

		public void SetCurrentLight (TrafficLight.TrafficLightState state)
		{
			
			TurnOffAll ();
			switch (state) {
			case TrafficLight.TrafficLightState.Green:
				green.enabled = true;

				break;
			case TrafficLight.TrafficLightState.Red:
				red.enabled = true;

				break;
			case TrafficLight.TrafficLightState.RedAmber:
				red.enabled = true;
				yellow.enabled = true;
				break;
			
			case TrafficLight.TrafficLightState.Amber:
				yellow.enabled = true;

				break;
			case TrafficLight.TrafficLightState.GreenNoPriority:
				// I would say this is blinking yellow
				StartYellowBlink();

				break;
			case TrafficLight.TrafficLightState.OffNoSignal:
				// already turned off, do not do anything

			break;
			}
		}

		public void StartYellowBlink() {
			yellow.enabled = true;
			blinkCoroutine=StartCoroutine (YellowBlinking());
			blinking = true;
		}
		public void StopYellowBlink() {
			yellow.enabled = false;
			if (blinking) {
				StopCoroutine (blinkCoroutine);
			}
			blinking = false;
		}

		IEnumerator YellowBlinking() {
			while (true) {
				yellow.enabled = !yellow.enabled;
				yield return intervalBlink;

			}
		}
		public void TurnOffAll ()
		{
			green.enabled = false;
			red.enabled = false;
			yellow.enabled = false;
			StopYellowBlink ();
		}
	}
}
