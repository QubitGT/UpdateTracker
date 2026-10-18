using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class ArrayLengthAttribute : DecoratingPropertyAttribute
{
	public int MinLength { get; }

	public int MaxLength { get; }

	public ArrayLengthAttribute(int length)
	{
		MinLength = (MaxLength = length);
	}

	public ArrayLengthAttribute(int minLength, int maxLength)
	{
		MinLength = minLength;
		MaxLength = maxLength;
	}
}
