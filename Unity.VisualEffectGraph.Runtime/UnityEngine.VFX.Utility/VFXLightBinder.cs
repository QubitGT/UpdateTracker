using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Light Binder")]
[VFXBinder("Utility/Light")]
internal class VFXLightBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "UnityEngine.Color" })]
	[SerializeField]
	[FormerlySerializedAs("m_ColorParameter")]
	protected ExposedProperty m_ColorProperty = "Color";

	[VFXPropertyBinding(new string[] { "System.Single" })]
	[SerializeField]
	[FormerlySerializedAs("m_BrightnessParameter")]
	protected ExposedProperty m_BrightnessProperty = "Brightness";

	[VFXPropertyBinding(new string[] { "System.Single" })]
	[SerializeField]
	[FormerlySerializedAs("m_RadiusParameter")]
	protected ExposedProperty m_RadiusProperty = "Radius";

	public Light Target;

	public bool BindColor = true;

	public bool BindBrightness;

	public bool BindRadius;

	public string ColorProperty
	{
		get
		{
			return (string)m_ColorProperty;
		}
		set
		{
			m_ColorProperty = value;
		}
	}

	public string BrightnessProperty
	{
		get
		{
			return (string)m_BrightnessProperty;
		}
		set
		{
			m_ColorProperty = value;
		}
	}

	public string RadiusProperty
	{
		get
		{
			return (string)m_RadiusProperty;
		}
		set
		{
			m_RadiusProperty = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && (!BindColor || component.HasVector4(ColorProperty)) && (!BindBrightness || component.HasFloat(BrightnessProperty)))
		{
			if (BindRadius)
			{
				return component.HasFloat(RadiusProperty);
			}
			return true;
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		if (BindColor)
		{
			component.SetVector4(ColorProperty, Target.color);
		}
		if (BindBrightness)
		{
			component.SetFloat(BrightnessProperty, Target.intensity);
		}
		if (BindRadius)
		{
			component.SetFloat(RadiusProperty, Target.range);
		}
	}

	public override string ToString()
	{
		return string.Format("Light : '{0}' -> {1}", m_ColorProperty, (Target == null) ? "(null)" : Target.name);
	}
}
