/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections;
using System.IO;

namespace Veneris
{
	public class FileLogger : MonoBehaviour
	{
		protected FileStream m_FileStream = null;
		protected StreamWriter m_StreamWriter = null;
		public string dirPath ;
		public string filePath;
		public string fileName;
		public int id;
		public bool log = true;
		public bool append = false;

		void Start ()
		{
			if (m_FileStream == null) {
				SetPaths ();
			}
		}
		public void SetPaths() {
			if (log) {
				if (string.IsNullOrEmpty(dirPath)) {
					dirPath = SimulationManager.Instance.outputPath;
				}
				filePath = dirPath +System.IO.Path.DirectorySeparatorChar+ fileName + "-" + id + ".txt";
				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
				if (append) {
					m_FileStream = new FileStream (filePath, FileMode.Append, FileAccess.ReadWrite);

				} else {
					m_FileStream = new FileStream (filePath, FileMode.Create, FileAccess.ReadWrite);
				}
				m_StreamWriter = new StreamWriter (m_FileStream);
			}
			
		}

		public void RecordWithTimestamp<T> (T t)
		{
			Record (Time.time + "\t" + t.ToString ());

		}

		public void Record (string r)
		{
			m_StreamWriter.WriteLine (r);

		}

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
