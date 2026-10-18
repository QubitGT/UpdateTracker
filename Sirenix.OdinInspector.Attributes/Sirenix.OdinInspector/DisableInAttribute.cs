using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[DontApplyToListElements]
[AttributeUsage(AttributeTargets.All)]
[Conditional("UNITY_EDITOR")]
public class DisableInAttribute : Attribute
{
	public PrefabKind PrefabKind;

	public DisableInAttribute(PrefabKind prefabKind)
	{
		PrefabKind = prefabKind;
	}
}
