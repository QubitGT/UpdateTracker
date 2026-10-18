using Fusion.Sockets;

namespace Fusion;

internal struct SimulationArgs
{
	public SimulationModes Mode;

	public NetAddress Address;

	public INetSocket Socket;

	public NetworkProjectConfig Config;

	public Simulation.ICallbacks Callbacks;

	public Tick ResumeTick;

	public byte[] ResumeState;

	public NetworkId ResumeNetworkId;

	public bool IsPlayer => Mode == SimulationModes.Client || Mode == SimulationModes.Host;

	public bool IsServer => Mode == SimulationModes.Server || Mode == SimulationModes.Host;
}
