using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[Serializable]
[UxmlObject]
public class LocalizedGameObject : LocalizedAsset<GameObject>
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : LocalizedAsset<GameObject>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new LocalizedGameObject();
		}
	}
}
