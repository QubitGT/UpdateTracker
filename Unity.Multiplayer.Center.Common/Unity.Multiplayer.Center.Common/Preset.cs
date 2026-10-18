using System;
using UnityEngine;

namespace Unity.Multiplayer.Center.Common;

[Serializable]
[InspectorOrder(InspectorSort.ByName, InspectorSortDirection.Ascending)]
public enum Preset
{
	[InspectorName("-")]
	None,
	[InspectorName("Adventure")]
	Adventure,
	[InspectorName("Shooter, Battle Royale, Battle Arena")]
	Shooter,
	[InspectorName("Racing")]
	Racing,
	[InspectorName("Card Battle, Turn-based, Tabletop")]
	TurnBased,
	[InspectorName("Simulation")]
	Simulation,
	[InspectorName("Strategy")]
	Strategy,
	[InspectorName("Sports")]
	Sports,
	[InspectorName("Role-Playing, MMO")]
	RolePlaying,
	[InspectorName("Async, Idle, Hyper Casual, Puzzle")]
	Async,
	[InspectorName("Fighting")]
	Fighting,
	[InspectorName("Arcade, Platformer, Sandbox")]
	Sandbox
}
