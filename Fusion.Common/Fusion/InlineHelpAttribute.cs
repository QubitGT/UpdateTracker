using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
public class InlineHelpAttribute : DecoratingPropertyAttribute
{
	private new const int DefaultOrder = -9000;

	public bool ShowTypeHelp { get; set; } = true;


	public InlineHelpAttribute()
		: base(-9000)
	{
	}
}
