/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Veneris {
public class IntersectionPriorityInfo : MonoBehaviour {

		//private Dictionary<long, Transform> priorityCheckPosition = null;
		//Serialize and show it in inspector
		[SerializeField]
		private List<Transform> checkPositions=null;
		[SerializeField]
		private Transform internalStop = null;
		[SerializeField]
		private bool stopAtInternalPosition=false;
		/*public Dictionary<long, Transform> GetCheckPositionsDictionary() {
			return priorityCheckPosition;
		}
		public void AddCheckPosition(long id, Transform pos) {
			if (priorityCheckPosition == null) {
				priorityCheckPosition = new Dictionary<long, Transform> ();
			}
			priorityCheckPosition.Add (id, pos);
		}
	*/
		public void AddCheckPosition(Transform pos) {
			if (checkPositions == null) {
				checkPositions = new List<Transform> ();
			}
			checkPositions.Add (pos);
		}
		public List<Transform> GetCheckPositions() {
			return checkPositions;
		}
		public bool HasHigherPriorityLanes() {
			
			if (checkPositions != null && checkPositions.Count >0) {
				return true;
			} else {
				return false;
			}
		}

		public bool StopAtInternalPosition() {
			return stopAtInternalPosition;
		}
		public void SetInternalStop(Transform stop) {
			internalStop = stop;
			stopAtInternalPosition = true;
		}
		public Transform GetInternalStopPosition() {
			return internalStop;
		}

		void OnDrawGizmosSelected ()
		{
			if (checkPositions != null) {
				foreach (Transform t in checkPositions) {
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine (transform.position, t.position);
				}
			}

		}
}
}
