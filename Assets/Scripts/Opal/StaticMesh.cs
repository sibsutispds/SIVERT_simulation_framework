/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Opal
{
	public class StaticMesh : MonoBehaviour
	{
		public OpalMeshProperties opalMeshProperties;
		public MeshFilter meshFilter;
		// Use this for initialization
		void Awake ()
		{
			meshFilter = GetComponent<MeshFilter> ();
			OpalMeshProperties op = GetComponent<OpalMeshProperties> ();
			if (op != null) {
				opalMeshProperties = op;
			}
		}
		public OpalMeshProperties GetOpalMeshProperties() {
			return opalMeshProperties;
		}

	}
}
