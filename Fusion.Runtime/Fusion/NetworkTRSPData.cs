using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(14)]
public struct NetworkTRSPData : INetworkStruct
{
	public const int WORDS = 14;

	public const int SIZE = 56;

	public const int POSITION_OFFSET = 2;

	[FieldOffset(0)]
	public NetworkBehaviourId Parent;

	[FieldOffset(8)]
	public Vector3 Position;

	[FieldOffset(20)]
	public Quaternion Rotation;

	[FieldOffset(36)]
	public Vector3Compressed Scale;

	[FieldOffset(48)]
	public int TeleportKey;

	[FieldOffset(52)]
	public NetworkId AreaOfInterestOverride;

	public static NetworkBehaviourId NonNetworkedParent
	{
		get
		{
			NetworkBehaviourId result = default(NetworkBehaviourId);
			result.Object = default(NetworkId);
			result.Behaviour = 1;
			return result;
		}
	}
}
