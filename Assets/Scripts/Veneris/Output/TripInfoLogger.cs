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
	public class TripInfoLogger: MonoBehaviour 
	{
		
		public string variableName="tripInfo";
		public AILogic ailogic=null;
		public VehicleManager vm=null;
		// Use this for initialization
		protected IResultLogger resultLogger = null;

		public float departTime=0f;
		public float realDepartTime = 0f;
		public string departLaneSumoId = null;
		public float timeLoss=0f; //Time lost due to driving below the ideal speed according to SUMO
		public bool recorded=false;

		void Awake() 
		{
			if (ailogic == null) {
				ailogic = transform.root.GetComponentInChildren<AILogic>();
				vm = ailogic.vehicleManager;
				ailogic.vehicleManager.AddInsertionListener (HandleInsertionTrigger);
				ailogic.vehicleManager.AddRemoveListener (HandleRemoveTrigger);
			}
			//resultLogger = SimulationManager.Instance.GetResultLogger (ailogic.vehicleInfo.vehicleId, variableName);
			resultLogger = SimulationManager.Instance.GetResultLogger (SimulationManager.ResultType.TripInfo);
		}
		/*void Start() {
			ailogic.vehicleManager.AddInsertionListener (HandleInsertionTrigger);
			ailogic.vehicleManager.AddRemoveListener (HandleRemoveTrigger);
			//ailogic.vehicleManager.AddDestroyListener (HandleDestroyTrigger);

			
		}*/
		void Update() {
			//Try to compute loss time
			if (ailogic != null) {
				if (ailogic.currentLane != null) {
					if (ailogic.vehicleInfo.speed < ailogic.currentLane.speed) {
						timeLoss += Time.deltaTime;
					}
				}
			}
		}

		protected void HandleInsertionTrigger(VehicleGenerationInfo info, int id) {
			
			if (id == ailogic.vehicleInfo.vehicleId) {
				
				realDepartTime = Time.time;
				departLaneSumoId = info.departLane;
				departTime = info.departTime;
			}
		}
		protected void HandleRemoveTrigger(VehicleInfo v) {
			if (v == ailogic.vehicleInfo) {
				LogResults (true);

			}
		}
		/*protected void HandleDestroyTrigger (int id) {
			if (id == ailogic.vehicleInfo.vehicleId) {
				//Now write results
				LogResults ();
				resultLogger.Close ();
				recordOnQuit = true;
			}

		}*/
		void OnApplicationQuit() {
			LogResults (false);

		}
		void OnDestroy ()
		{
			if (!recorded) {
				LogResults (false);
				//resultLogger.Close ();
			}
			if (vm != null) {
				vm.RemoveRemoveListener (HandleRemoveTrigger);
				vm.RemoveInsertionListener (HandleInsertionTrigger);
			}
			/*if (ailogic != null) {
				if (ailogic.vehicleManager != null) {
					ailogic.vehicleManager.RemoveDestroyListener (HandleDestroyTrigger);
				}
			}*/
		}
		protected void LogResults(bool arrived) {
			string result = "";
			result += ailogic.vehicleInfo.vehicleId + "\t";
			result += realDepartTime + "\t";
			//result += departLaneSumoId + "\t";
			result += (realDepartTime - departTime).ToString () + "\t"; //DepartDelay
			if (arrived) {
				result += (Time.time).ToString () + "\t"; //Arrival time
			} else {
				result += "-1\t";
			}
			result += ailogic.currentLane.sumoId +"\t"; //Arrival lane
			result += ailogic.vehicleInfo.speed.ToString() +"\t"; //Arrival speed
			result += (Time.time -realDepartTime).ToString() +"\t"; //duration
			result += ailogic.vehicleInfo.totalDistanceTraveled.ToString() +"\t"; //routeLength
			result += timeLoss.ToString() + "\t";


			resultLogger.Record(result);
			recorded = true;

		}
	}
}
