using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[DontApplyToListElements]
[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use [DisableIn(PrefabKind.PrefabInstance)] instead.", false)]
public class DisableInPrefabInstancesAttribute : Attribute
{
}
