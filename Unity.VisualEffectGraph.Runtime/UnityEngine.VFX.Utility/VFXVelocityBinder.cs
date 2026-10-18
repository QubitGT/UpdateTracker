namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Velocity Binder")]
[VFXBinder("Transform/Velocity")]
internal class VFXVelocityBinder : VFXSpaceableBinder
{
	[VFXPropertyBinding(new string[] { "UnityEngine.Vector3" })]
	[SerializeField]
	public ExposedProperty m_Property = "Velocity";

	public Transform Target;

	private static readonly float invalidPreviousTime = -1f;

	private float m_PreviousTime = invalidPreviousTime;

	private Vector3 m_PreviousPosition = Vector3.zero;

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

	public override void Reset()
	{
		m_PreviousTime = invalidPreviousTime;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Vector3 v = Vector3.zero;
		float time = Time.time;
		Vector3 vector = ApplySpacePosition(component, m_Property, Target.position);
		if (m_PreviousTime != invalidPreviousTime)
		{
			Vector3 vector2 = vector - m_PreviousPosition;
			float num = time - m_PreviousTime;
			if (Vector3.SqrMagnitude(vector2) > Mathf.Epsilon && num > Mathf.Epsilon)
			{
				v = vector2 / num;
			}
		}
		component.SetVector3(m_Property, v);
		m_PreviousPosition = vector;
		m_PreviousTime = time;
	}

	public override string ToString()
	{
		return string.Format("Velocity : '{0}' -> {1}", m_Property, (Target == null) ? "(null)" : Target.name);
	}
}
