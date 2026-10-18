namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Transform Binder")]
[VFXBinder("Transform/Transform")]
internal class VFXTransformBinder : VFXSpaceableBinder
{
	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Transform" })]
	[SerializeField]
	protected ExposedProperty m_Property = "Transform";

	public Transform Target;

	private ExposedProperty Position;

	private ExposedProperty Angles;

	private ExposedProperty Scale;

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
		Angles = m_Property + "_angles";
		Scale = m_Property + "_scale";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (Target != null && component.HasVector3(Position) && component.HasVector3(Angles))
		{
			return component.HasVector3(Scale);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		ApplySpaceTRS(component, Position, Target, out var position, out var eulerAngles, out var scale);
		component.SetVector3(Position, position);
		component.SetVector3(Angles, eulerAngles);
		component.SetVector3(Scale, scale);
	}

	public override string ToString()
	{
		return string.Format("Transform : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
