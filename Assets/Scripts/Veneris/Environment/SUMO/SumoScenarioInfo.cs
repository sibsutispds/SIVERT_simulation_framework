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
	public class SumoScenarioInfo : MonoBehaviour
	{

		public float[] bounds; 
		public void SetBounds(float[] b) {
			this.bounds = b;
		}
		public Vector3 GetCenter() {
			Debug.Log(new Vector3((bounds[0]+bounds[2])*0.5f,transform.position.y,(bounds[1]+bounds[3])*0.5f));
			return new Vector3((bounds[0]+bounds[2])*0.5f,transform.position.y,(bounds[1]+bounds[3])*0.5f);
		}
	}
}
