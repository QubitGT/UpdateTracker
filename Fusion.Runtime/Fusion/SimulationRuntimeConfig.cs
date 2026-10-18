using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
public struct SimulationRuntimeConfig
{
	[FieldOffset(0)]
	public TickRate.Resolved TickRate;

	[FieldOffset(16)]
	public SimulationModes ServerMode;

	[FieldOffset(20)]
	public int PlayerMaxCount;

	[FieldOffset(24)]
	public PlayerRef MasterClient;

	[FieldOffset(28)]
	public PlayerRef HostPlayer;

	[FieldOffset(32)]
	public Topologies Topology;
}
