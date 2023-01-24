/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Veneris
{
	public class WebGLSimulationManager : SimulationManager
	{
	/*	protected URLLogger generalResultLoggerW = null;
		protected URLLogger speedLoggerW = null;
		protected URLLogger positionLoggerW = null;
		protected URLLogger accelerationLoggerW = null;
		protected URLLogger tripInfoLoggerW = null;
		protected URLLogger environmentLoggerW = null;
		protected URLLogger performanceLoggerW = null;
		*/

		//protected string RoutesUrl = "routes-url";


		public bool disableLoggers = true;
		protected override void Awake ()
		{	
			Debug.Log ("WebGL simulation manager Awake");
			#if (UNITY_WEBGL && !UNITY_EDITOR)
			//StartCoroutine (GetConfigurationFromUrl ());
			base.Awake();
			#else

			Debug.Log("Not WebGL build. Falling back to SimulationManager");
			base.Awake();
			#endif
		}

		IEnumerator GetConfigurationFromUrl ()
		{
			
			using (UnityWebRequest www = UnityWebRequest.Get ("veneris.ini")) {
				yield return www.Send ();

				if (www.isNetworkError || www.isHttpError) {
					Debug.Log (www.error);
				} else {
					// Show results as text
				
					Debug.Log ("Downloaded configuration file: "+www.downloadHandler.text);
					string[] lines = System.Text.RegularExpressions.Regex.Split(www.downloadHandler.text,"\r\n|\r|\n");
					int ln = 0;
					for (int i = 0; i < lines.Length; i++) {
						ReadConfigurationLine (lines [i], ln);
						ln++;
					}
					ReadKeys ();
				}
			}

		}
		protected override void CreateLoggers() {

			Debug.Log ("WebGL Create Loggers");
			generalResultLogger = new URLLogger ("log/output.php",disableLoggers);

			speedLogger = new URLLogger ("log/speed.php",disableLoggers);
			
			positionLogger = new URLLogger ("log/position.php",disableLoggers);
		
			accelerationLogger = new URLLogger ("log/acceleration.php",disableLoggers);
		
			tripInfoLogger = new URLLogger ("log/tripinfo.php",disableLoggers);
		
			environmentLogger =new URLLogger ("log/environment.php",disableLoggers);

			performanceLogger = new URLLogger ("log/performance.php",disableLoggers);
		

		}

		public override void CloseLoggers() {
	
		}

		public override void ReadConfiguration ()
		{
			//TODO: finish this
			#if (UNITY_WEBGL && !UNITY_EDITOR)
			Debug.Log ("WebGL reading configuration");
			Debug.Log (SimulationTimeKey);
			fileConfigurationDictionary = new Dictionary<string, string> ();
			string v = "";
			try {
				 v = JavaScriptInterface.ReadKeyProxy (SimulationTimeKey);
			} catch (System.Exception e) {
				Debug.Log ("Exception thrown reading configuration " + e.StackTrace);
				
			}

			Debug.Log (SimulationTimeKey + "=" + v);
			fileConfigurationDictionary.Add(SimulationTimeKey,v);
			ReadKeys ();
			#else 
			base.ReadConfiguration();
			#endif

		
		}


		public override IResultLogger GetResultLogger(int id, string name, bool log=true,bool append=false) {
			URLLogger l = new URLLogger ("log/general.php?+id="+id+"&name="+name, disableLoggers);

			return l;
		}
		public override IResultLogger GetResultLogger(ResultType type) {
			switch (type) {
			case (ResultType.Acceleration):
				return GetAccelerationResultLogger ();

			case (ResultType.Position):
				return GetPositionResultLogger ();

			case (ResultType.Speed):
				return GetSpeedResultLogger ();
			
			case (ResultType.TripInfo):
				return GetTripInfoResultLogger ();

			default:
				return null;

			}
		}

		public override void RecordVariableWithTimestamp<T> (string name, T t ) {
			generalResultLogger.RecordVariableWithTimestamp (name, t);
		}

		public override void EndSimulation ()
		{
			Debug.Log ("Ending simulation at t=" + Time.time);
			if (endSimulationListeners != null) {
				endSimulationListeners ();
			}

			FinishLog ();
			endSimulation = true;
			#if (UNITY_WEBGL && !UNITY_EDITOR)
			JavaScriptInterface.ExecuteJS ("window.parent.quitPlayer()");
			#else
			base.EndSimulation();
			#endif

			//Now call JS to update the statistics at the website
		}

	}
}
