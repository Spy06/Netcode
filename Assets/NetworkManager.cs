using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetCode;

public class NetworkManager : MonoBehaviour, INetHandler {

	public string ip = "127.0.0.1";
	public int port = 80;

	private int currentPacketSegment;
	NetSocket socket;

	public Player playerPrefab;

	private Dictionary<int, Player> players = new Dictionary<int, Player> ();

	public static NetworkManager instance;
	private void Awake(){
		if (instance == null)
			instance = this;
		else
			Destroy (this);
	}

	// Use this for initialization
	void Start () {
		socket = new NetSocket (ConnectionType.Connect).IP(ip).Port(port);
		socket.Build (this);
		socket.Connect ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		socket.PollEvents ();
		NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, socket);
		packet.WriteFirst (PacketType.Data);
		packet.WriteMessage ("Hello server, this is client");

		socket.SendData (packet);
		currentPacketSegment++;
	}

	public void SendTransform(int ep, Vector3 position){
		NetPacket packet = new NetPacket (1024, currentPacketSegment, SendOptions.Unreliable, socket);
		packet.WriteFirst (PacketType.Position);
		packet.WriteMessage (ep);
		packet.WriteMessage (position.x);
		packet.WriteMessage (position.y);
		packet.WriteMessage (position.z);
		socket.SendData (packet);
		currentPacketSegment++;
	}

	Player Spawn(bool localPlayer, int ep){
		Player p = Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
		p.isLocalPlayer = localPlayer;
		p.id = ep;

		players.Add (ep, p);

		return p;
	}

	public void OnClientConnected(int id, bool localPlayer){
		Debug.Log ("Incoming Player: " + id);
		Spawn(localPlayer, id);
	}

	public void OnPositionReceived(UnityEngine.Vector3 pos, int id){
		Player value;
		if (players.TryGetValue (id, out value)) {
			if (value.isLocalPlayer == false)
				value.transform.position = Vector3.Lerp (value.transform.position, pos, Time.deltaTime * 10f);
		} else {
			Spawn (false, id);
		}
	}

	private void OnApplicationQuit(){
		socket.Close ();
	}
}
