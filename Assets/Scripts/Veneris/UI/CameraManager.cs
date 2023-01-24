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
	public class CameraManager
	{

		public Camera mainCamera = null;
		public Camera followCamera = null;
		// protected UnityStandardAssets.Cameras.FreeLookCam followCamScript;
		public VehicleManager vehicleManager = null;
		public int currentPlayer = -1;
		public SumoScenarioInfo scenarioInfo=null;
		public float cameraHeight;

		public CameraManager (VehicleManager m, float height)
		{

			if (mainCamera == null) {
				mainCamera = Camera.main;
			}

			GameObject fcam = GameObject.FindWithTag ("FollowCamera");
			if (fcam != null) {
				followCamera = fcam.GetComponentInChildren<Camera> ();
				// followCamScript = fcam.GetComponent<UnityStandardAssets.Cameras.FreeLookCam> ();
				followCamera.enabled = false;
				fcam.transform.root.position = mainCamera.transform.position;
			
			
			}
			vehicleManager = m;
			cameraHeight = height;
		}

		public void EnableFollowCamera ()
		{

			if (followCamera != null) {
				if (mainCamera != null) {
					//mainCamera.gameObject.SetActive(false);
					mainCamera.enabled=false;
				

				}
				//followCamera.gameObject.SetActive(true);
				followCamera.enabled = true;
			}
		}

		public void DisableFollowCamera ()
		{
			if (mainCamera != null) {
				
				if (followCamera != null) {
					//followCamera.gameObject.SetActive(false);
					followCamera.enabled = false;
				}
				//mainCamera.gameObject.SetActive(true);
				mainCamera.enabled=true;
			}
		}

		public bool FollowVehicle (int id)
		{
			if (vehicleManager != null) {
				if (vehicleManager.activeVehicleDictionary.ContainsKey (id)) {
					AILogic ai = vehicleManager.activeVehicleDictionary [id];
					if (followCamera != null) {
						// followCamScript.SetTarget (ai.transform.root);
						currentPlayer = id;
						EnableFollowCamera ();
						return true;
					} else {
						return false;
					}
				}



			}
			return false;
		}

		public bool FollowNextVehicle ()
		{
			int next = currentPlayer + 1;
			if (vehicleManager != null) {
				if (vehicleManager.activeVehicleDictionary.Count == 0) {
					return false;
				}
				if (next <= 0 || next >= vehicleManager.activeVehicleDictionary.Count) {
					next = 0;
				}
				return FollowVehicle (next);
			}
			return false;
		}

		public void CenterCamera ()
		{
			//Put camera in the center of the scenario

			if (scenarioInfo == null) {
				SumoScenarioInfo i = GameObject.FindObjectOfType<SumoScenarioInfo> ();
				if (i != null) {
					scenarioInfo = i;
				}
			}
			if (mainCamera != null && scenarioInfo != null) {
				//	GameObject go = new GameObject ("Cam Target");
				Vector3 cp = scenarioInfo.GetCenter ();

				//go.transform.position = new Vector3(cp.x,cameraHeight,cp.z) ;

				mainCamera.GetComponent<OrbitPanZoomCamera> ().target.position = new Vector3 (cp.x, cameraHeight, cp.z);
				mainCamera.transform.rotation = Quaternion.LookRotation (Vector3.down, Vector3.right);

				//Debug.Log ("Centering camera");
				//cam.transform.LookAt (networkBuilder.floor.GetComponent<SumoScenarioInfo>().GetCenter());
			}
			if (followCamera != null) {
				followCamera.transform.position = mainCamera.transform.position;
			}
			DisableFollowCamera ();
		}
		public bool ToggleRendering() {
			if (mainCamera.gameObject.activeSelf) {
				mainCamera.gameObject.SetActive (false);
				return false;
			} else {
				mainCamera.gameObject.SetActive (true);
				return true;
			}

		}
	}
}
