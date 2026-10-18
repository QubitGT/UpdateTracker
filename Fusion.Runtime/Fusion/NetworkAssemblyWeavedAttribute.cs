using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = false)]
public sealed class NetworkAssemblyWeavedAttribute : Attribute
{
}
