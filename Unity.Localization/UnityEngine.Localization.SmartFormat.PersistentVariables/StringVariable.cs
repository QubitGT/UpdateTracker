using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("String", null)]
public class StringVariable : Variable<string>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("String", null)]
	public new class UxmlSerializedData : Variable<string>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new StringVariable();
		}
	}
}
