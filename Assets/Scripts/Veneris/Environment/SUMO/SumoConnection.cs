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
	public class SumoConnection 
	{
		public string fromSumoId;
		public VenerisRoad fromRoad;
		public string toSumoId;
		public VenerisRoad toRoad;

		public VenerisLane fromLane;
		public  VenerisLane toLane;
		public string via; //Make this string to let serialize
		public bool internalLane;
		public Path internalPath;
		public SumoConnection(string fromSumoId, VenerisRoad fromRoad, string toSumoId, 	VenerisRoad toRoad, VenerisLane fromLane, VenerisLane toLane, string via, bool internalLane, Path internalPath) {
			this.fromSumoId = fromSumoId;
			this.fromRoad = fromRoad;
			this.toSumoId = toSumoId;
			this.toRoad = toRoad;
			this.fromLane = fromLane;
			this.toLane = toLane;
			this.via = via;
			this.internalLane = internalLane;
			this.internalPath = internalPath;
		}
	}
}
