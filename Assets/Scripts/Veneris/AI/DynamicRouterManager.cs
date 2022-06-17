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
	public class DynamicRouterManager : AgentRouteManager
	{

		protected override void Awake ()
		{
			base.Awake ();
			routeIds.Clear ();
			changesList.Clear ();
		}
		public override Path GetLastPathInRoute ()
		{
			//We still do not know the last path, just get the first lane path
			return routeRoads[routeRoads.Count-1].lanes[0].GetComponent<Path>();
		}

	}
}
