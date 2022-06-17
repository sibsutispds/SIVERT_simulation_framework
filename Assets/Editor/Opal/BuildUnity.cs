/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class BuildUnity
{


	[MenuItem ("File/Build Opal")]
	public static void BuildGame ()
	{

		string level = EditorUtility.OpenFilePanelWithFilters ("Choose Scene to Build", "", new string[]{"Scenes","unity"});
		// Get filename. 

		string path = EditorUtility.SaveFilePanel ("Choose Name and Location of Built Game","../",System.IO.Path.GetFileNameWithoutExtension(level),"exe");

		List<string> levels = new List<string> ();
		levels.Add (level);
		// Build player.
		BuildPipeline.BuildPlayer (levels.ToArray(), path, BuildTarget.StandaloneWindows, BuildOptions.None);
		string root = System.IO.Path.GetDirectoryName (path);
		string targetPath = root + "/Assets/Plugins/Opal/opal/";
		Directory.CreateDirectory (targetPath);
		// Copy a file from the project folder to the build folder, alongside the built game.
	
		if (System.IO.Directory.Exists("./Assets/Plugins/Opal/opal/") ){
			string[] files = System.IO.Directory.GetFiles("./Assets/Plugins/Opal/opal/","*.cu");

			// Copy the files and overwrite destination files if they already exist.
			foreach (string s in files)
			{
				// Use static Path methods to extract only the file name from the path.
				string fileName = System.IO.Path.GetFileName(s);
				string destFile = System.IO.Path.Combine(targetPath, fileName);
				System.IO.File.Copy(s, destFile, true);
			}
			 files = System.IO.Directory.GetFiles("./Assets/Plugins/Opal/opal/","*.h");

				// Copy the files and overwrite destination files if they already exist.
				foreach (string s in files)
				{
					// Use static Path methods to extract only the file name from the path.
					string fileName = System.IO.Path.GetFileName(s);
					string destFile = System.IO.Path.Combine(targetPath, fileName);
					System.IO.File.Copy(s, destFile, true);
				}
		}

	}

}
