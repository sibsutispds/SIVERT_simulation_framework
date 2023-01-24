/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Veneris;

namespace Opal
{
	public class Receiver : MonoBehaviour
	{



		public int id = 0;
		public float radius = 5f;
		protected GCHandle callbackHandle;
		protected bool registered=false;

		public SphereCollider sc;

		public delegate void OnPowerReceivedId (int rxId, float power, int txId);
		public delegate void OnPowerReceived ( float power, int txId);
		protected OnPowerReceivedId onPowerReceivedIdListeners;
		protected OnPowerReceived powerReceivedCallback;



		// Use this for initialization
		protected virtual void Awake ()
		{
			//For visualization purposes
		
			sc = GetComponent<SphereCollider> ();
			if (sc != null) {
				sc.radius = radius;
			}


			powerReceivedCallback = ReceivedPower;
			AllocateHandle ();


		}
		public virtual void AllocateHandle() {
			if (powerReceivedCallback != null) {
				callbackHandle = GCHandle.Alloc (powerReceivedCallback);
			}
		}
	
	
		protected virtual void OnEnable() {
			if (registered == false) {
				//Debug.Log (Time.time + ":Registering receiver " + id);
				OpalManager.Instance.RegisterReceiver (this);
				registered = true;
				transform.hasChanged = false;
			}

		}
		public void RegisterPowerListener(OnPowerReceivedId l) {
			onPowerReceivedIdListeners += l;
		}
		public void RemovePowerListener(OnPowerReceivedId l) {
			onPowerReceivedIdListeners -= l;
		}
	


		public IntPtr GetCallback ()
		{
			if (powerReceivedCallback != null) {
				return Marshal.GetFunctionPointerForDelegate (powerReceivedCallback);
			} else {
				throw new System.InvalidOperationException ("Callback null for receiver: " + id);
			}
		}


	

		public void recordPower (float power, int txId)
		{
			string m = Time.time+"\tRx[" + id + "]. Received p=" + power + " from " + txId;
			//SimulationManager.Instance.GetGeneralResultLogger ().Record (m);
		}
		public void printPower (float power, int txId)
		{
			string m = Time.time+"\tRx[" + id + "]. Received p=" + power + " from " + txId;

			Debug.Log (m);
		}
		protected void ReceivedPower(float power, int txId) {
			printPower (power, txId);
			if (onPowerReceivedIdListeners != null) {
				//printPower (power, txId);
				//Invoke listeners
				onPowerReceivedIdListeners (id,power, txId);
			}
		}
		public void UpdateTransform() {
			//Note that the transform may have been changed by other component prior to this call. If a call to Transmit() has been done in between, the power has been computed with the previous position
			transform.hasChanged = false;
			if (OpalManager.isInitialized) {
				OpalManager.Instance.UpdateReceiver (this);
			}
		}
		protected void FixedUpdate() {
			if (transform.hasChanged) {
				//Debug.Log ("transform has changed");

				//	Debug.Log (Time.time+"\t"+(transform.position - transmitter.position).magnitude );
				UpdateTransform ();
			}
		}
		protected virtual void OnDestroy ()
		{
			if (powerReceivedCallback != null) {
				callbackHandle.Free ();
			}
			if (OpalManager.isInitialized && registered) {
				Debug.Log (Time.time+": Removing receiver "+id+" on destroy");
				OpalManager.Instance.UnregisterReceiver (this);
				registered = false;
			}
		}
		protected virtual void OnDisable ()
		{
			
			if (OpalManager.isInitialized && registered) {
				Debug.Log (Time.time+": Removing receiver "+id+" on disable");
				OpalManager.Instance.UnregisterReceiver (this);
				registered = false;
			}
		}
	}
}
