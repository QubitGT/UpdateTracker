using UnityEngine.Serialization;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Sphere Collider Binder")]
[VFXBinder("Collider/Sphere")]
internal class VFXSphereBinder : VFXSpaceableBinder
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Sphere", "UnityEditor.VFX.TSphere" })]
	[SerializeField]
	[FormerlySerializedAs("m_Parameter")]
	protected ExposedProperty m_Property = "Sphere";

	public SphereCollider Target;

	private ExposedProperty m_Old_Center;

	private ExposedProperty m_New_Center;

	private ExposedProperty m_Radius;

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
		m_Old_Center = m_Property + "_center";
		m_New_Center = m_Property + "_transform_position";
		m_Radius = m_Property + "_radius";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && (component.HasVector3(m_New_Center) || component.HasVector3(m_Old_Center)))
		{
			return component.HasFloat(m_Radius);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		ExposedProperty exposedProperty = m_New_Center;
		if (!component.HasVector3(m_New_Center))
		{
			exposedProperty = m_Old_Center;
		}
		ApplySpaceTS(component, exposedProperty, Target.transform, out var position, out var scale);
		Vector3 v = position + Target.center;
		if (m_New_Center == exposedProperty)
		{
			component.SetVector3(m_New_Center, v);
		}
		else
		{
			component.SetVector3(m_Old_Center, v);
		}
		component.SetFloat(m_Radius, Target.radius * GetSphereColliderScale(scale));
	}

	public float GetSphereColliderScale(Vector3 scale)
	{
		return Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
	}

	public override string ToString()
	{
		return string.Format("Sphere : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
