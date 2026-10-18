using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Boolean", null)]
public class BoolVariable : Variable<bool>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Boolean", null)]
	public new class UxmlSerializedData : Variable<bool>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new BoolVariable();
		}
	}
}
