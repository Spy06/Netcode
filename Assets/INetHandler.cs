public interface INetHandler{
	void OnClientConnected(int id, bool localPlayer);

	void OnPositionReceived(UnityEngine.Vector3 pos, int id);
}