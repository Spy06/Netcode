using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using NetCode;
using System.Timers;

namespace Client
{
	class MainClass
	{
		private static NetSocket Connection;

		private static string IP = "127.0.0.1";
		private static int Port = 80;

		private static int currentPacketSegment;
		static Timer timer;
		public static void Main (string[] args){
			Connection = new NetSocket (ConnectionType.Connect).IP (IP).Port (Port);
			Connection.Build ();
			Connection.Connect ();

			//StartTimer ();
			while (true) {
				Connection.PollEvents ();
				SendMessage ();
			}
		}

		private static void StartTimer(){
			timer = new Timer (1000);
			timer.AutoReset = false;
			timer.Elapsed += (object sender, ElapsedEventArgs e) => SendMessage();
			timer.Start ();
		}

		private static void SendMessage(){
			NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, Connection);
			packet.WriteFirst (PacketType.Data);
			packet.WriteMessage ("Hello server, this is client");

			Connection.SendData (packet);
			currentPacketSegment++;
			//timer.Start ();
		}
	}
}
