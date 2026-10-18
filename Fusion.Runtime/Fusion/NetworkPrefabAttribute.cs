using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class NetworkPrefabAttribute : PropertyAttribute
{
}
