using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct TwistChainConstraintData : IAnimationJobData, ITwistChainConstraintData
{
	[SerializeField]
	private Transform m_Root;

	[SerializeField]
	private Transform m_Tip;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_RootTarget;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_TipTarget;

	[SerializeField]
	private AnimationCurve m_Curve;

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

	public Transform rootTarget
	{
		get
		{
			return m_RootTarget;
		}
		set
		{
			m_RootTarget = value;
		}
	}

	public Transform tipTarget
	{
		get
		{
			return m_TipTarget;
		}
		set
		{
			m_TipTarget = value;
		}
	}

	public AnimationCurve curve
	{
		get
		{
			return m_Curve;
		}
		set
		{
			m_Curve = value;
		}
	}

	bool IAnimationJobData.IsValid()
	{
		if (!(root == null) && !(tip == null) && tip.IsChildOf(root) && !(rootTarget == null) && !(tipTarget == null))
		{
			return curve != null;
		}
		return false;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		Transform transform2 = (tipTarget = null);
		Transform transform4 = (rootTarget = transform2);
		Transform transform6 = (tip = transform4);
		root = transform6;
		curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	}
}
