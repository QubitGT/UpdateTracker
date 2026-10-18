using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class NetworkInputWeavedAttribute : Attribute
{
	public int WordCount { get; }

	public NetworkInputWeavedAttribute(int wordCount)
	{
		WordCount = wordCount;
	}
}
