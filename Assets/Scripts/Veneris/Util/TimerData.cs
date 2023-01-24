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
	[System.Serializable]
	public class TimerData
	{
		public float startTime=-1f;
		public float maxTime = -1f;
		public TimerData(float maxTime) {
			this.maxTime = maxTime;
			startTime = -1f;
		}
		public bool IsSet() {
			if (startTime >= 0f) {
				return true;
			} else {
				return false;
			}

		}
		public void Set() {
			startTime = Time.time;
		}
		public void Unset() {
			startTime = -1f;
		}
		public bool Check() {
			if (startTime >= 0) {
				if ((Time.time-startTime)>maxTime) {
					return true;
				}
			}
			return false;
		}

	}
}
