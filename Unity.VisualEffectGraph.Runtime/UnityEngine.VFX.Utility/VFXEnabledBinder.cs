using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Enabled Binder")]
[VFXBinder("GameObject/Enabled")]
internal class VFXEnabledBinder : VFXBinderBase
{
	public enum Check
	{
		ActiveInHierarchy,
		ActiveSelf
	}

	public Check check;

	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "Enabled";

	public GameObject Target;

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
		component.SetBool(m_Property, (check == Check.ActiveInHierarchy) ? Target.activeInHierarchy : Target.activeSelf);
	}

	public override string ToString()
	{
		return string.Format("{2} : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name, check);
	}
}
