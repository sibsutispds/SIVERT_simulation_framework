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
	public class AIMDTimeScaleControl : MonoBehaviour
	{

		public bool applyControl = false;

	
		public int windowSize = 5;
		public float decreaseFactor = 2f;
		public float increaseTerm = 0.05f;
		public double decreaseFPSThreshold = 20;
		public float fuRatePercentageThreshold = 0.9f;

		public Average windowFpsAverage = null;
		public Average totalFpsAverage = null;
		public Average timeScaleAverage = null;
		public Average windowFURateAverage = null;
		public Average totalFURateAverage = null;

		public delegate void TimeScaleControlListener ();

		public TimeScaleControlListener listeners;

		public int counter = 0;
		public bool fasterThanRealTime = false;
		private float timeleft;

	
		void Start ()
		{
			  
			windowFpsAverage = new Average ();
			windowFpsAverage.Init ();
			windowFURateAverage = new Average ();
			windowFURateAverage.Init ();
			if (totalFURateAverage == null) {
				totalFURateAverage = new Average ();
				totalFURateAverage.Init ();
			}
			if (totalFpsAverage == null) {
				totalFpsAverage = new Average ();
				totalFpsAverage.Init ();

			}
			if (timeScaleAverage == null) {
				timeScaleAverage = new Average ();
				timeScaleAverage.Init ();

			}
			timeScaleAverage.Collect (Time.timeScale);
			timeScaleAverage.Collect (Time.timeScale);

			SimulationManager.Instance.RegisterFPSListener (OnFPSValue);
			//SimulationManager.Instance.RegisterFPSListener (OnFURateValue);
			SimulationManager.Instance.RegisterFURateListener (OnFURateValue);

		}

		public void OnFPSValue (float v)
		{


			windowFpsAverage.Collect (v);
			totalFpsAverage.Collect (v);




			if (windowFpsAverage.samples == windowSize) {
				if (applyControl) {
					ApplyFPSControl ();
				}
				//ApplyFURateControl ();
				++counter;
				if (counter == 24) {

					//Debug.Log (Time.time + ";timescale=" + Time.timeScale + ";" + GetInfoText ());
					counter = 0;
				}
				windowFpsAverage.ResetValues ();
			}

		}

		public void OnFURateValue (float v)
		{


			//Keep statistics
			windowFURateAverage.Collect (v);
			totalFURateAverage.Collect (v);
			//windowFpsAverage.Collect (SimulationManager.Instance.GetFPS ());
			//totalFpsAverage.Collect (SimulationManager.Instance.GetFPS ());


			


			if (windowFURateAverage.samples == windowSize) {
				if (applyControl) {
					ApplyFURateControl ();
				}
				++counter;
				if (counter == 24) {

					//Debug.Log (Time.time + ";timescale=" + Time.timeScale + ";" + GetInfoText ());
					counter = 0;
				}
				windowFURateAverage.ResetValues ();
			}

		}

		public void ApplyFPSControl ()
		{
			double avFPS = windowFpsAverage.Mean ();
			if (avFPS < decreaseFPSThreshold) {
				Time.timeScale = Time.timeScale / decreaseFactor;
			} else {
				
				Time.timeScale = Time.timeScale + increaseTerm;
				if (Time.timeScale > 1) {
					Time.timeScale = 1f;
				}
			}
			timeScaleAverage.Collect (Time.timeScale);
			if (listeners != null) {
				listeners ();
			}

			//Debug.Log ("avFPS=" + avFPS + " timeScale=" + Time.timeScale);
		}

		public void ApplyFURateControl ()
		{
			double avFURate = windowFURateAverage.Mean ();
			//double avFURate = windowFpsAverage.Mean ();
			//Debug.Log("avFURate="+avFURate+"th="+((1/Time.fixedDeltaTime)*Time.timeScale*fuRatePercentageThreshold));
			if (avFURate < (1 / Time.fixedDeltaTime) * Time.timeScale * fuRatePercentageThreshold) {
				
				if (Time.timeScale > 1) {
					Time.timeScale = Time.timeScale - 2f * increaseTerm;
				} else {
					Time.timeScale = Time.timeScale / decreaseFactor;
				}
			} else {

				Time.timeScale = Time.timeScale + increaseTerm;
				if (fasterThanRealTime == false) {
					if (Time.timeScale > 1) {
						Time.timeScale = 1f;
					}
				}
			}
			timeScaleAverage.Collect (Time.timeScale);
			if (listeners != null) {
				listeners ();
			}

			//Debug.Log ("avFPS=" + avFPS + " timeScale=" + Time.timeScale);
		}

		public string GetInfoText ()
		{
			return ":windowAvFU=" + windowFURateAverage.Mean () + ":totalAvFU=" + totalFURateAverage.Mean () + ":windowAvFPS=" + windowFpsAverage.Mean () + ":totalAvFPS=" + totalFpsAverage.Mean () + ":avTimescale=" + timeScaleAverage.Mean () + ":stdTimescale=" + timeScaleAverage.StdDev ();  
		}

	}
}
