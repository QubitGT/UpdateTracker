using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Short", null)]
public class ShortVariable : Variable<short>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Short", null)]
	public new class UxmlSerializedData : Variable<short>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new ShortVariable();
		}
	}
}
