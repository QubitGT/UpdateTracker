using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NetworkBehaviourWeavedAttribute : Attribute
{
	public int WordCount { get; }

	public NetworkBehaviourWeavedAttribute(int wordCount)
	{
		WordCount = wordCount;
	}
}
