using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct DampedTransformData : IAnimationJobData, IDampedTransformData
{
	[SerializeField]
	private Transform m_ConstrainedObject;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_Source;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_DampPosition;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_DampRotation;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainAim;

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
			return m_Source;
		}
		set
		{
			m_Source = value;
		}
	}

	public float dampPosition
	{
		get
		{
			return m_DampPosition;
		}
		set
		{
			m_DampPosition = Mathf.Clamp01(value);
		}
	}

	public float dampRotation
	{
		get
		{
			return m_DampRotation;
		}
		set
		{
			m_DampRotation = Mathf.Clamp01(value);
		}
	}

	public bool maintainAim
	{
		get
		{
			return m_MaintainAim;
		}
		set
		{
			m_MaintainAim = value;
		}
	}

	string IDampedTransformData.dampPositionFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_DampPosition");

	string IDampedTransformData.dampRotationFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_DampRotation");

	bool IAnimationJobData.IsValid()
	{
		if (!(m_ConstrainedObject == null))
		{
			return !(m_Source == null);
		}
		return false;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_ConstrainedObject = null;
		m_Source = null;
		m_DampPosition = 0.5f;
		m_DampRotation = 0.5f;
		m_MaintainAim = true;
	}
}
