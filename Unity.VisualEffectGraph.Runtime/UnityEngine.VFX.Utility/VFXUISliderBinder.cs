using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/UI Slider Binder")]
[VFXBinder("UI/Slider")]
internal class VFXUISliderBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Single" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "FloatParameter";

	public Slider Target;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = value;
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null)
		{
			return component.HasFloat(m_Property);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetFloat(m_Property, Target.value);
	}

	public override string ToString()
	{
		return string.Format("UI Slider : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
