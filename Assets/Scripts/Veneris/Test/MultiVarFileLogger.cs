/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.IO;

namespace Veneris.Vehicle
{
	public class MultiVarFileLogger : MonoBehaviour
	{
		private FileStream m_FileStream = null;
		private StreamWriter m_StreamWriter = null;
		public string dirPath = "C:/temp/";
		public string filePath;
		public string fileName = "default";
		public int id;
		public bool log = true;
		public bool append = false;


		public string line; 

		protected virtual void Start ()
		{
			if (log) {
				filePath = dirPath + fileName + "-" + id + ".txt";
				if (append) {
					m_FileStream = new FileStream (filePath, FileMode.Append, FileAccess.ReadWrite);

				} else {
					m_FileStream = new FileStream (filePath, FileMode.Create, FileAccess.ReadWrite);
				}
				m_StreamWriter = new StreamWriter (m_FileStream);
			}
			line = "";

		}
		public virtual void RecordHeaders(){

		}

		public void AddValue<T> (T t){
			line = line + t.ToString() + "\t";
		}

		public void RecordAdded(){
			m_StreamWriter.WriteLine (line);
			line = "";
		}

		//		public void RecordWithTimestamp<T> (T t)
		//		{
		//			Record (Time.time + "\t" + t.ToString ());
		//
		//		}
		//
		//		public void Record (string r)
		//		{
		//			m_StreamWriter.WriteLine (r);
		//
		//		}

		void OnDestroy ()
		{
			if (m_StreamWriter != null) {
				m_StreamWriter.Flush ();
				m_StreamWriter.Close ();
			}
			if (m_FileStream != null) {
				m_FileStream.Close ();
			}
		}
	}
}
