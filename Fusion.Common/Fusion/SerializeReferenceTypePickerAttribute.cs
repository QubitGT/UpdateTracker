using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public class SerializeReferenceTypePickerAttribute : DecoratingPropertyAttribute
{
	public bool GroupTypesByNamespace = true;

	public bool ShowFullName = false;

	public Type[] Types { get; private set; }

	public SerializeReferenceTypePickerAttribute(params Type[] types)
	{
		Types = types;
	}
}
