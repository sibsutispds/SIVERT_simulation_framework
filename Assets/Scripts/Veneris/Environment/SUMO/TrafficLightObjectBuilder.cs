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
	public class TrafficLightObjectBuilder
	{

		public GameObject root = null;
		public GameObject post = null;

		// Use this for initialization
		public GameObject CreatePostFromPrefab (GameObject trafficLightPostPrefab )
		{

			root = new GameObject ("Traffic Light Post");
			post = Object.Instantiate (trafficLightPostPrefab);

			//post.parent=root.transform;

			
			post.transform.SetParent (root.transform, false);
			return root;
			
			//tl.transform.parent = root.transform;
	
		}

		public GameObject AddTrafficLightPrefab (GameObject trafficLightPrefab, float translation, float localscale=1f)
		{
			Transform bar = post.transform.Find ("Bar");
			GameObject tl = Object.Instantiate (trafficLightPrefab);
			tl.transform.position = bar.localPosition;
			//tl.transform.Translate(new Vector3(4f,0f,0.17f));
			tl.transform.Translate (Vector3.right*translation);
			tl.transform.Translate (Vector3.forward*0.17f);
			tl.transform.SetParent (root.transform, false);
			if (localscale != 1f) {
				tl.transform.localScale = new Vector3 (localscale, 1f, 1f);
			}
			return tl;
		}
		
	}
	
	

}
