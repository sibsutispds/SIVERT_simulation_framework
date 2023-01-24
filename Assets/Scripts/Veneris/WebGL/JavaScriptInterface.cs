/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Veneris
{
	public class JavaScriptInterface 
	{
		[DllImport("__Internal")]
		public static extern string ReadKey(string key);

		[DllImport("__Internal")]
		public static extern void UrlLog(string url, string str);



		//Proxy functions call an external JS file (attached via a <script> tag in the HTML file, instead of the compiled in the Plugins. This way JS development is faster, since no 
		//recompiling of JS is necessary. I am not sure if this decreases performance
		[DllImport("__Internal")]
		public static extern string ReadKeyProxy(string key);

		[DllImport("__Internal")]
		//Should be done properly ...
		//1 for network
		//2 for routes
		//3 for polyogns 
		//4 for osm json
		public static extern string ReadJSONBuilderFile(int type);


		[DllImport("__Internal")]
		public static extern void UrlLogProxy(string url, string str);

		[DllImport("__Internal")]
		public static extern void ExecuteJS(string code);

	}
}
