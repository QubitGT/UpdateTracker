using System;
using System.Diagnostics;
using UnityEngine;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property)]
[Conditional("FUSION_UNITY")]
[Conditional("UNITY_EDITOR")]
[Conditional("UNITY_2020_1_OR_NEWER")]
[UnityPropertyAttributeProxy(typeof(SpaceAttribute))]
public sealed class UnitySpaceAttribute : Attribute
{
	public int order { get; set; }

	public UnitySpaceAttribute(float space)
	{
	}
}
