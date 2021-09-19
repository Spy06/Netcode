/// <summary>
/// Sending options, used for sending packet. Be careful, wrong sending option can result in a problem
/// </summary>
public enum PacketType : byte{

	/// <summary>
	/// Data packet, used for sending a message
	/// </summary>
	Data,

	/// <summary>
	/// Client connection
	/// </summary>
	Connection,

	/// <summary>
	/// Acknowledgement, used for replying a reliable message
	/// </summary>
	Ack,

	/// <summary>
	/// Position packet
	/// </summary>
	Position
}