namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Position Binder")]
[VFXBinder("Transform/Position")]
internal class VFXPositionBinder : VFXSpaceableBinder
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Position", "UnityEngine.Vector3" })]
	[SerializeField]
	protected ExposedProperty m_Property = "Position";

	public Transform Target;

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
			return component.HasVector3(m_Property);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Vector3 v = ApplySpacePosition(component, m_Property, Target.transform.position);
		component.SetVector3(m_Property, v);
	}

	public override string ToString()
	{
		return string.Format("Position : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
