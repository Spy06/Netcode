using System;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

public enum ConnectionType{
	Listen, Connect
}

namespace NetCode
{
	public class NetSocket{
		public const int MAX_PACKET_RETRY = 10;

		private Dictionary<int, NetPacket> tracker = new Dictionary<int, NetPacket>();

		private Socket socket;

		private string ip;
		private int port;

		private IPEndPoint endPoint;

		private ConnectionType cType;

		private Dictionary<int, NetPacket> pendingPackets = new Dictionary<int, NetPacket> ();

		/// <summary>
		/// Construct a new socket, and setting the values to default
		/// </summary>
		public NetSocket(ConnectionType type){
			cType = type;

			socket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			uint IOC_IN = 0x80000000;
			uint IOC_VENDOR = 0x18000000;
			uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
			socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

			if (type == ConnectionType.Connect) {
				ip = "127.0.0.1";
				port = 80;
			} else {
				port = 80;
			}
		}

		public NetSocket IP(string a){
			ip = a;
			return this;
		}
		public NetSocket Port(int a){
			port = a;
			return this;
		}

		public void Build(){
			if (cType == ConnectionType.Connect) {
				endPoint = new IPEndPoint (IPAddress.Parse (ip), port);
			} else {
				endPoint = new IPEndPoint (IPAddress.Any, port);
				socket.Bind (endPoint);
				UnityEngine.Debug.Log ("Listening...");
			}
		}

		public void Connect(){
			EndPoint targetConnect = (EndPoint)endPoint;
			socket.Connect (targetConnect);

			UnityEngine.Debug.Log ("Connecting to " + targetConnect.ToString ());

		}

		public EndPoint PollEvents(){
			if (socket.Poll (50000, SelectMode.SelectRead)) {
				EndPoint ep = new IPEndPoint (IPAddress.Any, 0);
				byte[] buffer =  new byte[1024];
				int recv = socket.ReceiveFrom (buffer, ref ep);
				NetPacket packet = new NetPacket (1024, this);
				packet.ReceiveMessage(buffer, recv);
				return ep;
			}

			return null;
		}

		public void SendData(NetPacket packet){
			byte[] dataToSend = packet.GetData ();
			EndPoint targetSend = (EndPoint)endPoint;
			socket.SendTo (dataToSend, targetSend);

			if (packet.PacketSendOption == SendOptions.Reliable) {
				packet.CatchPacket ();
				pendingPackets.Add (packet.PacketID, packet);
			}
		}

		public void SendData(NetPacket packet, EndPoint targetSend){
			byte[] dataToSend = packet.GetData ();
			socket.SendTo (dataToSend, targetSend);

			if (packet.PacketSendOption == SendOptions.Reliable) {
				packet.CatchPacket ();
				pendingPackets.Add (packet.PacketID, packet);
			}
		}

		public void ProcessPacket(NetPacket packet){
			NetPacket p;
			if (tracker.TryGetValue (packet.PacketID, out p)) {

			} else {
				tracker.Add (packet.PacketID, packet);
				packet.ReadMessage ();
			}
		}

		public void ProcessAck(int packetID){
			NetPacket packet;
			if (pendingPackets.TryGetValue (packetID, out packet)) {
				packet.Close ();
				pendingPackets.Remove (packetID);
			}
		}

		public void SendAck(int packetID, EndPoint target){
			NetPacket packet = new NetPacket (1024, packetID, SendOptions.Unreliable, this);
			packet.WriteFirst (PacketType.Ack);
			packet.WriteMessage ("Acked packet " + packetID);

			byte[] data = packet.GetData ();
			socket.SendTo (data, target);
		}
	}
}