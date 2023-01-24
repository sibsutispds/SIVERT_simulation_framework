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
	
	public class OpalPeriodicTransmitter : VenerisTransceiver
	{
		
		public float txPower=1.0f; 
		public int transmissions=0;
		public Vector3 polarization; //TODO: only consider purely vertical or horizontal polarization. Can be extended by usign the actual rotation of the rigidbody representing the antenna
		public float beaconingRate=10;
		protected float beaconingInterval = 0.1f;
		protected float lastTransmission=0.0f;
		protected Vector3ToMarshal polarizationM;
		protected override void OnEnable ()
		{
			//Debug.Log ("OpalPeriodicTransmitter::OnEnable()");
			base.OnEnable ();
			polarizationM = OpalInterface.ToMarshal(polarization);
			beaconingInterval = 1 / beaconingRate;
			lastTransmission = -1.0f;
		}
		void FixedUpdate ()
		{

				//var watch = System.Diagnostics.Stopwatch.StartNew ();
			if ((Time.time - lastTransmission) > beaconingInterval) {
				Transmit ();
				//watch.Stop ();
				//Debug.Log ("Time to transmit=" + (watch.ElapsedMilliseconds / 1000f) + " s");
				transmissions++;
				lastTransmission = Time.time;
			}

		}
		public void Transmit() {

			//Debug.Log (Time.time+"\t. Transmit:"+transform.position+"id="+id+"txPower="+txPower );
			OpalManager.Instance.Transmit(id,txPower,transform.position,polarizationM);
			//OpalManager.Instance.Transmit(id,txPower,transform.position,polarization);

		}
	}
}
