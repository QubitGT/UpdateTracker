using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class BuildRegion : PlayFabBaseModel
{
	public CurrentServerStats CurrentServerStats;

	public DynamicStandbySettings DynamicStandbySettings;

	public int MaxServers;

	public string Region;

	public int StandbyServers;

	public string Status;
}
