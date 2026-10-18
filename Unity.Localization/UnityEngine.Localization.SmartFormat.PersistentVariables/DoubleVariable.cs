using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Double", null)]
public class DoubleVariable : Variable<double>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Double", null)]
	public new class UxmlSerializedData : Variable<double>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new DoubleVariable();
		}
	}
}
