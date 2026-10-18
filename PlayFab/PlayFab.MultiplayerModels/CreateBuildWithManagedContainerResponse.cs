using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class CreateBuildWithManagedContainerResponse : PlayFabResultCommon
{
	public string BuildId;

	public string BuildName;

	public ContainerFlavor? ContainerFlavor;

	public DateTime? CreationTime;

	public List<AssetReference> GameAssetReferences;

	public List<GameCertificateReference> GameCertificateReferences;

	public string GameWorkingDirectory;

	public InstrumentationConfiguration InstrumentationConfiguration;

	public Dictionary<string, string> Metadata;

	public int MultiplayerServerCountPerVm;

	public string OsPlatform;

	public List<Port> Ports;

	public List<BuildRegion> RegionConfigurations;

	public string ServerType;

	public string StartMultiplayerServerCommand;

	public AzureVmSize? VmSize;
}
