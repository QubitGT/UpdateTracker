using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables;

[Serializable]
[UxmlObject]
[DisplayName("Object Reference", null)]
public class ObjectVariable : Variable<Object>
{
	[Serializable]
	[CompilerGenerated]
	[DisplayName("Object Reference", null)]
	public new class UxmlSerializedData : Variable<Object>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new ObjectVariable();
		}
	}
}
