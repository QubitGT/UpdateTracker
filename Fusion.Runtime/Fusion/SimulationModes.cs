using System;
using System.ComponentModel;

namespace Fusion;

[Flags]
public enum SimulationModes
{
	[Description("Server")]
	Server = 1,
	[Description("Host")]
	Host = 2,
	[Description("Client")]
	Client = 4
}
