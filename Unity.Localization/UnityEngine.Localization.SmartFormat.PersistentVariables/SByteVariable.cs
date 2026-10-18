using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Signed Byte", null)]
public class SByteVariable : Variable<sbyte>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Signed Byte", null)]
	public new class UxmlSerializedData : Variable<sbyte>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new SByteVariable();
		}
	}
}
