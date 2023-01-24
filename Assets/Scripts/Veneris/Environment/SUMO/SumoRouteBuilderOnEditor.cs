/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Veneris
{
	public class SumoRouteBuilderOnEditor : SumoRouteBuilder
	{
		public override GameObject LoadVehiclePrefab() {
			return AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Resources/Prefabs/Vehicles/STDRSCar.prefab");
		}

	}
}
#endif
