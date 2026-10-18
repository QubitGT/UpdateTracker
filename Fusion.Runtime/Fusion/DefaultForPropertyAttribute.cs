using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class DefaultForPropertyAttribute : PropertyAttribute
{
	public string PropertyName { get; }

	public int WordOffset { get; }

	public int WordCount { get; }

	public DefaultForPropertyAttribute(string propertyName, int wordOffset, int wordCount)
	{
		PropertyName = propertyName;
		WordOffset = wordOffset;
		WordCount = wordCount;
	}
}
