using System;
using System.Diagnostics;

namespace Fusion.Runtime.Unity;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
[Conditional("FUSION_UNITY")]
internal class UnityDummyAttribute : Attribute
{
	public UnityDummyAttribute()
	{
	}

	public UnityDummyAttribute(string str)
	{
	}
}
