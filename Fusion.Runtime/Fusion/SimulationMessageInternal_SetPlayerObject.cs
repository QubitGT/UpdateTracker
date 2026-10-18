using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
internal struct SimulationMessageInternal_SetPlayerObject
{
	public const int SIZE = 4;

	[FieldOffset(0)]
	public NetworkId Object;
}
