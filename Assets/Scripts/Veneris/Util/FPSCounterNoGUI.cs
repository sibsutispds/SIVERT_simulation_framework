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
	public class FPSCounterNoGUI : MonoBehaviour
	{
		public  float updateInterval = 1F;
		public  float fps=0;
		public float fpsFU=0;

		private int calledFU = 0;
		private float lastRealTime=0f;
		private float lastRealTimeFU=0f;
		private int lastFrameCount=0;
		private float fuTotal=0f;
		public delegate void FPSListener(float f);

		public FPSListener listeners=null;
		public FPSListener listenersFU=null;


		void OnEnable ()
		{
			fps = 0;
			fpsFU=0;
			calledFU = 0;
			lastRealTime = Time.realtimeSinceStartup;
			lastRealTimeFU = Time.realtimeSinceStartup;

			fuTotal = 0;

		}

		void Update ()
		{
			float rt=Time.realtimeSinceStartup;
			float deltaRT = rt- lastRealTime;
			// Interval ended - update GUI text and start new interval
			if ( deltaRT>= updateInterval) {
				
				fps = (Time.frameCount-lastFrameCount) / deltaRT;
				lastRealTime = rt;
				lastFrameCount = Time.frameCount;

				if (listeners != null) {
					listeners (fps);
				}
			}
		}
		void FixedUpdate() {
			float rt=Time.realtimeSinceStartup;
			float deltaRT = rt- lastRealTimeFU;
			++calledFU;
			fuTotal += Time.deltaTime;
			if (deltaRT >= updateInterval) {
				
				fpsFU = (calledFU / deltaRT);
				//Debug.Log ("fpsFU="+fpsFU+"calledFU=" + calledFU + "fuTotal" + fuTotal + "steps/s (simulated time)=" + (calledFU / fuTotal));
				lastRealTimeFU = rt;
				calledFU = 0;
				fuTotal = 0f;
				if (listenersFU != null) {
					listenersFU (fpsFU);
				}


			}
		}

	}
}
