using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Plane Binder")]
[VFXBinder("Utility/Plane")]
internal class VFXPlaneBinder : VFXSpaceableBinder
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Plane" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "Plane";

	public Transform Target;

	private ExposedProperty Position;

	private ExposedProperty Normal;

	public string Property
	{
		get
		{
			return (string)m_Property;
		}
		set
		{
			m_Property = value;
			UpdateSubProperties();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateSubProperties();
	}

	private void OnValidate()
	{
		UpdateSubProperties();
	}

	private void UpdateSubProperties()
	{
		Position = m_Property + "_position";
		Normal = m_Property + "_normal";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && component.HasVector3(Position))
		{
			return component.HasVector3(Normal);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		ApplySpacePositionNormal(component, Position, Target.transform, out var position, out var normal);
		component.SetVector3(Position, position);
		component.SetVector3(Normal, normal);
	}

	public override string ToString()
	{
		return string.Format("Plane : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
