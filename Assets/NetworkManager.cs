using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetCode;

public class NetworkManager : MonoBehaviour {

	public string ip = "127.0.0.1";
	public int port = 80;

	private int currentPacketSegment;
	NetSocket socket;

	// Use this for initialization
	void Start () {
		socket = new NetSocket (ConnectionType.Connect).IP(ip).Port(port);
		socket.Build ();
		socket.Connect ();
	}
	
	// Update is called once per frame
	void Update () {
		socket.PollEvents ();
		NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, socket);
		packet.WriteFirst (PacketType.Data);
		packet.WriteMessage ("Hello server, this is client");

		socket.SendData (packet);
		currentPacketSegment++;
	}
}
