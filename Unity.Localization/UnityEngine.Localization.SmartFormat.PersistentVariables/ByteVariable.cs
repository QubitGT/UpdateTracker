using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Byte", null)]
public class ByteVariable : Variable<byte>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Byte", null)]
	public new class UxmlSerializedData : Variable<byte>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new ByteVariable();
		}
	}
}
