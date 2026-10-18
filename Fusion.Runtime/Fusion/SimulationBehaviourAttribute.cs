using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class SimulationBehaviourAttribute : Attribute
{
	public SimulationStages Stages { get; set; }

	public SimulationModes Modes { get; set; }

	public Topologies Topologies { get; set; }
}
