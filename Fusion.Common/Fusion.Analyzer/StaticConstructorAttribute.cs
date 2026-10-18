using System;
using System.Diagnostics;

namespace Fusion.Analyzer;

[AttributeUsage(AttributeTargets.Constructor)]
[Conditional("false")]
public class StaticConstructorAttribute : Attribute
{
}
