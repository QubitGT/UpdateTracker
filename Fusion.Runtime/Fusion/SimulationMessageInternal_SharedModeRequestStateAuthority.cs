using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
internal struct SimulationMessageInternal_SharedModeRequestStateAuthority
{
	public const int SIZE = 8;

	[FieldOffset(0)]
	public NetworkId Object;

	[FieldOffset(4)]
	public int Acquire;
}
