using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class NetworkDeserializeMethodAttribute : Attribute
{
}
