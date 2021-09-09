/// <summary>
/// Sending options, used for sending packet. Be careful, wrong sending option can result in a problem
/// </summary>
public enum SendOptions : byte{

	/// <summary>
	/// Reliable, packet always transferred
	/// </summary>
	Reliable,

	/// <summary>
	/// Unreliable, packet can be dropped, duplicated
	/// </summary>
	Unreliable
}