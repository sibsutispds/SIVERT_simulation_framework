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
	
	public class VehicleSafetyAreaDetector : MonoBehaviour
	{
		public enum AreaPosition
		{
			Front,
			Back,
			Left,
			Right,
		}
		public AreaPosition position;
		public VehicleInfo info = null;
		public List<Collider> detectedVehicles=null;

		// Use this for initialization
		void Start ()
		{
			detectedVehicles = new List<Collider> (8);
			info = transform.root.GetComponent<VehicleInfo> ();
		}
	
		public void RemoveVehicle(VehicleInfo v) {
			for (int i = detectedVehicles.Count-1; i >=0 ; i--) {
				if (detectedVehicles[i] != null) {
					if (detectedVehicles [i].GetComponent<VehicleSafetyAreaDetector> ().info == v) {
						detectedVehicles.RemoveAt (i);
					}
				} else {
					detectedVehicles.RemoveAt (i);
				}
				
			}
		}
	
		void OnTriggerEnter(Collider other) {
			
			detectedVehicles.Add ( other);
		}
		void OnTriggerExit(Collider other) {
			
			detectedVehicles.Remove (other);
		}
	}
}
