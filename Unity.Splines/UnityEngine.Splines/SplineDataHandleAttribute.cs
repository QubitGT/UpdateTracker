using System;

namespace UnityEngine.Splines;

[Obsolete("Use SplineDataHandles.DataPointHandles instead and EditorTools to interact with SplineData.", false)]
[AttributeUsage(AttributeTargets.Field)]
public abstract class SplineDataHandleAttribute : Attribute
{
}
