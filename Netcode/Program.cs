using System;
using System.Net;
using System.Net.Sockets;
using NetCode;
using System.Collections.Generic;

namespace Server
{
	class MainClass
	{
		private static NetSocket Connection;
		private static int port = 80;

		private static int currentPacketSegment;

		private static Dictionary<EndPoint, EndPoint> clients = new Dictionary<EndPoint, EndPoint>();

		private static int currentClientID;

		public static void Main (string[] args)
		{
			Connection = new NetSocket (ConnectionType.Listen).Port (port);
			Connection.Build ();
			while (true) {
				Poll ();
				SendToAll ();
			}
		}

		private static void Poll(){
			EndPoint ep = Connection.PollEvents ();
			EndPoint value;
			if (ep != null) {
				if (!clients.TryGetValue (ep, out value)) {
					Console.WriteLine ("Incoming Connection From {0}", ep.ToString ());
					clients.Add (ep, ep);
					currentClientID++;
				}
			}

		}

		private static void SendToAll(){
			foreach (EndPoint ep in clients.Values) {
				SendMessage (ep);
			}
		}

		private static void SendMessage(EndPoint targetSend){
			NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, Connection);
			packet.WriteFirst (PacketType.Data);
			packet.WriteMessage ("Hello Client!, This Is Your Server");

			Connection.SendData (packet, targetSend);
			currentPacketSegment++;
		}
	}
}
