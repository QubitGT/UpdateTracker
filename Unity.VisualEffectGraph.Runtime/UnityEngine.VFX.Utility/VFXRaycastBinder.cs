namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Raycast Binder")]
[VFXBinder("Physics/Raycast")]
internal class VFXRaycastBinder : VFXBinderBase
{
	public enum Space
	{
		Local,
		World
	}

	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.Position" })]
	[SerializeField]
	protected ExposedProperty m_TargetPosition = "TargetPosition";

	[VFXPropertyBinding(new string[] { "UnityEditor.VFX.DirectionType" })]
	[SerializeField]
	protected ExposedProperty m_TargetNormal = "TargetNormal";

	[VFXPropertyBinding(new string[] { "System.Boolean" })]
	[SerializeField]
	protected ExposedProperty m_TargetHit = "TargetHit";

	protected ExposedProperty m_TargetPosition_position;

	protected ExposedProperty m_TargetNormal_direction;

	public GameObject RaycastSource;

	public Vector3 RaycastDirection = Vector3.forward;

	public Space RaycastDirectionSpace;

	public LayerMask Layers = -1;

	public float MaxDistance = 100f;

	private RaycastHit m_HitInfo;

	public string TargetPosition
	{
		get
		{
			return (string)m_TargetPosition;
		}
		set
		{
			m_TargetPosition = value;
			UpdateSubProperties();
		}
	}

	public string TargetNormal
	{
		get
		{
			return (string)m_TargetNormal;
		}
		set
		{
			m_TargetNormal = value;
			UpdateSubProperties();
		}
	}

	public string TargetHit
	{
		get
		{
			return (string)m_TargetHit;
		}
		set
		{
			m_TargetHit = value;
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
		m_TargetPosition_position = m_TargetPosition + "_position";
		m_TargetNormal_direction = m_TargetNormal + "_direction";
	}

	public override bool IsValid(VisualEffect component)
	{
		if (component.HasVector3(m_TargetPosition_position) && component.HasVector3(m_TargetNormal_direction) && component.HasBool(m_TargetHit))
		{
			return RaycastSource != null;
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		Vector3 direction = ((RaycastDirectionSpace == Space.Local) ? RaycastSource.transform.TransformDirection(RaycastDirection) : RaycastDirection);
		bool b = Physics.Raycast(new Ray(RaycastSource.transform.position, direction), out m_HitInfo, MaxDistance, Layers);
		component.SetVector3(m_TargetPosition_position, m_HitInfo.point);
		component.SetVector3(m_TargetNormal_direction, m_HitInfo.normal);
		component.SetBool(TargetHit, b);
	}

	public override string ToString()
	{
		return string.Format(string.Format("Raycast : {0} -> {1} ({2})", (RaycastSource == null) ? "null" : RaycastSource.name, RaycastDirection, RaycastDirectionSpace));
	}
}
