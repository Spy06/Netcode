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
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine ("Something is wrong with your connection");
				Console.ForegroundColor = ConsoleColor.White;
				return;
			}

			conn.SendData (this);
			retry++;
			timer.Stop ();
			timer.Start ();
		}

		public void ReceiveMessage(byte[] buffer, int recv){
			byte[] _data = new byte[recv];
			Array.Copy (buffer, _data, recv);

			stream = new MemoryStream (_data);
			reader = new BinaryReader (stream);
			packetID = reader.ReadInt32 ();
			SendOptions sendOpt = (SendOptions)reader.ReadByte ();
			PacketType packetType = (PacketType)reader.ReadByte ();
			acked = true;

			if (sendOpt == SendOptions.Reliable) {
				//conn.SendAck (packetID, ep);
				conn.ProcessPacket (this);
			} else {
				ReadMessage ();
			}
		}

		public void ReadMessage(){
			string msg = reader.ReadString ();
			Console.WriteLine ("Packet ID ({0})Message: {1}", packetID, msg);
		}

		/// <summary>
		/// Write a message to the packet
		/// </summary>
		/// <param name="msg">Message.</param>
		public void WriteMessage(string msg){
			writer.Write (msg);
		}

		public byte[] GetData(){
			return data;
		}
	}
}

