using System;

namespace Meta.Conduit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HandleEntityResolutionFailure : Attribute
{
}
