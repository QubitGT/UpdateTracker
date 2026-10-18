using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fusion;

[Serializable]
public class SimulationConfig
{
	public enum InputTransferModes
	{
		Redundancy = 0,
		RedundancyUncompressed = 2,
		LatestState = 1
	}

	public enum DataConsistency
	{
		Full,
		Eventual
	}

	public enum SimulationTimeMode
	{
		UnscaledDeltaTime,
		DeltaTime
	}

	[HideInInspector]
	[InlineHelp]
	public int InputDataWordCount;

	public NetworkProjectConfig.ReplicationFeatures ReplicationFeatures = NetworkProjectConfig.ReplicationFeatures.Scheduling;

	[FormerlySerializedAs("inputTransferMode")]
	[InlineHelp]
	public InputTransferModes InputTransferMode;

	[NonSerialized]
	public DataConsistency ObjectDataConsistency;

	[InlineHelp]
	public SimulationTimeMode SimulationUpdateTimeMode = SimulationTimeMode.UnscaledDeltaTime;

	[FormerlySerializedAs("DefaultPlayerCount")]
	[FormerlySerializedAs("DefaultPlayers")]
	[FormerlySerializedAs("Players")]
	[Unit(Units.None)]
	[InlineHelp]
	[RangeEx(1.0, 255.0)]
	public int PlayerCount = 10;

	public TickRate.Selection TickRateSelection = TickRate.Default;

	[NonSerialized]
	public Topologies Topology;

	[NonSerialized]
	public bool HostMigration;

	[HideInInspector]
	public byte MaxObjectDestroysSentPerPacket = 32;

	internal bool EnableSerializers = true;

	public bool SchedulingEnabled => (ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.Scheduling) == NetworkProjectConfig.ReplicationFeatures.Scheduling;

	public bool AreaOfInterestEnabled => (ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement;

	public bool SchedulingWithoutAOI => SchedulingEnabled && !AreaOfInterestEnabled;

	public int InputTotalWordCount => InputDataWordCount + 4;

	internal SimulationConfig Init(int? playerCountOverride, int? inputWordCount)
	{
		SimulationConfig simulationConfig = Copy();
		if (playerCountOverride.HasValue)
		{
			simulationConfig.PlayerCount = playerCountOverride.Value;
		}
		if (inputWordCount.HasValue)
		{
			simulationConfig.InputDataWordCount = inputWordCount.Value;
		}
		return simulationConfig;
	}

	internal SimulationConfig Copy()
	{
		return (SimulationConfig)MemberwiseClone();
	}
}
