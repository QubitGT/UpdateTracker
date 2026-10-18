using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class RegisterAssetReferenceAttributeForwardToChildAttribute : Attribute
{
	public readonly Type AttributeType;

	public RegisterAssetReferenceAttributeForwardToChildAttribute(Type attributeType)
	{
		AttributeType = attributeType;
	}
}
