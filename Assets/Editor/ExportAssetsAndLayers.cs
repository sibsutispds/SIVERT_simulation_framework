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
	public class ExportAssetsAndLayers
	{
		[MenuItem ("Assets/Export Veneris Complete")]
		public static void ExportAll ()
		{
			string[] projectContent = AssetDatabase.GetAllAssetPaths();  
			AssetDatabase.ExportPackage(projectContent, "VenerisComplete.unitypackage", ExportPackageOptions.Recurse |ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets );  
			Debug.Log("Project Exported"); 
		}
	}
}
