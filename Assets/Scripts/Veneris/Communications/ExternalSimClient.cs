/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.IO;

using FlatBuffers;


namespace Veneris.Communications
{
	public class ExternalSimClient : MonoBehaviour
	{


		public Int32 port=9993;
		public string server="localhost";
		public bool useFile=false;
		public string filePath="";
		Stream stream;
	
		void Awake() {
			bool initialized = false;
			if (useFile) {
				initialized=OpenFile ();
			} else {
				initialized=Connect ();
			}
			if (!initialized) {
				Debug.LogError ("Could not open file or connection");
			}
		}
		public bool OpenFile() {
			try  {
				Debug.Log("Creating file for storing messages: "+filePath);
				stream=File.OpenWrite(filePath);
				return true;
			} catch (Exception e) {
				Debug.LogError ("Could not open file");
				return false;
			}
		}
		public bool Connect ()
		{

		


			try {
				// Create a TcpClient. 
				Debug.Log("Connecting to Veneris server at "+server+":"+port);
				TcpClient client = new TcpClient (server, port);   
			
				// Get a client stream for reading and writing. 
				stream = client.GetStream ();

				Debug.Log("Connected...");
				return true;
			} catch (ArgumentNullException e) {
				Debug.LogError ("ArgumentNullException: " + e.Message);
				return false;
			} catch (SocketException e) {
				Debug.LogError ("SocketException: " + e.Message);
				return false;
			}
		}
	
		// Update is called once per frame
		public void SendQueue ()
		{
			//Debug.Log ("SendQueue");
			if (stream != null) {
				if (stream.CanWrite) {
			
					while(MessageManager.hasMessage ()) {
						// Get our message
						//uint objType = MessageManager.consumeType();
						//byte [] obj = MessageManager.consumeMessage();
						//KeyValuePair<byte[],Communications.VenerisMessageTypes> msg = MessageManager.consumeMessage ();
						MessageManager.VenerisMessage msg= MessageManager.consumeMessage ();
						if (msg.data == null) {
							//Debug.Log ("Sending only header: " + msg.Value);
							sendHeader (msg.type, 0,msg.timestamp);
						} else {
							//Debug.Log ("Sending message: "+msg.Value+". Length=" + msg.Key.Length);
							// Send header message
							sendHeader (msg.type, msg.data.Length,msg.timestamp);

							// Send our message
							sendMsg (msg.data);
						}
						msg = null;
					}
					//stream.Flush ();
				}
			}

		}
		void FixedUpdate() {
			SendQueue ();
			sendTime ();
		}

		void OnDestroy() {
			Close ();
		}
		 public void Close ()
		{
			// Clear all messages in queue
			MessageManager.clearAll ();

			// Send a End message
			if (stream != null) {
				if (stream.CanWrite) {
					Debug.Log ("Sending end simulation");
					sendHeader (Communications.VenerisMessageTypes.End, 0, Time.time);
					// Close socket

					stream.Close ();
				}
			}


		}

		public void sendHeader (Communications.VenerisMessageTypes type, int length, float timestamp)
		{
			FlatBuffers.FlatBufferBuilder fbb = new FlatBufferBuilder (32);
			//Force defaults in order to keep the header length fixed, otherwise when sending a header message size=0 or time=0, the header length will be shorter than 32 and the server crashes
			fbb.ForceDefaults = true;
			//Debug.Log ("ForceDefaults=" + fbb.ForceDefaults);
			// Build header struct
			Header.StartHeader (fbb);


			Header.AddType (fbb, type);
			Header.AddSize (fbb, (uint)length);
			Header.AddTime (fbb, timestamp);
			var hm = Header.EndHeader (fbb);
			Header.FinishHeaderBuffer (fbb, hm);			
		
			// Send header message
			byte[] _bb = fbb.SizedByteArray ();

			//Debug.Log ("Header buffer size=" + _bb.Length +". type="+type +". size="+length);
		
			stream.Write (_bb, 0, _bb.Length);
			stream.Flush ();
		}

		void sendMsg (byte[] data)
		{
			stream.Write (data, 0, data.Length);
			stream.Flush ();
		}
	

		public  void sendTime ()
		{
			
			FlatBufferBuilder fbb = new FlatBufferBuilder (sizeof(float));
			fbb.ForceDefaults = true;
			ExternalTime.StartExternalTime (fbb);
			ExternalTime.AddTime (fbb, Time.time);
			var mt = ExternalTime.EndExternalTime (fbb);
			ExternalTime.FinishExternalTimeBuffer (fbb, mt);
			byte[] messageBytes = fbb.SizedByteArray ();
			//Debug.Log ("Sending time "+ messageBytes.Length +" type="+Communications.VenerisMessageTypes.ExternalTime);
			sendHeader( Communications.VenerisMessageTypes.ExternalTime,messageBytes.Length,Time.time);
			sendMsg (messageBytes);

			//MessageManager.enqueue (fbb.SizedByteArray (), (uint)Type.ExternalTime);
		}
	}
}