/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FluentBehaviourTree;
using System;
namespace Veneris {
	public class AIBehaviour : MonoBehaviour, IComparable {
		public IBehaviourTreeNode mainBehaviour=null;
		public enum PriorityType
		{
			DistanceInRoute,
		};
		public PriorityType priorityType;
		public float defaultPriority=400f;
		public float priorityValue;
		public Priority GetPriority;
		public bool destroy=false;
		public  delegate float  Priority();
		public delegate void OnSelfFinish();
		public OnSelfFinish onSelfFinishListeners = null;
		/*#if UNITY_EDITOR
		public string currentNode = null;
		#endif
		*/

		public string behaviourName;
		public bool running = false;

		public virtual void Prepare() {
			GetPriority = delegate() {
				return defaultPriority;	
			};

		}

		public virtual void MarkToDestroy() {
			destroy = true;
		}
		public bool CanBeDestroyed() {
			return destroy;
		}

		public virtual void FinalizeBehaviour() {
		}

		public virtual void ActivateBehaviour() {
			
			running = true;
			//Update lane, just in case

		}
		public virtual void DeactivateBehaviour() {
			running = false;
		}
		public void Run() {
			if (mainBehaviour == null) {
				return;
			} else {
				
				mainBehaviour.Tick ();
				/*#if UNITY_EDITOR
				mainBehaviour.Tick ( ref currentNode);
				#endif*/

			}
		}
		public void Run(List<String>  logSteps) {
			if (mainBehaviour == null) {
				return;
			} else {
				logSteps.Clear ();
				mainBehaviour.Tick (logSteps);
				/*#if UNITY_EDITOR
				mainBehaviour.Tick ( ref currentNode);
				#endif*/

			}
		}

		public void Run(int id) {
			if (mainBehaviour == null) {
				return;
			} else {

				//For Debug 
				mainBehaviour.Tick (null,id.ToString());
				//mainBehaviour.Tick ();
			}
		}
		public int CompareTo(object other) {
			return CompareDistanceInRouteTo ((AIBehaviour) other);
		}

		public virtual int CompareDistanceInRouteTo(AIBehaviour other) {
			
			return GetPriority ().CompareTo (other.GetPriority ());
		}
		public virtual void SelfFinished() {
			if (onSelfFinishListeners != null) {
				onSelfFinishListeners ();
			}
		}
	}
}
