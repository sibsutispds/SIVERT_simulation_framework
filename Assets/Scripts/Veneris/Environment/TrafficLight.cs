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
	public class TrafficLight : MonoBehaviour
	{
		public enum TrafficLightState 
		{
			Undefined,
			//Not set yet
			Red,
			//Vehicles must stop
			Amber,
			// yellow, vehicles start to decelerate if far away, otherwise they pass
			GreenNoPriority,
			// no priority - vehicles may pass the junction if no vehicle uses a higher priorised foe stream, otherwise they decelerate for letting it pass.
			Green,
			// priority - vehicles may pass the junction 
			RedAmber,
			//red+yellow light' for a signal, may be used to indicate upcoming green phase but vehicles may not drive yet
			OffBlinking,
			//'off - blinking' signal is switched off, blinking light indicates vehicles have to yield 
			OffNoSignal,
			//'off - no signal' signal is switched off, vehicles have the right of way 
		}
		public enum TrafficLightType {
			Actuated,
			Static,
		}
		[System.Serializable]
		public class TrafficLightPhase {
			public float duration;
			public List<TrafficLightState> sequence = null;
			public TrafficLightPhase(float d, List<TrafficLightState> seq) {
				this.duration=d;
				sequence=seq;
			}

		}

		public string sumoId;
		public TrafficLightType sumoType;
		public float offset;
		public string sumoProgramId;
		public List<TrafficLightPhase> phases;
		public int currentPhase=0;

		protected Coroutine changePhaseCoroutine =null;
		public delegate void PhaseListener ();	
		public PhaseListener listeners=null;

		// Use this for initialization
		void Start ()
		{
			currentPhase = 0;

			changePhaseCoroutine = StartCoroutine (StartNewPhase());
		}

		IEnumerator StartNewPhase() {
			while (true) {
				
				yield return new WaitForSeconds (phases[currentPhase].duration);

				currentPhase++;
				currentPhase %= phases.Count;
				//Invoke listeners

				if (listeners != null) {

					listeners ();
				}
			}
		}
	
		public void AddTrafficLightPhase(TrafficLightPhase phase) {
			if (phases == null) {
				phases = new List<TrafficLightPhase> ();
			}
			phases.Add (phase);
		}
		public TrafficLightState GetState(int index) {
			return phases [currentPhase].sequence [index];
		}
		public void RegisterPhaseListener(PhaseListener l) {
			listeners += l;
		}
	}
}
