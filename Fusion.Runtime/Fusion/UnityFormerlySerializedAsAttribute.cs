using System;
using System.Diagnostics;
using UnityEngine.Serialization;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("FUSION_UNITY")]
[Conditional("UNITY_EDITOR")]
[Conditional("UNITY_2020_1_OR_NEWER")]
[UnityPropertyAttributeProxy(typeof(FormerlySerializedAsAttribute))]
public sealed class UnityFormerlySerializedAsAttribute : Attribute
{
	public UnityFormerlySerializedAsAttribute(string oldName)
	{
	}
}
