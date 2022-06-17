/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris {
public class VenerisRoad : MonoBehaviour {
	public enum Ways {	OneWay,		TwoWay}	;
	public long roadId=0;
	public long edgeId = 0;
	public int kind = 0;
	public string roadName="";
	public string sumoId = "";
	public float totalWidth=0f;
	public Ways ways=Ways.TwoWay;

	public Path directorRoadPath;

	public VenerisLane[] lanes;

	// Use this for initialization
	void Start () {
	
	}
	public VenerisLane GetMyLane(Transform t) {
			
			for (int i = 0; i < lanes.Length; i++) {
				
			 
				if (lanes[i].IsOnLane(t)) {
					return lanes[i];
			}
		}
		return null;
	}
	
	}
}
