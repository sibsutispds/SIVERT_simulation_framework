/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
namespace Veneris
{

	public class ChangeStaticObjects 
	{

		[MenuItem ("Veneris/Unset static for static meshes")]
		static void UnsetStatic ()
		{
			Opal.StaticMesh[] staticMeshes = GameObject.FindObjectsOfType<Opal.StaticMesh> (); 

			for (int i = 0; i < staticMeshes.Length; i++) {

				//StaticEditorFlags f = GameObjectUtility.GetStaticEditorFlags (staticMeshes [i].gameObject);

				//f = f & ~(StaticEditorFlags.OffMeshLinkGeneration);
				StaticEditorFlags f =   StaticEditorFlags.ContributeGI |StaticEditorFlags.NavigationStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.ReflectionProbeStatic;
				GameObjectUtility.SetStaticEditorFlags (staticMeshes [i].gameObject, f);

		

			}
		}


	}
}