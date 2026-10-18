using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class DisplayNameAttribute : DecoratingPropertyAttribute
{
	public readonly string Name;

	public DisplayNameAttribute(string name)
	{
		Name = name;
	}
}
