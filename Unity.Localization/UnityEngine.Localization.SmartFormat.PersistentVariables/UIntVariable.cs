using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Unsigned Integer", null)]
public class UIntVariable : Variable<uint>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Unsigned Integer", null)]
	public new class UxmlSerializedData : Variable<uint>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new UIntVariable();
		}
	}
}
