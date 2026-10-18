using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct OverrideTransformData : IAnimationJobData, IOverrideTransformData
{
	[Serializable]
	public enum Space
	{
		World,
		Local,
		Pivot
	}

	[SerializeField]
	private Transform m_ConstrainedObject;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_OverrideSource;

	[SyncSceneToStream]
	[SerializeField]
	private Vector3 m_OverridePosition;

	[SyncSceneToStream]
	[SerializeField]
	private Vector3 m_OverrideRotation;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_PositionWeight;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_RotationWeight;

	[NotKeyable]
	[SerializeField]
	private Space m_Space;

	public Transform constrainedObject
	{
		get
		{
			return m_ConstrainedObject;
		}
		set
		{
			m_ConstrainedObject = value;
		}
	}

	public Transform sourceObject
	{
		get
		{
			return m_OverrideSource;
		}
		set
		{
			m_OverrideSource = value;
		}
	}

	public Space space
	{
		get
		{
			return m_Space;
		}
		set
		{
			m_Space = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return m_OverridePosition;
		}
		set
		{
			m_OverridePosition = value;
		}
	}

	public Vector3 rotation
	{
		get
		{
			return m_OverrideRotation;
		}
		set
		{
			m_OverrideRotation = value;
		}
	}

	public float positionWeight
	{
		get
		{
			return m_PositionWeight;
		}
		set
		{
			m_PositionWeight = Mathf.Clamp01(value);
		}
	}

	public float rotationWeight
	{
		get
		{
			return m_RotationWeight;
		}
		set
		{
			m_RotationWeight = Mathf.Clamp01(value);
		}
	}

	int IOverrideTransformData.space => (int)m_Space;

	string IOverrideTransformData.positionWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_PositionWeight");

	string IOverrideTransformData.rotationWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_RotationWeight");

	string IOverrideTransformData.positionVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName("m_OverridePosition");

	string IOverrideTransformData.rotationVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName("m_OverrideRotation");

	bool IAnimationJobData.IsValid()
	{
		return m_ConstrainedObject != null;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_ConstrainedObject = null;
		m_OverrideSource = null;
		m_OverridePosition = Vector3.zero;
		m_OverrideRotation = Vector3.zero;
		m_Space = Space.Pivot;
		m_PositionWeight = 1f;
		m_RotationWeight = 1f;
	}
}
