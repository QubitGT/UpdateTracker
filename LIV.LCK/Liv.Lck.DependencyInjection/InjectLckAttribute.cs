using System;
using UnityEngine;

namespace Liv.Lck.DependencyInjection;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class InjectLckAttribute : PropertyAttribute
{
}
