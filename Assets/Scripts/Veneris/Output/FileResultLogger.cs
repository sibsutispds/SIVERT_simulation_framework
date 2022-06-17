/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

namespace Veneris
{
	public class FileResultLogger : IResultLogger
	{
		protected FileStream m_FileStream = null;
		protected StreamWriter m_StreamWriter = null;
		public string dirPath ;
		public string filePath;
		public string fileName;
		public int id;
		public bool log = true;
		public bool append = false;

		public FileResultLogger(string dir,int id, string name, bool log, bool append) {
			dirPath = dir;
			this.id = id;
			this.fileName = name;
			this.log = log;
			this.append = append;
			filePath = dirPath +System.IO.Path.DirectorySeparatorChar+ fileName + "-" + id + ".txt";
		}
		public FileResultLogger(string dir, string name, bool log, bool append) {
			dirPath = dir;
		
			this.fileName = name;
			this.log = log;
			this.append = append;
			filePath = dirPath +System.IO.Path.DirectorySeparatorChar+ fileName + ".txt";
		}
		public void CreateStream() {
			if (log) {


				Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
				if (append) {
					m_FileStream = new FileStream (filePath, FileMode.Append, FileAccess.ReadWrite);

				} else {
					m_FileStream = new FileStream (filePath, FileMode.Create, FileAccess.ReadWrite);
				}
				m_StreamWriter = new StreamWriter (m_FileStream, System.Text.Encoding.ASCII);
			}

		}

		public void RecordWithTimestamp<T> (T t)
		{
			Record (Time.time + "\t" + t.ToString ());

		}
		public void RecordVariableWithTimestamp<T> (string name,T t) {
			Record (Time.time + "\t" + name+"\t"+t.ToString ());
		}
		public void RecordWithTimestamp<T> (T t, int id)
		{
			Record (id+"\t"+Time.time + "\t" + t.ToString ());

		}
		public void RecordVariableWithTimestamp<T> (string name,T t, int id) {
			Record (id+"\t"+Time.time + "\t" + name+"\t"+t.ToString ());
		}

		public void Record (string r)
		{
			m_StreamWriter.WriteLine (r);

		}


		public void Close ()
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
