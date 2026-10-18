using Fusion.Protocol;

namespace Fusion;

public sealed class HostMigrationToken
{
	public GameMode GameMode { get; private set; }

	internal CloudCommunicator CloudCommunicator { get; private set; }

	internal byte[] ResumeState => HostSnapshot?.Data;

	internal Tick? ResumeTick => HostSnapshot?.Tick;

	internal NetworkId? ResumeId => (HostSnapshot != null) ? new NetworkId?(new NetworkId(HostSnapshot.NetworkID)) : null;

	private Snapshot HostSnapshot { get; }

	internal HostMigrationToken(Snapshot hostSnapshot, CloudCommunicator cloudCommunicator, GameMode gameMode)
	{
		HostSnapshot = hostSnapshot;
		CloudCommunicator = cloudCommunicator;
		GameMode = gameMode;
	}
}
