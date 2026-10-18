using System;

namespace Unity.Burst.CompilerServices;

[AttributeUsage(AttributeTargets.Method)]
public class SkipLocalsInitAttribute : Attribute
{
}
