using System;

namespace Fusion;

public class PreserveInPluginAttribute : Attribute
{
	public bool KeepNonStateMembers { get; set; } = true;

}
