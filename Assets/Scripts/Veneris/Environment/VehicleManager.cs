/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Veneris
{
	public class VehicleManager : MonoBehaviour
	{

		public Dictionary<long,Path> idToPathDictionary = null;
		public Dictionary<Path, VenerisLane> pathToLaneDictionary = null;
		public Dictionary<int, AILogic> activeVehicleDictionary = null;
		public int maxActiveVehicles = 0;
		public GameObject vehiclePrefab;


		public delegate void OnRemoveVehicle(VehicleInfo i);
		public OnRemoveVehicle removeListeners=null;
		public delegate void OnInsertVehicle(VehicleGenerationInfo info,int id);
		public OnInsertVehicle insertListeners = null;
		protected List<Action<VehicleGenerationInfo, int>> onInsertionList=null;


		public virtual string GetInfoText() {
			if (activeVehicleDictionary != null) {
				return ":Active=" + activeVehicleDictionary.Count + ":maxActive=" + maxActiveVehicles;
			} else {
				return "No active vehicles";
			}
			
		}


	
		public virtual void RemovedVehicle(VehicleInfo vid) {
		}

		public virtual void EndOfRouteReached(VehicleInfo vid) {
		}

		public virtual void RemoveAndReinsert(VehicleInfo info,List<VenerisRoad> routeRoads) {
		}
		public virtual AILogic IsVehicleActive(int vid) {
			return null;
		}
		public void AddRemoveListener(OnRemoveVehicle action)
		{
			removeListeners += action;
		}
		public  void RemoveRemoveListener (OnRemoveVehicle action)
		{
			removeListeners -=action;

		}
		protected void TriggerRemoveListeners(VehicleInfo i) {
			if (removeListeners != null) {
				removeListeners (i);
			}
		}
		public void AddInsertionListener(OnInsertVehicle action)
		{
			insertListeners += action;
			//if (onInsertionList == null) {
			//	onInsertionList = new List<Action<VehicleGenerationInfo, int>> ();
			//}

			//onInsertionList.Add (action);
		}
		public  void RemoveInsertionListener (OnInsertVehicle action)
		{
			insertListeners -= action;
			/*for (int i = onInsertionList.Count-1; i >=0; i--) {
				if (onInsertionList [i] == action) {
					onInsertionList.RemoveAt (i);
				}

			}
			*/
			//onInsertionList.RemoveAll (action.Equals);

		}
		protected void TriggerInsertionListeners(VehicleGenerationInfo info, int id) {
			if (insertListeners != null) {
				insertListeners (info, id);
				//for (int i = 0; i < onInsertionList.Count; i++) {
				//foreach (Action<VehicleGenerationInfo> a in onInsertionList) {
				//	onInsertionList[i] (info,id);

				//}
			}
		}
	}
}
