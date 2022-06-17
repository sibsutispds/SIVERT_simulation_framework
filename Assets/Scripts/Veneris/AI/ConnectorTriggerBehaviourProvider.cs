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
	public class ConnectorTriggerBehaviourProvider : AIBehaviourProvider
	{
		public PathConnector connector = null;

		void Awake ()
		{
			connector = GetComponentInChildren<PathConnector> ();

		}

		// Use this for initialization
		void Start ()
		{
			use = new Usage (0, UseFrequency.Always, int.MaxValue);

		
		}

		public override bool SetBehaviour (GameObject go, out AIBehaviour newBehaviour)
		{
			Debug.Log ("Setting ConnectorTrigger Behaviour");
			base.SetBehaviour (go, out newBehaviour);
			if (CheckUseLimit ()) {
				
				ConnectorTrigger ct = go.AddComponent<ConnectorTrigger> ();
				ct.SetConnector (connector);

				ct.Prepare ();
				go.GetComponent<AILogic> ().SetCurrentBehaviour (ct);
				use.timesUsed += 1;
				newBehaviour = ct;
				return true;

			}
			newBehaviour = null;
			return false;
		}

	}
}
