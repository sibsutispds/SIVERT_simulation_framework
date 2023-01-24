/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
namespace Veneris {
public class AIBehaviourProvider : MonoBehaviour {
		public enum UseFrequency {Always, Once, Limited};
		[System.Serializable]
		public class Usage {
			public int timesUsed ;
			public UseFrequency repetition;
			public int maxUses ;
			public Usage(int times, UseFrequency f, int max) {
				this.timesUsed=times;
				this.repetition=f;
				this.maxUses=max;
			}
		}
		public Usage use;

	public virtual bool SetBehaviour(GameObject go, out AIBehaviour b) {
			b = null;
			return false;
	}
	public virtual void CheckBehaviourValidity (GameObject go) {

	}
	public bool CheckUseLimit() {
			
			switch (use.repetition) {
			
			case UseFrequency.Once:
				if (use.timesUsed >= 1) {
					
					return false;
				}
				break;
			case UseFrequency.Limited:
				if (use.timesUsed >= use.maxUses) {
					return false;
				}
				break;
			
			}
			return true;
	}

}
}
