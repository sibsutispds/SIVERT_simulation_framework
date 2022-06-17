/******************************************************************************/
// 
// Copyright (c) 2019 Esteban Egea-Lopez http://ait.upct.es/eegea
// 
/*******************************************************************************/



using System;
using System.Collections.Generic;
using UnityEngine;
namespace Veneris.Communications
{
	public static class MessageManager
	{

		public class VenerisMessage {
			public byte[] data;
			public float timestamp;
			public Communications.VenerisMessageTypes type;
			public VenerisMessage(Communications.VenerisMessageTypes type, float timestamp, byte[] data) {
				this.type=type;
				this.timestamp = timestamp;
				this.data = data;
			}
		}
		//private static Queue<KeyValuePair<byte[],Communications.VenerisMessageTypes>> msgQueue = new Queue<KeyValuePair<byte[],Communications.VenerisMessageTypes>> ();
		private static Queue<VenerisMessage> msgQueue = new Queue<VenerisMessage> ();


		public static void enqueue (byte[] enq, Communications.VenerisMessageTypes type)
		{
			//msgQueue.Enqueue (new KeyValuePair<byte[],Communications.VenerisMessageTypes> (enq, type));
			msgQueue.Enqueue (new VenerisMessage (type, Time.time, enq));
		}

		public static bool hasMessage ()
		{
			return (msgQueue.Count > 0) ? true : false;
		}

		public static VenerisMessage consumeMessage ()
		{
			return msgQueue.Dequeue ();
		}

		public static void clearAll ()
		{
			msgQueue.Clear ();
		}
	}
}


