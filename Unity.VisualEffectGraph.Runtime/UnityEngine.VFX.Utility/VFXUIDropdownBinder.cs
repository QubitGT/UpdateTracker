using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/UI Dropdown Binder")]
[VFXBinder("UI/Dropdown")]
internal class VFXUIDropdownBinder : VFXBinderBase
{
	[VFXPropertyBinding(new string[] { "System.Int32" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "IntParameter";

	public Dropdown Target;

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
			return component.HasInt(m_Property);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		component.SetInt(m_Property, Target.value);
	}

	public override string ToString()
	{
		return string.Format("UI Dropdown : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
