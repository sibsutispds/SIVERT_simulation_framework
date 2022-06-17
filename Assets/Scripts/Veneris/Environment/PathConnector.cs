/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

//using UnityEditor;

namespace Veneris
{

	//We need to do all of this in order to have it serialized in the editor, when we want to edit the network in the editor.
	//With a simple struc and a Dictionary is enough if the network is loaded at runtime


	//Dictionary version
	 
	/*
	[System.Serializable]
	public class ConnectionInfo
	{
		public enum ConnectionDirection
		{
			Straight,
			Turn,
			Left,
			Right,
			PartiallyLeft,
			PartiallyRight,
			Invalid}

		;
		public Path path;
		public ConnectionDirection type;

	}
	[System.Serializable]
	public class PathConnectorData   {

	
		public Dictionary<long,List<ConnectionInfo>> conn ;
		public  PathConnectorData() {
			conn = new Dictionary<long, List<ConnectionInfo>> ();

		}


		public void AddPathConnection (long pathId, Path connectedPath, ConnectionInfo.ConnectionDirection d) {
			if (!conn.ContainsKey (pathId)) {

				conn [pathId] = new List<ConnectionInfo> ();

			}
			ConnectionInfo info = new ConnectionInfo ();
			info.path = connectedPath;
			info.type = d;
			Debug.Log ("Adding path conn " + info.path.pathId + " to " + pathId);
			conn [pathId].Add (info);

		}
		
		

	} 

	public class PathConnector : MonoBehaviour
	{

		



		//public Dictionary<long,List<ConnectionInfo>> connections = null;

		// Use this for initialization


		void Start ()
		{
			Debug.Log ("Start path connector " + name);
			Debug.Log (connections);
			Debug.Log (connections.conn);
			Debug.Log (connections.conn.Keys.Count);
	
			foreach (long id in connections.conn.Keys) {
				Debug.Log ("key =" + id);
				foreach (ConnectionInfo c in connections.conn[id]) {
					Debug.Log ("conn to " + c.path.pathId);
				}
			}
		}

		public List<ConnectionInfo> GetPathsConnectedTo (long pathId)
		{
			Debug.Log ("Conn " + name);
			Debug.Log (connections);
			Debug.Log (connections.conn);
			Debug.Log (connections.conn.Keys.Count);
			Debug.Log (pathId);
			foreach (long id in connections.conn.Keys) {
				Debug.Log ("key =" + id);
				foreach (ConnectionInfo c in connections.conn[id]) {
					Debug.Log ("conn to " + c.path.pathId);
				}
			}
			return connections.conn [pathId];
		}

		public void AddPathConnection (long pathId, Path connectedPath, ConnectionInfo.ConnectionDirection d)
		{

			if (connections == null) {
				

				
				connections = new Dictionary<long, List<ConnectionInfo>> ();
			}

			connections.AddPathConnection (pathId, connectedPath, d);

			if (!connections.conn.ContainsKey (pathId)) {
		
				connections.conn [pathId] = new List<ConnectionInfo> ();

			}
			ConnectionInfo info;
			info.path = connectedPath;
			info.type = d;
			Debug.Log ("Adding path conn " + info.path.pathId + " to " + pathId);
			connections.conn [pathId].Add (info);

}

void OnDrawGizmosSelected ()
{
	Debug.Log ("Draw Gizmos selected");
	Debug.Log ("Playin" + Application.isPlaying);
	Gizmos.color = Color.white;
	if (connections != null && connections.conn != null) {
		foreach (List<ConnectionInfo> l in connections.conn.Values) {
			foreach (ConnectionInfo i in l) {
				if (Application.isPlaying) {
					Debug.Log (i.path.pathId);
				}
				i.path.DrawPath (true, Color.black);
			}
		}
	}
}

}

	*/
	

	[System.Serializable]
	public class PathConnectorData   {



	
		public List<ConnectionInfo> connectionsList;
		public  PathConnectorData() {
			connectionsList = new List<ConnectionInfo> ();

		}


		public void AddPathConnection (long pathId, Path connectedPath, ConnectionInfo.ConnectionDirection d, TrafficLight t, int tlindex) {
			int index = connectionsList.FindIndex (x=>x.pathId==pathId);
			ConnectionInfo info = null;
			if (index==-1) {

				info = new ConnectionInfo(pathId);
				connectionsList.Add (info);
				//Keep it sorted: few elements and searches are more common than insertions
				connectionsList.Sort();

			} else {
				info=connectionsList[index];
			}

			info.AddPathDirectionInfo (connectedPath, d, t, tlindex);

		//	Debug.Log ("Adding path conn " + info.pathId + " to " + pathId);


		}
		[NotNull]
		public ConnectionInfo GetConnectionInfo(long id) {
			
			int index = connectionsList.FindIndex (x =>x.pathId == id);

			if (index == -1) {
				return null;
			} 
			else if(index == null){
				throw new Exception("index is null, mothefucker!");
			}
			else {
				return connectionsList [index];
			}

		}
		public List<long> GetIncomingPaths() {
			List<long> inc = new List<long> ();
			foreach (ConnectionInfo info in connectionsList) {
				inc.Add (info.pathId);
			}
			return inc;
		}

	}

	[System.Serializable]
	public class ConnectionInfo
	{
		public enum ConnectionDirection
		{
			Straight,
			Turn,
			Left,
			Right,
			PartiallyLeft,
			PartiallyRight,
			Invalid}

		;
		[System.Serializable]
		public class PathDirectionInfo
		{
			public TrafficLight trafficLight =null;
			public int trafficLightIndex = 0;
			public Path p;
			public ConnectionDirection direction;
			public PathDirectionInfo(Path p, ConnectionDirection d, TrafficLight t, int tlindex) {
				this.p=p;
				this.direction=d;
				this.trafficLight= t;
				this.trafficLightIndex=tlindex;
			}
		}
		public ConnectionInfo(long id) {
			this.pathId = id;
			connectedPaths = new List<PathDirectionInfo> ();
		}
		/*public int CompareTo(ConnectionInfo other) {
			return this.pathId.CompareTo (other.pathId);
		}*/
		public long pathId;
		public List<PathDirectionInfo> connectedPaths;
		public void AddPathDirectionInfo(Path p, ConnectionDirection d, TrafficLight t, int tlindex) {
			connectedPaths.Add (new PathDirectionInfo (p, d,t, tlindex));
		}
		public bool IsPathIdInConnectedPaths(long id) {
			for (int i = 0; i < connectedPaths.Count; i++) {
			//foreach (PathDirectionInfo pair in connectedPaths) {
				if (connectedPaths[i].p.pathId == id) {
					return true;
				}
			}
			return false;
		}
		public PathDirectionInfo GetPathDirectionInfoInConnectedPaths(long id) {
			for (int i = 0; i < connectedPaths.Count; i++) {
				if (connectedPaths [i].p.pathId == id) {
					return connectedPaths [i];
				}
			}
			return null;
		}
		//public ConnectionDirection type;

	}
	public class PathConnector : MonoBehaviour
	{

		[SerializeField]
		private PathConnectorData connections = null;


		// Use this for initialization

		/*
		void Start ()
		{
			Debug.Log ("Start path connector " + name);
			Debug.Log (connections);
			Debug.Log (connections.connectionsList);
			Debug.Log (connections.connectionsList.Count);
	
			foreach (ConnectionInfo i in connections.connectionsList) {
				Debug.Log ("key =" + i.pathId);
				foreach (ConnectionInfo.PathDirectionPair p in i.connectedPaths) {
					Debug.Log ("conn to " + p.p.pathId);
				}
			}

		}
	*/


		public List<long> GetIncomingPathsToConnector() {
			return connections.GetIncomingPaths ();
		}

		public bool IsPathIdConnectedTo(long fromId, long toId) {
			ConnectionInfo info = GetPathsConnectedTo (fromId);
			if (info == null) {
				return false;
			} else {
				return info.IsPathIdInConnectedPaths (toId);
			}
		}

		public ConnectionInfo GetPathsConnectedTo (long pathId)
		{
			/*Debug.Log ("Start path connector " + name);
			Debug.Log (connections);
			Debug.Log (connections.connectionsList);
			Debug.Log (connections.connectionsList.Count);

			foreach (ConnectionInfo i in connections.connectionsList) {
				Debug.Log ("key =" + i.pathId);
				foreach (ConnectionInfo.PathDirectionPair p in i.connectedPaths) {
					Debug.Log ("conn to " + p.p.pathId);
				}
			}
			*/
			//Debug.Log ("GetPathsConnectedTo " + name);
			return connections.GetConnectionInfo (pathId);
		}

		public void AddPathConnection (long pathId, Path connectedPath, ConnectionInfo.ConnectionDirection d, TrafficLight t, int tlindex)
		{
	

			if (connections == null) {
				
			
				connections = new PathConnectorData ();

			}
			connections.AddPathConnection (pathId, connectedPath, d, t, tlindex);
	
		}

		void OnDrawGizmosSelected ()
		{
			
			Gizmos.color = Color.white;
			if (connections != null && connections.connectionsList != null) {
				foreach (ConnectionInfo i in connections.connectionsList) {
					foreach (ConnectionInfo.PathDirectionInfo p in i.connectedPaths) {
						
						p.p.DrawPath (true, Color.black);
					}
				}
			}
		}

	}
}
