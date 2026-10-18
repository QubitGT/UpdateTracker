using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ScriptHelpAttribute : PropertyAttribute
{
	public bool Hide { get; set; }

	public string Url { get; set; }

	public ScriptHeaderBackColor BackColor { get; set; } = ScriptHeaderBackColor.Gray;


	public ScriptHeaderStyle Style { get; set; } = ScriptHeaderStyle.Photon;

}
