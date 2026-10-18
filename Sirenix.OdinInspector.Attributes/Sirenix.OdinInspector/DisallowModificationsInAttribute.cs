using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector;

[Conditional("UNITY_EDITOR")]
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class DisallowModificationsInAttribute : Attribute
{
	public PrefabKind PrefabKind;

	public DisallowModificationsInAttribute(PrefabKind kind)
	{
		PrefabKind = kind;
	}
}
