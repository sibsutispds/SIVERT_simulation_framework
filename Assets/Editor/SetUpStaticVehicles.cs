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
	public class SetUpStaticVehicles
	{
	
		[MenuItem ("Opal/Generate Vehicles")]
		public static void GenerateVehicles ()
		{ 
			int xv = 60;
			int zv = 40;
			int c = 0;
			GameObject m = new GameObject ("Mover");
			m.transform.position = new Vector3 (2752f, 0.6f, 791.92f);
			GameObject root = new GameObject ("Vehicles");

			while (c < xv) {
				GameObject v;
				/*if (c % 10 == 0) {
					v = GameObject.Instantiate ((GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Resources/Prefabs/Vehicles/StaticSTDRSVenerisTruck.prefab", typeof(GameObject)));
					m.transform.Translate (Vector3.right * 6.5f);
				} else {
					 v = GameObject.Instantiate ((GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Resources/Prefabs/Vehicles/StaticSTDRSVeneris.prefab", typeof(GameObject)));
				}*/
				v = GameObject.Instantiate ((GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Resources/Prefabs/Vehicles/StaticSTDRSVeneris.prefab", typeof(GameObject)));
				v.transform.position = m.transform.position;
				v.transform.rotation = Quaternion.AngleAxis (90f, Vector3.up);
				v.name = "Vehicle " + c;
				v.transform.SetParent (root.transform);
				VehicleInfo vi = v.GetComponent<VehicleInfo> ();
				vi.vehicleId = c;

				v.SetActive (true);
				/*if (c % 10 == 0) {
					m.transform.Translate (Vector3.right * 12f);
				} else {
					m.transform.Translate (Vector3.right * 6.5f);
				}*/
				m.transform.Translate (Vector3.right * 6.5f);
				c++;
			}
			m.transform.position = new Vector3 (2982.8f, 0.6f,925.37f);
			int d = 0;
			/*while (d < zv) {
				GameObject v = GameObject.Instantiate ((GameObject)AssetDatabase.LoadAssetAtPath ("Assets/Resources/Prefabs/Vehicles/StaticSTDRSVeneris.prefab", typeof(GameObject)));
				v.transform.position = m.transform.position;
				v.transform.rotation = Quaternion.AngleAxis (180f, Vector3.up);
				v.name = "Vehicle " + c;
				v.transform.SetParent (root.transform);
				VehicleInfo vi = v.GetComponent<VehicleInfo> ();
				vi.vehicleId = c;
				c++;
				d++;
				v.SetActive (true);
				m.transform.Translate (Vector3.back * 6.5f);
			}*/


		}
	}
}
