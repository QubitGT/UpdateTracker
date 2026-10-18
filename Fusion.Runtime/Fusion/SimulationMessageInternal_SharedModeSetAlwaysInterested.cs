using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
internal struct SimulationMessageInternal_SharedModeSetAlwaysInterested
{
	public const int SIZE = 12;

	[FieldOffset(0)]
	public NetworkId Object;

	[FieldOffset(4)]
	public int Interested;

	[FieldOffset(8)]
	public int Player;
}
