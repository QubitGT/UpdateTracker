namespace Photon.Realtime;

public struct PhotonPortDefinition
{
	public static readonly PhotonPortDefinition AlternativeUdpPorts = new PhotonPortDefinition
	{
		NameServerPort = 27000,
		MasterServerPort = 27001,
		GameServerPort = 27002
	};

	public ushort NameServerPort;

	public ushort MasterServerPort;

	public ushort GameServerPort;
}
