using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class NetworkedWeavedAttribute : Attribute
{
	public int WordOffset { get; }

	public int WordCount { get; }

	public NetworkedWeavedAttribute(int wordOffset, int wordCount)
	{
		WordOffset = wordOffset;
		WordCount = wordCount;
	}
}
