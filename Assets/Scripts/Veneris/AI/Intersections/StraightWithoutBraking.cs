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
	public class StraightWithoutBraking : IntersectionBehaviour
	{

		public IBehaviourTreeNode StraightWithoutBrakingTree ()
		{
			BehaviourTreeBuilder builder = new BehaviourTreeBuilder ();
			return builder.
				Sequence ("straight without braking").
					//Do ("log",()=>{ailogic.Log(39,"straight without at "+intersection.name); return FluentBehaviourTree.BehaviourTreeStatus.Success;}).	
					Do ("set next path speed limit", () => SetNextPathSpeedLimit ()).
					Do ("Apply behaviour", () => SetApplyBehaviour ()).
					Do ("check straight-without-braking:change to default-behaviour", ()=> SetDefault ()).
				End ().
			Build ();
		}
		public override void Prepare ()
		{
			base.Prepare ();
			
			action = IntersectionAction.StraightWithoutBraking;
			behaviourName="StraightWithoutBraking at  "+intersection.name;
			//ailogic.Log ("Preparing "+behaviourName);
			mainBehaviour = StraightWithoutBrakingTree ();
			//Call at the end to let traffic light tracker work
			//base.Prepare ();
			SetApproachActionAndPriority();

		}


	}
}
