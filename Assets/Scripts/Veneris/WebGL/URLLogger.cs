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
	public class URLLogger : IResultLogger
	{
		public string url;
		public bool disabled = false;
		public URLLogger(string url, bool disabled) {
			this.url = url;
			this.disabled = disabled;
		}
		public void RecordWithTimestamp<T> (T t)
		{
			if (disabled) {
				return;
			}
			Record (Time.time + "\t" + t.ToString ());

		}
		public void RecordVariableWithTimestamp<T> (string name,T t) {
			if (disabled) {
				return;
			}
			Record (Time.time + "\t" + name+"\t"+t.ToString ());
		}
		public void RecordWithTimestamp<T> (T t, int id)
		{
			if (disabled) {
				return;
			}
			Record (id+"\t"+Time.time + "\t" + t.ToString ());

		}
		public void RecordVariableWithTimestamp<T> (string name,T t, int id) {
			if (disabled) {
				return;
			}
			Record (id+"\t"+Time.time + "\t" + name+"\t"+t.ToString ());
		}

		public void Record (string r)
		{
			if (disabled) {
				return;
			}
			JavaScriptInterface.UrlLogProxy (url, r);

		}
		public void Close() {
		}
	}
}
