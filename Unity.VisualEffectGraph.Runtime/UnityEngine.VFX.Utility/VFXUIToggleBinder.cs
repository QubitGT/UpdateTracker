using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/UI Toggle Binder")]
[VFXBinder("UI/Toggle")]
internal class VFXUIToggleBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "BoolParameter";

	public Toggle Target;

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
			return component.HasBool(m_Property);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetBool(m_Property, Target.isOn);
	}

	public override string ToString()
	{
		return string.Format("UI Toggle : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
