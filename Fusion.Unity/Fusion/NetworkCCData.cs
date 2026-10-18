using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(22)]
public struct NetworkCCData : INetworkStruct
{
	public const int WORDS = 18;

	public const int SIZE = 72;

	[FieldOffset(0)]
	public NetworkTRSPData TRSPData;

	[FieldOffset(56)]
	private int _grounded;

	[FieldOffset(60)]
	private Vector3Compressed _velocityData;

	public bool Grounded
	{
		get
		{
			return _grounded == 1;
		}
		set
		{
			_grounded = (value ? 1 : 0);
		}
	}

	public Vector3 Velocity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _velocityData;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			_velocityData = value;
		}
	}
}
