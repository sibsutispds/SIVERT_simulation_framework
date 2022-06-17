/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Veneris
{
	public class TriggerEventPublisher : MonoBehaviour
	{


		protected List<Action<Collider>> enterList;
		protected List<Action<Collider>> exitList;

		public delegate void OnEnter(Collider c);
		//public OnEnter onEnterListeners = null;

		void Awake ()
		{
			if (enterList == null) {
				enterList = new List<Action<Collider>> ();
			}

			if (exitList == null) {
				exitList = new List<Action<Collider>> ();
			}
		}

		public virtual void AddEnterListener (Action<Collider> action)
		{
			enterList.Add (action);
			//onEnterListeners +=action;
		}

		public virtual void RemoveEnterListener (Action<Collider> action)
		{
			
			//enterList.RemoveAll (action.Equals);
			//onEnterListeners -= action;
			//foreach (Action<Collider> a in enterList) 
			//Debug.Log("list count"+enterList.Count);
			for (int i = enterList.Count-1; i >=0 ; --i) {
				if (enterList [i] == action) {
					enterList.RemoveAt(i);
				}
				
			}
		}

		public virtual void AddExitListener (Action<Collider> action)
		{
			exitList.Add (action);
		}

		public virtual void RemoveExitListener (Action<Collider> action)
		{
			//exitList.RemoveAll (action.Equals);
			for (int i = exitList.Count-1; i >=0 ; --i) {
				if (exitList [i] == action) {
					exitList.RemoveAt(i);
				}

			}
			//foreach (Action<Collider> a in enterList) 
			//Debug.Log("list count"+enterList.Count);
		}

		protected  void TriggerEnterListeners (Collider other)
		{

			/*if (onEnterListeners != null) {
				onEnterListeners (other);
			}*/
			//Zero GC

			for (int i = 0; i < enterList.Count; i++) {
				
			
			//foreach (Action<Collider> a in enterList) {
				
				enterList[i](other);
//				Debug.Log ("calling delegate");
			}

		}

		protected virtual void TriggerExitListeners (Collider other)
		{
			for (int i = 0; i < exitList.Count; i++) {
			//foreach (Action<Collider> a in exitList) {
				exitList[i](other);
				//				Debug.Log ("calling delegate");
			}
		}

		void OnTriggerEnter (Collider other)
		{
			//foreach (Action<Collider> a in enterList) {
			//	a (other);
			//	Debug.Log ("calling delegate in TriggerEventPublisher");
			//}
			//Debug.Log("Calling OntriggerEnter for "+other.transform.root.name);
			TriggerEnterListeners (other);
		}

		void OnTriggerExit (Collider other)
		{
			//foreach (Action<Collider> a in exitList) {
			//	a (other);
			//	Debug.Log ("calling delegate in TriggerEventPublisher");
			//}

			TriggerExitListeners (other);
		}
		void OnMouseDown() {
			SimulationManager.Instance.MouseDownOnVehicle (transform.root);
		}
	}
}
