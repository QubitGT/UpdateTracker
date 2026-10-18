using System;

namespace Unity.Multiplayer.Center.Common;

[Serializable]
public class SelectedSolutionsData
{
	public enum HostingModel
	{
		None,
		ClientHosted,
		DedicatedServer,
		CloudCode,
		DistributedAuthority
	}

	public enum NetcodeSolution
	{
		None,
		NGO,
		N4E,
		CustomNetcode,
		NoNetcode
	}

	public HostingModel SelectedHostingModel;

	public NetcodeSolution SelectedNetcodeSolution;
}
