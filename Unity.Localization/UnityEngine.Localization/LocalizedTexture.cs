using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[Serializable]
[UxmlObject]
public class LocalizedTexture : LocalizedAsset<Texture>
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : LocalizedAsset<Texture>.UxmlSerializedData
	{
		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
		}

		public override object CreateInstance()
		{
			return new LocalizedTexture();
		}
	}

	protected override BindingResult ApplyDataBindingValue(in BindingContext context, Texture value)
	{
		if (value is Texture2D value2)
		{
			return SetDataBindingValue(in context, value2);
		}
		return base.ApplyDataBindingValue(in context, value);
	}
}
