using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct BlendConstraintData : IAnimationJobData, IBlendConstraintData
{
	[SerializeField]
	private Transform m_ConstrainedObject;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_SourceA;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_SourceB;

	[SyncSceneToStream]
	[SerializeField]
	private bool m_BlendPosition;

	[SyncSceneToStream]
	[SerializeField]
	private bool m_BlendRotation;

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
	private bool m_MaintainPositionOffsets;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainRotationOffsets;

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

	public Transform sourceObjectA
	{
		get
		{
			return m_SourceA;
		}
		set
		{
			m_SourceA = value;
		}
	}

	public Transform sourceObjectB
	{
		get
		{
			return m_SourceB;
		}
		set
		{
			m_SourceB = value;
		}
	}

	public bool blendPosition
	{
		get
		{
			return m_BlendPosition;
		}
		set
		{
			m_BlendPosition = value;
		}
	}

	public bool blendRotation
	{
		get
		{
			return m_BlendRotation;
		}
		set
		{
			m_BlendRotation = value;
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

	public bool maintainPositionOffsets
	{
		get
		{
			return m_MaintainPositionOffsets;
		}
		set
		{
			m_MaintainPositionOffsets = value;
		}
	}

	public bool maintainRotationOffsets
	{
		get
		{
			return m_MaintainRotationOffsets;
		}
		set
		{
			m_MaintainRotationOffsets = value;
		}
	}

	string IBlendConstraintData.blendPositionBoolProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_BlendPosition");

	string IBlendConstraintData.blendRotationBoolProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_BlendRotation");

	string IBlendConstraintData.positionWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_PositionWeight");

	string IBlendConstraintData.rotationWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_RotationWeight");

	bool IAnimationJobData.IsValid()
	{
		if (!(m_ConstrainedObject == null) && !(m_SourceA == null))
		{
			return !(m_SourceB == null);
		}
		return false;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_ConstrainedObject = null;
		m_SourceA = null;
		m_SourceB = null;
		m_BlendPosition = true;
		m_BlendRotation = true;
		m_PositionWeight = 0.5f;
		m_RotationWeight = 0.5f;
		m_MaintainPositionOffsets = false;
		m_MaintainRotationOffsets = false;
	}
}
