/******************************************************************************/
// 
// Copyright (c) 2019 Fernando Losilla 
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;

namespace Veneris.Vehicle
{
	public class AeroDrag : MonoBehaviour
	{
		
		public Vector3 cDrag = new Vector3 (0.5f, 0f, 0.32f);
		public Vector3 area = new Vector3 (2.5f, 0f, 2.26f);


		private Rigidbody rb;

		public Vector3 _dragForce;
		//private float factor;

		// Use this for initialization
		void Start ()
		{
			rb = GetComponent<Rigidbody> ();
			if (rb == null) {
				Debug.Log (this.name + ": Disabling drag force. Aero component must be attached to a vehicle root object with a Rigidbody component.");
				this.enabled = false;
				return;
			}

			//factor = 0.5f * 1.293f * cDrag * area;

		}

		// Update is called once per frame
		void FixedUpdate ()
		{
			applyForce ();
		}

		public void applyForce ()
		{
			// Drag equation
			_dragForce = -0.6465f * Vector3.Scale (Vector3.Scale(cDrag, area),  rb.velocity) * rb.velocity.magnitude; // 1/2 * 1.293 * cDrag * area * v * v;
			rb.AddForce (_dragForce, ForceMode.Force);
		}
	}
}