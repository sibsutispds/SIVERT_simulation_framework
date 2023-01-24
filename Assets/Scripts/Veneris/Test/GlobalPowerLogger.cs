/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalPowerLogger : MonoBehaviour {

	// Use this for initialization
	public string path;
	public string logName;
	public Opal.Receiver[]  receivers;
	protected Veneris.FileResultLogger logger;

	void Awake () {
		
		receivers = GameObject.FindObjectsOfType<Opal.Receiver> ();

		for (int i = 0; i < receivers.Length; i++) {
			receivers [i].RegisterPowerListener (LogPower);
		}
		Debug.Log (receivers.Length + " receivers registered");
		logger = new Veneris.FileResultLogger (path, logName, true, false);
		logger.CreateStream ();
	}
	protected void LogPower (int rxId, float power, int txId)
	{
		Debug.Log (txId + "\t" + rxId + "\t" + power);
		logger.RecordWithTimestamp (txId+"\t"+rxId+"\t" + power);
	}

	void OnDestroy ()
	{
		
		logger.Close ();
		for (int i = 0; i < receivers.Length; i++) {
			if (receivers[i]!=null) {
				receivers [i].RemovePowerListener (LogPower);
			}
		}
	}

}
