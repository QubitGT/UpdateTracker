using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Float", null)]
public class FloatVariable : Variable<float>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Float", null)]
	public new class UxmlSerializedData : Variable<float>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new FloatVariable();
		}
	}
}
