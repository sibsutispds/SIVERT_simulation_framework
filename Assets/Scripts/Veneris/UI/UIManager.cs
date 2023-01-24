/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Veneris
{
	
	public class UIManager : Singleton<UIManager>
	{

		public Text simTime=null;
		public Text timeScaleText=null;
		int vehicleLayer;
		//public Slider timeScale=null;
		//public InputField timeScaleInput=null;
		public Text activeVehicles = null;
		public VehicleManager vehicleManager=null;
		//public Toggle automaticTimeScaleControl = null;
		public Text generalTextInfo = null;
		public Text pauseText = null;
		public Button quitButton = null;
		public Button centerButton = null;
		public SimulationManager simManager=null;
		protected string timeScaleControlInfo =null;
		protected string fpsValue =null;
		protected string fuRateValue =null;
		// Use this for initialization
		void Start ()
		{
			if (simManager == null) {
				simManager = SimulationManager.Instance;
			}
			if (simTime == null) {
				simTime = transform.Find("SimulationTime").GetComponent<Text>();
			}

			if (generalTextInfo == null) {
				generalTextInfo = transform.Find("GeneralTextInfo").GetComponent<Text>();


			}

			if (quitButton == null) {
				quitButton = transform.Find("QuitButton").GetComponent<Button>();
				//Debug.Log ("Found quit buttom");

			}
			quitButton.onClick.AddListener (OnQuitClick);
			if (centerButton == null) {
				centerButton = transform.Find("CenterButton").GetComponent<Button>();
				//Debug.Log ("Found quit buttom");

			}
			centerButton.onClick.AddListener (OnCenterClick);
			if (pauseText == null) {
				pauseText = transform.Find("PauseText").GetComponent<Text>();


			}

			if (activeVehicles == null) {
				activeVehicles = transform.Find("ActiveVehicles").GetComponent<Text>();

				if (vehicleManager == null) {
					VehicleManager[] v=GameObject.FindObjectsOfType(typeof(VehicleManager)) as VehicleManager[];
					if (v.Length > 0) {
						vehicleManager = v [0];
						if (vehicleManager.activeVehicleDictionary != null) {
							activeVehicles.text = "Active vehicles: " + vehicleManager.activeVehicleDictionary.Count;
						} 
					}
				}

					
			}

			simManager.RegisterFPSListener (OnFPSValue);
			simManager.RegisterFURateListener (OnFURateValue);
			simManager.RegisterEndSimulationListener (OnEndSimulation);
			simManager.RegisterOnPauseListener (OnPause);

			vehicleLayer = 1<<LayerMask.NameToLayer ("Vehicle");
			OnPause (false);
		}
	
		// Update is called once per frame
		void Update ()
		{
			
			simTime.text = "Simulation time (s):  " + Time.time.ToString() + "Real time (s):"+Time.unscaledTime.ToString();
			if (vehicleManager.activeVehicleDictionary != null) {

				if (simManager.selectedVehicle != null) {
				
					activeVehicles.text = "Active vehicles: " + vehicleManager.activeVehicleDictionary.Count.ToString () + "  Selected vehicle: " + SimulationManager.Instance.selectedVehicle.GetComponent<VehicleInfo> ().vehicleId.ToString ();
				} else {
					activeVehicles.text = "Active vehicles: " + vehicleManager.activeVehicleDictionary.Count.ToString ();
				}
			} else {
				activeVehicles.text = "Active vehicles: 0";
			}
			/*if (Input.GetMouseButtonDown (0)) {
				RaycastHit hitInfo;
				bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity, vehicleLayer);
				if (hit) {
					Debug.Log ("hit vehicle=" + hitInfo.transform.name);
					SimulationManager.Instance.LoggedVehicleId = hitInfo.transform.GetComponent<VehicleInfo> ().vehicleId;
				} else {
					SimulationManager.Instance.LoggedVehicleId = -1;
				}
			}*/
		

		}
		void OnPause(bool paused) {
			
			pauseText.gameObject.SetActive (paused);

		}
		void OnQuitClick() {
			//Force application quit
			simManager.QuitSimulation ();
			#if UNITY_EDITOR 
			UnityEditor.EditorApplication.isPaused=true;
			#else
			Debug.Log("Quit clicked");
			Application.Quit();
			#endif

		}
		void OnCenterClick() {
			Debug.Log ("Center clicked");
			simManager.GetCameraManager().CenterCamera ();

		}
		void OnEndSimulation() {
			if (generalTextInfo != null) {
				generalTextInfo.text
			= "ENDSIMULATION. SimulationTime/RealTime=" + (Time.time / Time.realtimeSinceStartup) + "; Max Active Vehicles =" + vehicleManager.maxActiveVehicles;
			}
		}

		void OnFPSValue(float v) {
			fpsValue = v.ToString ();
			generalTextInfo.text = "FPS: " + fpsValue +"FU Rate="+fuRateValue;
		}
		void OnFURateValue(float v) {
			fuRateValue = v.ToString ();
			generalTextInfo.text = "FPS: " + fpsValue +"FU Rate="+fuRateValue;
		}
		public override void OnDestroy() {
			if (simManager != null) {
				simManager.RemoveFPSListener (OnFPSValue);
				simManager.RemoveFURateListener (OnFURateValue);
				simManager.RemoveEndSimulationListener (OnEndSimulation);
				simManager.RemoveOnPauseListener (OnPause);
			}
			base.OnDestroy ();
		}
	
	}
}
