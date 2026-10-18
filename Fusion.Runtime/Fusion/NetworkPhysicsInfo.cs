using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(10)]
public struct NetworkPhysicsInfo : INetworkStruct
{
	public const int WORD_COUNT = 10;

	public const int SIZE = 40;

	[FieldOffset(0)]
	public float TimeScale;
}
