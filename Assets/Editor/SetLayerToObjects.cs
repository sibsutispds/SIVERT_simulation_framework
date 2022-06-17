/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Veneris
{
	
	public class SetLayerToObjects 
	{

		[MenuItem ("Veneris/Set Layer Lane")]
		static void SetLayer ()
		{
			VenerisLane[] v = GameObject.FindObjectsOfType (typeof(VenerisLane)) as VenerisLane[];
			for (int i = 0; i < v.Length; i++) {
				if (v [i].IsInternal ()) {
					continue;
				}
				v [i].gameObject.layer = LayerMask.NameToLayer ("Lane");

			}
		}
	

	}
}
