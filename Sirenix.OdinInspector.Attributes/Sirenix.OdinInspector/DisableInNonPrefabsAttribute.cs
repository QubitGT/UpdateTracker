using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[DontApplyToListElements]
[AttributeUsage(AttributeTargets.All)]
[EditorBrowsable(EditorBrowsableState.Never)]
[Conditional("UNITY_EDITOR")]
[Obsolete("Use [DisableIn(PrefabKind.NonPrefabInstance)] instead.", false)]
public class DisableInNonPrefabsAttribute : Attribute
{
}
