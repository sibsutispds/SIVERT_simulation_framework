/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
namespace Veneris
{
	public class SimulationManager : Singleton<SimulationManager>
	{

		public enum ResultType
		{
			Speed,
			Position,
			Acceleration,
			TripInfo,
			General,
		};
		protected SimulationManager () {
		}

		#region configuration keys
		protected string OutputPathKey="output-dir";
		protected string SimulationTimeKey="sim-time";
		protected string RoutesFileKey="routes-file";
		protected string NoVehicleManagerKey = "no-vehicle-manager";
		#endregion
		#region resultLoggers
		protected IResultLogger generalResultLogger = null;
		protected IResultLogger speedLogger = null;
		protected IResultLogger positionLogger = null;
		protected IResultLogger accelerationLogger = null;
		protected IResultLogger tripInfoLogger = null;
		protected IResultLogger environmentLogger = null;
		protected IResultLogger performanceLogger = null;

		#endregion
		public string configurationFile = "veneris.ini";
		public  string outputPath = "";
		public int LoggedVehicleId = -1;
		public float endSimulationTime=-1f;
		public bool takeSnapshots = false;
		public int snapShotFrequency=5; // multiplier of the logTime. That is, a snapshot is taken every logTime*snapShotFrequency

		public UIManager uiManager = null;
		public VehicleManager vehicleManager=null;
		public FPSCounterNoGUI fpsCounter = null;
		public AIMDTimeScaleControl timeScaleControl = null;
		protected CameraManager camManager = null;

		protected bool noVehicleManager=false;


		//public delegate void FPSValue (float v);
		//protected FPSValue fpsListeners = null;
		//protected FPSValue fURateListeners =null;
		public delegate void TimeScaleControlValue ();
		protected TimeScaleControlValue timeScaleControlListeners = null;
		public delegate void OnEndSimulation();
		protected OnEndSimulation endSimulationListeners = null;
		public delegate void OnMouseDownOnVehicle (Transform i);
		public OnMouseDownOnVehicle mouseDownOnVehicleListeners = null;
		public delegate void OnPause(bool pause);
		public OnPause pauseListeners = null;
		public Transform selectedVehicle=null;
		public bool endSimulation = false;
		public float logInterval = 20f;
		protected System.Random defaultRNG= null;
		protected string[] args=null;
		protected Coroutine logCoroutine;
		protected WaitForSeconds logTime = null;
		protected int logSnapshots = 0;
		protected Coroutine endSimulationCoroutine=null;
		protected Dictionary<string, string> fileConfigurationDictionary;
	
		protected SumoScenarioInfo scenarioInfo = null;
		public float cameraHeight = 70f;
		protected virtual void Awake() {
			
			ReadConfiguration ();
		
			CreateLoggers ();

			if (fpsCounter == null) {
				fpsCounter = GetComponent<FPSCounterNoGUI> ();
			
			}
			//fpsCounter.listeners += OnFPSValue;
			//fpsCounter.listenersFU += OnFURateValue;
			selectedVehicle = null;
			//ActivateTimeScaleControl ();
			uiManager = FindObjectOfType<UIManager> ();
			ToggleOnScreenUI ();
			defaultRNG = new System.Random ();
			logTime = new WaitForSeconds (logInterval);

	//Create builders if necessary
			if (!IsRouteBuilderCreated ()) {
				string value;
				if (fileConfigurationDictionary.TryGetValue (RoutesFileKey, out value)) {
					BuildVehicleManager (value);
				}
			}
			if (vehicleManager == null) {
				if (noVehicleManager==false) {
					VehicleManager[] v = GameObject.FindObjectsOfType (typeof(VehicleManager)) as VehicleManager[];
					if (v.Length == 0) {
						Debug.LogError ("Could not find a VehicleManager on the scene.");
						Application.Quit ();
					} else {
						vehicleManager = v [0];
					}
				}
			}
			// camManager = new CameraManager (vehicleManager,cameraHeight);
		}
		void Start() {
			
		
			args = System.Environment.GetCommandLineArgs ();
		
			if (timeScaleControl == null) {
				timeScaleControl = gameObject.GetComponent<AIMDTimeScaleControl> ();
			}
			logCoroutine = StartCoroutine (logState ());


		
			if (endSimulationTime >= 0) {
				endSimulationCoroutine = StartCoroutine (EndSimulationTimer ());
			}
			GenerationInfo[] gi=GameObject.FindObjectsOfType(typeof(GenerationInfo)) as GenerationInfo[];
			#if VENERIS_DEBUG
				Debug.Log("Veneris Debug build. Potentially heavy memory usage.");
				generalResultLogger.Record("Veneris Debug build. Potentially heavy memory usage");
			#endif
			for (int i = 0; i < gi.Length; i++) {
				generalResultLogger.Record (gi [i].GetGenerationInfo ());
			}
			// camManager.CenterCamera ();

		}

		public virtual bool IsRouteBuilderCreated() {
			SumoRouteBuilder rb=GameObject.FindObjectOfType<SumoRouteBuilder> ();
			if (rb == null) {
				return false;
			} else {
				return true;
			}
		}

		public  void BuildVehicleManager(string pathToRoutes) {
			SumoBuilder builder = GameObject.FindObjectOfType (typeof(SumoBuilder)) as SumoBuilder;
			if (builder != null) {
				builder.pathToRoutes = pathToRoutes;
				builder.BuildRoutes ();
			} else {
				Debug.LogError ("Could not find a SumoBuilder on the scene. VehicleManager not build");
				Application.Quit ();
			}
		}
	
		public void ReadConfigurationLine(string line, int lineNumber) {
			
			char[] separator = new char[]{ '=' };
			if (string.IsNullOrEmpty (line)) {
				return;
			}
			string[] tokens = line.Split (separator);
			if (tokens.Length != 2) {
				Debug.LogError ("Error in configuration file: \"" + line + "\" at line " + lineNumber);
			}
			fileConfigurationDictionary.Add (tokens [0], tokens [1]);
		}
		public void ReadKeys() {
			//Read keys
			string value;
			if (fileConfigurationDictionary.TryGetValue (OutputPathKey, out value)) {
				if (System.IO.Path.IsPathRooted (value)) {
					outputPath = value;
				} else {
					outputPath = Directory.GetCurrentDirectory () +"/"+ value;
				}
			}
			if (fileConfigurationDictionary.TryGetValue (SimulationTimeKey, out value)) {
				float t;
				if (float.TryParse (value, out t)) {
					endSimulationTime = t;
				}
			}
			if (fileConfigurationDictionary.TryGetValue (NoVehicleManagerKey, out value)) {
				
				bool b;
				if (bool.TryParse (value, out b)) {
					
					noVehicleManager = b;
				}
			}
		}
		public virtual void ReadConfiguration() {
			string line;
			int lineNumber = 0;
			fileConfigurationDictionary = new Dictionary<string, string> ();

			if (File.Exists (configurationFile)) {
				using (System.IO.StreamReader file = new System.IO.StreamReader (configurationFile)) {
					
					Debug.Log ("Reading configuration file " + Directory.GetCurrentDirectory () + System.IO.Path.DirectorySeparatorChar + configurationFile);
					while ((line = file.ReadLine ()) != null) {  
						ReadConfigurationLine (line, lineNumber);
						/*if (string.IsNullOrEmpty (line)) {
							continue;
						}
						string[] tokens = line.Split (separator);
						if (tokens.Length != 2) {
							Debug.LogError ("Error in configuration file: \"" + line + "\" at line " + lineNumber);
						}
						fileConfigurationDictionary.Add (tokens [0], tokens [1]);
						*/
						lineNumber++;
					}
					ReadKeys ();
				
				}
			} else {
				Debug.Log ("Configuration file not found: " + Directory.GetCurrentDirectory () + "/"+ configurationFile);
			}
			
		}
		public IEnumerator EndSimulationTimer() {
			
				yield return new WaitForSeconds (endSimulationTime);
				EndSimulation ();
				
		}

		public CameraManager GetCameraManager() {
			return camManager;
		}

		public string GetInfoText() {
			string m = Time.time.ToString ()+":RealTime="+Time.realtimeSinceStartup;
			if (timeScaleControl != null) {
				m += timeScaleControl.GetInfoText ();
			}
			if (vehicleManager != null) {
				m += vehicleManager.GetInfoText ();
			}
			return m;
		}

		public IEnumerator logState() {
			while (true) {
				//Do not log right now because it is called on Start() and some other modules Start function may have not been called yet
				yield return logTime;
				string m = GetInfoText ();
				Debug.Log (m);
				performanceLogger.Record (m);
				if (takeSnapshots) {
					if (logSnapshots % snapShotFrequency == 0) {
						ScreenCapture.CaptureScreenshot (outputPath + System.IO.Path.DirectorySeparatorChar + Time.time.ToString () + ".png");
					}
				}

			}

		}
		protected virtual void CreateLoggers() {
			
			if (string.IsNullOrEmpty(outputPath)) {
				outputPath=Directory.GetCurrentDirectory();
			}
			outputPath = outputPath + System.IO.Path.DirectorySeparatorChar + SceneManager.GetActiveScene().name;
			FileResultLogger grl= new FileResultLogger (outputPath, "output", true, false);
			grl.CreateStream ();
			generalResultLogger=grl;
			FileResultLogger sl = new FileResultLogger (outputPath, "speed", true, false);
			sl.CreateStream ();
			speedLogger = sl;
			FileResultLogger pl = new FileResultLogger (outputPath, "position", true, false);
			pl.CreateStream ();
			positionLogger = pl;
			FileResultLogger al = new FileResultLogger (outputPath, "acceleration", true, false);
			al.CreateStream ();
			accelerationLogger = al;
			FileResultLogger til = new FileResultLogger (outputPath, "tripInfo", true, false);
			til.CreateStream ();
			tripInfoLogger = til;
			FileResultLogger el = new FileResultLogger (outputPath, "environment", true, false);
			el.CreateStream ();
			environmentLogger = el;
			FileResultLogger pfl = new FileResultLogger (outputPath, "performance", true, false);
			pfl.CreateStream ();
			performanceLogger = pfl;

		}

		public virtual void CloseLoggers() {
			speedLogger.Close ();
			accelerationLogger.Close ();
			positionLogger.Close ();
			tripInfoLogger.Close ();
			environmentLogger.Close ();
			generalResultLogger.Close ();
			performanceLogger.Close ();
		}

		public virtual IResultLogger GetGeneralResultLogger(){
			return generalResultLogger;
		}
		public virtual IResultLogger GetSpeedResultLogger(){
			return speedLogger;
		}
		public virtual IResultLogger GetPositionResultLogger() {
			return positionLogger;
		}
		public virtual IResultLogger GetAccelerationResultLogger(){
			return accelerationLogger;
		}
		public virtual IResultLogger GetTripInfoResultLogger(){
			return tripInfoLogger;
		}
		public virtual IResultLogger GetResultLogger(int id, string name, bool log=true,bool append=false) {
			FileResultLogger l = new FileResultLogger (outputPath,id,name, log, append);
			l.CreateStream ();
			return l;
		}
		public virtual IResultLogger GetResultLogger(ResultType type) {
			switch (type) {
			case (ResultType.Acceleration):
				return GetAccelerationResultLogger ();

			case (ResultType.Position):
				return GetPositionResultLogger ();

			case (ResultType.Speed):
				return GetSpeedResultLogger ();

			case (ResultType.TripInfo):
				return GetTripInfoResultLogger ();
			case (ResultType.General):
				return GetGeneralResultLogger ();
			
			default:
				return null;

			}
		}

		public virtual void RecordVariableWithTimestamp<T> (string name, T t ) {
			generalResultLogger.RecordVariableWithTimestamp (name, t);
		}


		public System.Random GetRNG() {
			return defaultRNG;
		}

		public bool ToggleRendering() {
			return camManager.ToggleRendering ();

		}
		public bool ToggleOnScreenUI() {
			if (uiManager != null) {
				if (uiManager.gameObject.activeSelf) {
					uiManager.gameObject.SetActive (false);

					return false;
				} else {
					uiManager.gameObject.SetActive (true);

					return true;
				}
			}
			return false;
		}
		public float GetFPS() {
			return fpsCounter.fps;
		}
		public float GetFPSInterval() {
			return fpsCounter.updateInterval;
		}
	
	
		public void QuitSimulation() {
			if (!endSimulation) {
				EndSimulation ();
			} else {
				Debug.Log ("QuitSimulation(): EndSimulation not called because simulation already ended");
			}
		}

		public void FinishLog() {


			//Collect stats
			VenerisLane[] v=GameObject.FindObjectsOfType(typeof(VenerisLane)) as VenerisLane[];
			environmentLogger.Record ("%lane\tavNumberOfVehicles\tvarNumberOfVehicles\tavDensity\tvarDensity");
			//Debug.Log ("v.l=" + v.Length);
			for (int i = 0; i < v.Length; i++) {
				if (v [i].IsInternal ()) {
					continue;
				}
				v [i].CollectStats ();
				//environmentLogger.Record ("lane=" + v [i].sumoId + ":avNumberOfVehicles=" + v [i].averageNumberOfVehicles.ComputeWeightedAverage()+":varNumberOfVehicles="+v[i].averageNumberOfVehicles.Variance()+ ":avDensity=" + v [i].averageDensity.ComputeWeightedAverage()+":varDensity="+v[i].averageDensity.Variance());
				environmentLogger.Record (v [i].sumoId + "\t" + v [i].averageNumberOfVehicles.ComputeWeightedAverage()+"\t"+v[i].averageNumberOfVehicles.Variance()+ "\t" + v [i].averageOccupancy.ComputeWeightedAverage()+"\t"+v[i].averageOccupancy.Variance());
				//Debug.Log (v [i].sumoId + "\t" + v [i].averageNumberOfVehicles.ComputeWeightedAverage()+"\t"+v[i].averageNumberOfVehicles.Variance()+ "\t" + v [i].averageDensity.ComputeWeightedAverage()+"\t"+v[i].averageDensity.Variance());
			}
			performanceLogger.Record (GetInfoText ());
			generalResultLogger.Record("SimulationManager::ENDSIMULATION. Time="+Time.time+";SimulationTime/RealTime=" + (Time.time / Time.realtimeSinceStartup));

		}
		public void Pause() {
			Debug.Log ("Pause called");
				Time.timeScale = 0;
				if (pauseListeners != null) {
					pauseListeners (true);
				}

		}
		public void UnPause() {
			Time.timeScale = 1;
			if (pauseListeners != null) {
				pauseListeners (false);
			}
		}
		public virtual void EndSimulation() {
			Debug.Log ("Ending simulation at t=" + Time.time);
			if (endSimulationListeners != null) {
				endSimulationListeners ();
			}

			FinishLog ();
			endSimulation = true;
			#if UNITY_EDITOR 
			UnityEditor.EditorApplication.isPaused=true;

			#else
			if (args!=null) {
				for (int i = 0; i < args.Length; i++) {
					if (args[i].Contains("-batchmode")) {
						Application.Quit();
						return;
					}
				}
			}

				//Just pause the application
				Time.timeScale=0;
				//Application.Quit();

			#endif

		}
		public override void OnDestroy() {
			if (!endSimulation) {
				EndSimulation ();
			}
			CloseLoggers ();
			base.OnDestroy ();
		}
		public bool IsTimeScaleControlActive() {
			if (timeScaleControl == null) {
				return false;
			} else {
				return timeScaleControl.applyControl;

			}
		}
		public void ActivateTimeScaleControl() {
			if (timeScaleControl == null) {
				timeScaleControl = gameObject.GetComponent<AIMDTimeScaleControl> ();
				timeScaleControl.listeners+=OnTimeScaleControl;
			}
			timeScaleControl.applyControl = true;

		}
		public void DeactivateTimeScaleControl() {
			if (timeScaleControl != null) {
				timeScaleControl.applyControl = false;
			}
		}
		/*public void OnFPSValue(float v) {
			if (fpsListeners != null) {
				fpsListeners (v);
			}
		}
		public void OnFURateValue(float v) {
			if (fURateListeners != null) {
				fURateListeners (v);
			}
		}*/
		public void OnTimeScaleControl() {
			if (timeScaleControlListeners != null) {
				timeScaleControlListeners ();
			}
		}
		public void RegisterFPSListener(FPSCounterNoGUI.FPSListener l) {
			//fpsListeners += l;
			fpsCounter.listeners += l;
		}
		public void RemoveFPSListener(FPSCounterNoGUI.FPSListener l) {
			fpsCounter.listeners -= l;
		}

		public void RegisterFURateListener(FPSCounterNoGUI.FPSListener l) {
			fpsCounter.listenersFU += l;
		}
		public void RemoveFURateListener(FPSCounterNoGUI.FPSListener l) {
			fpsCounter.listenersFU -= l;
		}
		public void RegisterTimeScaleControlListener(TimeScaleControlValue l) {
			timeScaleControlListeners += l;
		}
		public void RemoveTimeScaleControlListener(TimeScaleControlValue l) 
		{
			timeScaleControlListeners -= l;
		}
		public void RegisterEndSimulationListener(OnEndSimulation l) {
			endSimulationListeners += l;
		}
		public void RemoveEndSimulationListener(OnEndSimulation l) 
		{
			endSimulationListeners -= l;
		}
		public void RegisterOnMouseDownOnVehicleListener(OnMouseDownOnVehicle l) {
			mouseDownOnVehicleListeners += l;
		}
		public void RemoveEndSimulationListener(OnMouseDownOnVehicle l) 
		{
			mouseDownOnVehicleListeners -= l;
		}

		public void RegisterOnPauseListener(OnPause l) {
			pauseListeners += l;
		}
		public void RemoveOnPauseListener(OnPause l) 
		{
			pauseListeners -= l;
		}

		public string GetTimeScaleControlInfoText() {
			return timeScaleControl.GetInfoText ();
		}
		public void MouseDownOnVehicle(Transform t) {
			if (mouseDownOnVehicleListeners != null) {
				mouseDownOnVehicleListeners (t);
			}
			if (t != selectedVehicle) {
				selectedVehicle = t;
			}
		}

	}
}
