/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
namespace Veneris {
public class ForcedDecelerationBehaviourProvider : AIBehaviourProvider {

		public float duration = -1f;
		public float speedLimit=-1f;
		public float throttleIntensity = 1f;

	
	// Use this for initialization
	void Start () {
			use = new Usage (0,UseFrequency.Once,1 );

	
	}
	
		public override bool SetBehaviour (GameObject go, out AIBehaviour newBehaviour)
		{
			base.SetBehaviour (go, out newBehaviour);
			if (CheckUseLimit ()) {
				ForcedDeceleration f = go.AddComponent<ForcedDeceleration> ();
				f.duration = duration;
				f.speedLimit = speedLimit;
				f.throttleIntensity = throttleIntensity;
				f.Prepare ();
				go.GetComponent<AILogic> ().AddBehaviourToTaskList (f);
				use.timesUsed += 1;
				newBehaviour = f;
				return true;
			}
			newBehaviour = null;
			return false;
		}

}
}
