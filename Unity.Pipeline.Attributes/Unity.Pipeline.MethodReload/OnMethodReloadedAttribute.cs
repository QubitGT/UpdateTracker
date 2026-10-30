using System;
using JetBrains.Annotations;

namespace Unity.Pipeline.MethodReload;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[MeansImplicitUse]
public class OnMethodReloadedAttribute : Attribute
{
}
