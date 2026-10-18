using System;
using System.ComponentModel;

namespace Fusion;

[Flags]
public enum SimulationStages
{
	[Description("Fwrd")]
	Forward = 2,
	[Description("Resim")]
	Resimulate = 4
}
