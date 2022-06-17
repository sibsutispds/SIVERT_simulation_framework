/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Veneris {
public class CustomExportPackage  {

		[MenuItem ("Veneris/Custom Veneris Export")]
		static void export()
		{
			AssetDatabase.ExportPackage (AssetDatabase.GetAllAssetPaths(), "Veneris.unitypackage",ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies | ExportPackageOptions.IncludeLibraryAssets);
		}
	}
}
