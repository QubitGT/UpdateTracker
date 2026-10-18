using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class NetworkStructWeavedAttribute : Attribute
{
	public int WordCount { get; }

	internal bool IsGenericComposite { get; }

	public NetworkStructWeavedAttribute(int wordCount)
	{
		WordCount = wordCount;
	}

	internal NetworkStructWeavedAttribute(int wordCount, bool isGenericComposite)
	{
		WordCount = wordCount;
		IsGenericComposite = isGenericComposite;
	}
}
