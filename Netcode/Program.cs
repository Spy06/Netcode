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

		private static Dictionary<int, EndPoint> clients = new Dictionary<int, EndPoint>();
		private static Dictionary<int, Vector3> transforms = new Dictionary<int, Vector3>();

		private static int currentClientID;

		public static void Main (string[] args)
		{
			Connection = new NetSocket (ConnectionType.Listen).Port (port);
			Connection.Build ();
			//timer = new System.Timers.Timer (30);
			//timer.AutoReset = false;
			//timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => Poll ();
			while (true) {
				Poll ();
				SendToAll ();
			}
		}

		private static void Poll(){
			EndPoint ep = Connection.PollEvents ();
			if (ep != null) {
				if (!clients.ContainsValue (ep)){
					Console.WriteLine ("Incoming Connection From {0}", ep.ToString ());
					clients.Add (currentClientID, ep);
					transforms.Add (currentClientID, new Vector3 (0f, 0f, 0f));
					SendConnection (ep, currentClientID);
					currentClientID++;
				}
			}
		}

		public static void OnPositionReceived(Vector3 pos, int id){
			if (transforms.ContainsKey (id)) {
				transforms [id] = pos;
			}
		}

		private static void SendConnection(EndPoint ep, int id){
			foreach (int e in transforms.Keys) {
				if (e != id) {
					NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Reliable, Connection);
					packet.WriteFirst (PacketType.Connection);
					packet.WriteMessage (id);
					packet.WriteMessage (false);

					Connection.SendData (packet, clients[e]);
					packet = null;
					currentPacketSegment++;
					Console.WriteLine ("Sending {0} to {1}", ep, e);
				}
			}

			foreach (int e in transforms.Keys) {
				NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Reliable, Connection);
				packet.WriteFirst (PacketType.Connection);
				packet.WriteMessage (e);
				bool lp;
				if (e == id) {
					lp = true;
				} else {
					lp  = false;
				}

				packet.WriteMessage (lp);

				Connection.SendData (packet, ep);
				packet = null;
				currentPacketSegment++;
			}

		}

		private static void SendPosition(Vector3 pos, EndPoint ip, int id){
			for(int i = 0; i < transforms.Count; i++){
				if (i != id) {
					NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, Connection);
					packet.WriteFirst (PacketType.Position);
					packet.WriteMessage (id);
					packet.WriteMessage (pos.x);
					packet.WriteMessage (pos.y);
					packet.WriteMessage (pos.z);

					Connection.SendData (packet, clients[i]);
					currentPacketSegment++;
				}
			}
		}

		private static void SendToAll(){
			for(int i = 0; i < transforms.Count; i++){
				//SendPosition (e, transforms [ep]);
				//SendMessage (clients[i]);
				SendPosition (transforms [i], clients[i], i);
			}
		}

		private static void SendPosition(EndPoint targetSend, Vector3 pos){
			NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, Connection);
			packet.WriteFirst (PacketType.Position);
			packet.WriteMessage (targetSend.ToString());
			packet.WriteMessage (pos.x);
			packet.WriteMessage (pos.y);
			packet.WriteMessage (pos.z);

			Connection.SendData (packet, targetSend);
			currentPacketSegment++;
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
