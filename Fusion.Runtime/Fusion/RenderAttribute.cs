using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class RenderAttribute : Attribute
{
	public RenderTimeframe Timeframe { get; set; }

	public RenderSource Source { get; set; }

	public string Method { get; set; }

	public RenderAttribute()
	{
	}

	public RenderAttribute(RenderTimeframe timeframe, RenderSource source)
	{
		Timeframe = timeframe;
		Source = source;
	}
}
