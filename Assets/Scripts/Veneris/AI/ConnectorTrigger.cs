/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluentBehaviourTree;
namespace Veneris
{
	//Use to implement complex behaviour at ConnectorTriggers. Otherwise just use the default behaviour with HandleEnterVehicleTrigger
	public class ConnectorTrigger : AIBehaviour
	{
		public AILogic ailogic=null;
		public PathConnector connector =null;
		void Awake () {
			if (ailogic == null) {
				ailogic = GetComponent<AILogic> ();
			}
		}
		public void SetConnector(PathConnector c) {
			this.connector = c;
		}
		public override void Prepare ()
		{
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			//mainBehaviour = builder.Sequence ("stop-intersection").Do ("check-clearance", t=>this.CheckClearance ()).Do("set-default",t=>this.SetDefault()).End().Build ();
			Debug.Log("changing path at connector ");
			mainBehaviour = builder.Sequence ("select-next-path").Do("check-path",()=>SelectNextPath()).Do("set-default",()=>this.SetDefault()).End().Build ();


		}
		public FluentBehaviourTree.BehaviourTreeStatus SelectNextPath() {
			//Implement additional logic
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}
		public FluentBehaviourTree.BehaviourTreeStatus SetDefault() {
			ailogic.EndRunningBehaviour (this);
			Destroy (GetComponent<ConnectorTrigger> (),0.1f);
			return FluentBehaviourTree.BehaviourTreeStatus.Success;
		}

	}
}
