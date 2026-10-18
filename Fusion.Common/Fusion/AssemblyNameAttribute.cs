using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public class AssemblyNameAttribute : DrawerPropertyAttribute
{
	public bool RequiresUnsafeCode { get; set; }
}
