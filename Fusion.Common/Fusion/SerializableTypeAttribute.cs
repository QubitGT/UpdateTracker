using System;

namespace Fusion;

public class SerializableTypeAttribute : PropertyAttribute
{
	public Type BaseType { get; set; }

	public bool UseFullAssemblyQualifiedName { get; set; }

	public bool WarnIfNoPreserveAttribute { get; set; }
}
