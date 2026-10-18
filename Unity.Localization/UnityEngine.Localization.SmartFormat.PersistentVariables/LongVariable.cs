using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Long", null)]
public class LongVariable : Variable<long>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Long", null)]
	public new class UxmlSerializedData : Variable<long>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new LongVariable();
		}
	}
}
