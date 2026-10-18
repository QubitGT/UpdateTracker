using System;
using System.Diagnostics;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("FUSION_UNITY")]
[Conditional("UNITY_EDITOR")]
[Conditional("UNITY_2020_1_OR_NEWER")]
[UnityPropertyAttributeProxy(typeof(NonSerializedAttribute))]
public sealed class UnityNonSerializedAttribute : Attribute
{
}
