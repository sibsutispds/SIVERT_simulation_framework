/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opal;
namespace Veneris
{
	//With Veneris, transmission and reception is delegated to OMNET++ modules. This class is just a Receiver, but we have changed the name to remark that transmitter and receivers are united in Veneris
	//Since they are the same module, the transmitter position gets updated also when the receiver is updated.
	public class VenerisTransceiver : Receiver
	{
		public Veneris.AILogic ailogic;



		protected override void OnEnable ()
		{
			Veneris.VehicleInfo vi=GetComponentInParent<Veneris.VehicleInfo>();
			if (vi != null) {
				
				this.id = vi.vehicleId;

			}
		
			base.OnEnable ();

			ailogic = transform.root.GetComponentInChildren<Veneris.AILogic>();
			if (ailogic != null) {
				if (ailogic.vehicleManager != null) {
					ailogic.vehicleManager.AddRemoveListener (HandleDestroyTrigger);
					//Debug.Log (Time.time+": VenerisTransceiver: added remove listener for " + id);
				}
			}
		}
		protected void HandleDestroyTrigger(Veneris.VehicleInfo info) {
			//Unregister here
			if (info.vehicleId == id) {
				Debug.Log (Time.time + ": HandleDestroyTrigger called for " + id);
				if (OpalManager.isInitialized && registered) {
					Debug.Log (Time.time + ": Removing receiver " + id + " on destroy trigger");
					OpalManager.Instance.UnregisterReceiver (this);
					registered = false;
				} else {
					Debug.Log ("Registered=" + registered);
					Debug.Log ("OpalManager.isInitialized=" + OpalManager.isInitialized);
				}
				DynamicMesh dm = transform.root.GetComponent<DynamicMesh> ();
				if (dm != null) {
					Debug.Log (Time.time + ": Removing dynamic mesh " + id + " on destroy trigger");
					dm.RemoveGroup ();
				}
				DynamicMesh[] dms = transform.root.GetComponentsInChildren<DynamicMesh> ();
				for (int i = 0; i < dms.Length; i++) {
					dms [i].RemoveGroup ();
				}
			}
			

		}
		protected override void OnDestroy ()
		{
			base.OnDestroy ();

			if (ailogic != null) {
				ailogic.vehicleManager.RemoveRemoveListener (HandleDestroyTrigger);
			}

		}
		protected override void OnDisable ()
		{
			base.OnDisable ();

		}
	}
}
