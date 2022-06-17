/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Opal
{
	public class Transmitter : MonoBehaviour
	{

		// Use this for initialization
		public int id=0;
		public float txPower=0.158f; //22 dBm
		public int transmissions=0;
		public Vector3 polarization; //TODO: only consider purely vertical or horizontal polarization. Can be extended by usign the actual rotation of the rigidbody representing the antenna
		protected Vector3ToMarshal polarizationM;

		void Start ()
		{
			
			if (transform.rotation.eulerAngles.x == 0 && transform.rotation.eulerAngles.z == 0) {
				polarization = Vector3.up;

			} else {
				polarization = Vector3.forward;
			}

			Debug.Log ("Transmitter: " + id + ". Polarization=" + polarization+"Tx Power="+txPower);
			polarizationM = OpalInterface.ToMarshal(polarization);
			//Debug.DrawRay (transform.position, Vector3.forward*10f,Color.blue);
			//Debug.DrawRay (transform.position, Quaternion.Euler (45f, 0.0f, 0.0f) * Vector3.forward*10f, Color.red);
		}
	
	
		/*void FixedUpdate ()
		{
			
				var watch = System.Diagnostics.Stopwatch.StartNew ();
				Transmit ();
				watch.Stop ();
				Debug.Log ("Time to transmit=" + (watch.ElapsedMilliseconds / 1000f).ToString("E") + " s");
				

		
		}*/



		public void Transmit() {
			

			OpalManager.Instance.Transmit(id,txPower,transform.position,polarizationM);
			transmissions++;
			//OpalManager.Instance.Transmit(id,txPower,transform.position,polarization);

		}
		public void Transmit(float p) {
			Debug.Log (Time.time+"\t. Transmit:"+transform.position );
			OpalManager.Instance.Transmit(id,p,transform.position,polarizationM);
			transmissions++;
			//OpalManager.Instance.Transmit(id,txPower,transform.position,polarization);
		}
		public Vector3ToMarshal GetPolarizationMarshaled() {
			return polarizationM;
		}

	}
}
