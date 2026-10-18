using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
public struct SimulationInputHeader
{
	public const int WORD_COUNT = 4;

	public const int SIZE = 16;

	[FieldOffset(0)]
	public Tick Tick;

	[FieldOffset(4)]
	public float InterpAlpha;

	[FieldOffset(8)]
	public Tick InterpFrom;

	[FieldOffset(12)]
	public Tick InterpTo;
}
