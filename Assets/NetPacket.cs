using System;
using System.IO;
using System.Timers;
using System.Net;

namespace NetCode
{
	public class NetPacket
	{
		private byte[] data;
		private MemoryStream stream;
		private BinaryWriter writer;
		private BinaryReader reader;
		private int packetID;

		public int PacketID { get { return packetID; } }

		private SendOptions packetSendOption;

		public SendOptions PacketSendOption { get { return packetSendOption; } }

		private bool acked;

		private bool started;

		private int retry;

		private NetSocket conn;

		private Timer timer;

		public void Close(){
			timer.Stop ();
		}

		public NetPacket(int size, int _packetID, SendOptions sendOption, NetSocket socket){
			data = new byte[size];
			packetID = _packetID;
			packetSendOption = sendOption;
			conn = socket;
		}

		public NetPacket(int size, SendOptions sendOption, NetSocket socket){
			data = new byte[size];
			packetSendOption = sendOption;
			conn = socket;
		}

		public NetPacket(int size, NetSocket socket){
			data = new byte[size];
			conn = socket;
		}

		public void WriteFirst(PacketType packetType){
			stream = new MemoryStream (data);
			writer = new BinaryWriter (stream);
			writer.Write (packetID);
			writer.Write ((byte)packetSendOption);
			writer.Write ((byte)packetType);
		}

		public void CatchPacket(){
			if (started)
				return;

			timer = new Timer (1000);
			timer.AutoReset = false;
			timer.Elapsed += (object sender, ElapsedEventArgs e) => Resend();
			timer.Start ();
			started = true;
		}

		private void Resend(){
			if (retry >= NetSocket.MAX_PACKET_RETRY || acked) {
				UnityEngine.Debug.Log ("Something is wrong");
				return;
			}

			conn.SendData (this);
			retry++;
			timer.Stop ();
			timer.Start ();
		}

		public void ReceiveMessage(byte[] buffer, int recv, EndPoint ep){
			byte[] _data = new byte[recv];
			Array.Copy (buffer, _data, recv);

			stream = new MemoryStream (_data);
			reader = new BinaryReader (stream);
			packetID = reader.ReadInt32 ();
			SendOptions sendOpt = (SendOptions)reader.ReadByte ();
			PacketType packetType = (PacketType)reader.ReadByte ();
			acked = true;

			if (packetType == PacketType.Data) {
				if (sendOpt == SendOptions.Reliable) {
					conn.SendAck (packetID, ep);
					conn.ProcessPacket (this);
				} else {
					ReadString ();
				}
			} else if (packetType == PacketType.Connection) {
				UnityEngine.Debug.Log ("connection");
				conn.handler.OnClientConnected (reader.ReadInt32 (), reader.ReadBoolean ());
			} else if (packetType == PacketType.Position) {
				int ip = reader.ReadInt32 ();
				float x = reader.ReadSingle ();
				float y = reader.ReadSingle ();
				float z = reader.ReadSingle ();
				UnityEngine.Vector3 received = new UnityEngine.Vector3 (x, y, z);
				conn.handler.OnPositionReceived (received, ip);
			}
		}

		public string ReadString(){
			string msg = reader.ReadString ();
			if (msg.Contains ("Ack"))
				UnityEngine.Debug.Log ("Packet ID ("+ packetID +")Message: " + msg);

			return msg;
		}

		public float ReadFloat(){
			return reader.ReadSingle ();
		}

		/// <summary>
		/// Write a message to the packet
		/// </summary>
		/// <param name="msg">Message.</param>
		public void WriteMessage(string msg){
			writer.Write (msg);
		}

		/// <summary>
		/// Write a message to the packet
		/// </summary>
		/// <param name="msg">Message.</param>
		public void WriteMessage(float msg){
			writer.Write (msg);
		}

		/// <summary>
		/// Write a message to the packet
		/// </summary>
		/// <param name="msg">Message.</param>
		public void WriteMessage(int msg){
			writer.Write (msg);
		}

		/// <summary>
		/// Write a message to the packet
		/// </summary>
		/// <param name="msg">Message.</param>
		public void WriteMessage(bool msg){
			writer.Write (msg);
		}


		public byte[] GetData(){
			return data;
		}
	}
}

