/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using FluentBehaviourTree;
namespace Veneris {
public class ForcedDeceleration : AIBehaviour {

		public float duration = -1f;
		public float speedLimit=-1f;
		public AILogic ailogic=null;
		public float throttleIntensity = 1f;
		private float elapsedTime = -1f;


		void Awake() {
			if (ailogic == null) {
				ailogic = GetComponent<AILogic>();
			}
		}
	
		public override void Prepare ()
		{
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			mainBehaviour = builder.Sequence ("forced-deceleration").Do ("decelerate", ()=>this.Decelerate ()).Do("set-default",()=>this.SetDefault()).End().Build ();
			Debug.Log ("Forced deceleration during " + duration);
		
		}
		public FluentBehaviourTree.BehaviourTreeStatus Decelerate() {
			if (elapsedTime<0) {
				elapsedTime = 0f;
			}
			if (duration >= 0) {
				elapsedTime += Time.deltaTime;
				//Debug.Log (elapsedTime);
				if (elapsedTime < duration) {
					//Keep decelerating
					if (speedLimit >= 0) {
						if (ailogic.vehicleInfo.sqrSpeed <= (speedLimit * speedLimit)) {
							ailogic.throttle = 0f;
							return FluentBehaviourTree.BehaviourTreeStatus.Success;
						}
					}

					
					ailogic.throttle = -1f * throttleIntensity;
					return FluentBehaviourTree.BehaviourTreeStatus.Running;
					
				} else {
					ailogic.throttle = 0f;
					return FluentBehaviourTree.BehaviourTreeStatus.Success;
				}

			} else {
				ailogic.throttle = -1f * throttleIntensity;
				return FluentBehaviourTree.BehaviourTreeStatus.Running;
			}
		}
		public FluentBehaviourTree.BehaviourTreeStatus SetDefault() {
			ailogic.EndRunningBehaviour (this);
			Destroy (GetComponent<ForcedDeceleration> (),0.1f);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
}
}
