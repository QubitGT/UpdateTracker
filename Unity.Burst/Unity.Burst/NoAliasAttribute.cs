using System;

namespace Unity.Burst;

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class NoAliasAttribute : Attribute
{
}
