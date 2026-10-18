using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ReadOnlyAttribute : DecoratingPropertyAttribute
{
	public bool InPlayMode { get; set; } = true;


	public bool InEditMode { get; set; } = true;

}
