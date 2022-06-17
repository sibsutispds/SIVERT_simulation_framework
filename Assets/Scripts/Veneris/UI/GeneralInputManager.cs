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
	public class GeneralInputManager : MonoBehaviour
	{
		public SimulationManager simManager = null;
		public CameraManager camManager=null;
		// Use this for initialization
		void Start ()
		{
			if (simManager == null) {
				simManager = SimulationManager.Instance;
			}
			simManager.RegisterOnMouseDownOnVehicleListener (OnMouseDownOnVehicle);
			if (camManager == null) {
				camManager = simManager.GetCameraManager ();
			}
		
		}
	
		// Update is called once per frame
		void Update ()
		{
			if (Input.GetKeyDown (KeyCode.P)) {
				if (simManager.selectedVehicle != null) {
					ToggleDisplayRouteOnSelected ();
					
				}
			}
			if (Input.GetKeyDown (KeyCode.L)) {
				if (simManager.selectedVehicle != null) {
					ShowLogOnSelected ();

				}
			}
			if (Input.GetKeyDown (KeyCode.T)) {
				if (simManager.selectedVehicle != null) {
					ApplyThrottleOnSelected ();

				}
			}
			if (Input.GetKeyDown (KeyCode.I)) {
				simManager.ToggleOnScreenUI ();
			}
			if (Input.GetKeyDown (KeyCode.R)) {
				//Disable rendering
				simManager.ToggleRendering();
			}
			if (Input.GetKeyDown (KeyCode.Q)) {
				simManager.QuitSimulation ();
			}
			if (Input.GetKeyDown (KeyCode.Space)) {
				simManager.Pause ();
			}
			if (Input.GetKeyDown (KeyCode.G)) {
				simManager.UnPause ();
			}
			if (Input.GetKeyDown (KeyCode.F)) {
				if (!camManager.FollowNextVehicle()) {
					camManager.DisableFollowCamera ();
				}
			}
			if (Input.GetKey (KeyCode.F) && Input.GetKey (KeyCode.LeftControl)) {
				camManager.DisableFollowCamera ();
			}
		
		}
		public void ShowLogOnSelected() {
			VehicleInfo i = simManager.selectedVehicle.GetComponent<VehicleInfo> ();
			if (i != null) {
				Debug.Log ("Vehicle " + i.vehicleId + "show log called");
				i.aiLogic.GetComponent<MOBILIDMPathTracker> ().showLog = true;
				i.aiLogic.showLog = true;
			}
		}
		public void ApplyThrottleOnSelected() {
			VehicleInfo i = simManager.selectedVehicle.GetComponent<VehicleInfo> ();
			if (i != null) {
				Debug.Log ("Vehicle " + i.vehicleId + "apply throttle called");
				i.aiLogic.GetComponent<MOBILIDMPathTracker> ().Throttle (1f);

			}
		}
		public void ToggleDisplayRouteOnSelected() {
			VehicleInfo i = simManager.selectedVehicle.GetComponent<VehicleInfo> ();
			if (i != null) {
				i.aiLogic.routeManager.ToggleDisplayRoute ();
			}
		}
		public void OnMouseDownOnVehicle(Transform t) {
			if (simManager.selectedVehicle != null) {
				if (t != simManager.selectedVehicle) {
					VehicleInfo i = simManager.selectedVehicle.GetComponent<VehicleInfo> ();
					if (i != null) {
						if (i.aiLogic.routeManager.IsDisplayingRoute ()) {
							i.aiLogic.routeManager.HideRoute ();
						}
					}
				}
			}
		}
	}
}
