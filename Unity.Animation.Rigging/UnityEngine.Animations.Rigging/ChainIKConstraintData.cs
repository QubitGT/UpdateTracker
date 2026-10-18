using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct ChainIKConstraintData : IAnimationJobData, IChainIKConstraintData
{
	internal const int k_MinIterations = 1;

	internal const int k_MaxIterations = 50;

	internal const float k_MinTolerance = 0f;

	internal const float k_MaxTolerance = 0.01f;

	[SerializeField]
	private Transform m_Root;

	[SerializeField]
	private Transform m_Tip;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_Target;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_ChainRotationWeight;

	[SyncSceneToStream]
	[SerializeField]
	[Range(0f, 1f)]
	private float m_TipRotationWeight;

	[NotKeyable]
	[SerializeField]
	[Range(1f, 50f)]
	private int m_MaxIterations;

	[NotKeyable]
	[SerializeField]
	[Range(0f, 0.01f)]
	private float m_Tolerance;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainTargetPositionOffset;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainTargetRotationOffset;

	public Transform root
	{
		get
		{
			return m_Root;
		}
		set
		{
			m_Root = value;
		}
	}

	public Transform tip
	{
		get
		{
			return m_Tip;
		}
		set
		{
			m_Tip = value;
		}
	}

	public Transform target
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
		}
	}

	public float chainRotationWeight
	{
		get
		{
			return m_ChainRotationWeight;
		}
		set
		{
			m_ChainRotationWeight = Mathf.Clamp01(value);
		}
	}

	public float tipRotationWeight
	{
		get
		{
			return m_TipRotationWeight;
		}
		set
		{
			m_TipRotationWeight = Mathf.Clamp01(value);
		}
	}

	public int maxIterations
	{
		get
		{
			return m_MaxIterations;
		}
		set
		{
			m_MaxIterations = Mathf.Clamp(value, 1, 50);
		}
	}

	public float tolerance
	{
		get
		{
			return m_Tolerance;
		}
		set
		{
			m_Tolerance = Mathf.Clamp(value, 0f, 0.01f);
		}
	}

	public bool maintainTargetPositionOffset
	{
		get
		{
			return m_MaintainTargetPositionOffset;
		}
		set
		{
			m_MaintainTargetPositionOffset = value;
		}
	}

	public bool maintainTargetRotationOffset
	{
		get
		{
			return m_MaintainTargetRotationOffset;
		}
		set
		{
			m_MaintainTargetRotationOffset = value;
		}
	}

	string IChainIKConstraintData.chainRotationWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_ChainRotationWeight");

	string IChainIKConstraintData.tipRotationWeightFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_TipRotationWeight");

	bool IAnimationJobData.IsValid()
	{
		if (m_Root == null || m_Tip == null || m_Target == null)
		{
			return false;
		}
		int num = 1;
		Transform parent = m_Tip;
		while (parent != null && parent != m_Root)
		{
			parent = parent.parent;
			num++;
		}
		if (parent == m_Root)
		{
			return num > 2;
		}
		return false;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_Root = null;
		m_Tip = null;
		m_Target = null;
		m_ChainRotationWeight = 1f;
		m_TipRotationWeight = 1f;
		m_MaxIterations = 15;
		m_Tolerance = 0.0001f;
		m_MaintainTargetPositionOffset = false;
		m_MaintainTargetRotationOffset = false;
	}
}
