using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct TwistCorrectionData : IAnimationJobData, ITwistCorrectionData
{
	public enum Axis
	{
		X,
		Y,
		Z
	}

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_Source;

	[NotKeyable]
	[SerializeField]
	private Axis m_TwistAxis;

	[SyncSceneToStream]
	[SerializeField]
	[WeightRange(-1f, 1f)]
	private WeightedTransformArray m_TwistNodes;

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

	public WeightedTransformArray twistNodes
	{
		get
		{
			return m_TwistNodes;
		}
		set
		{
			m_TwistNodes = value;
		}
	}

	public Axis twistAxis
	{
		get
		{
			return m_TwistAxis;
		}
		set
		{
			m_TwistAxis = value;
		}
	}

	Transform ITwistCorrectionData.source => m_Source;

	Vector3 ITwistCorrectionData.twistAxis => Convert(m_TwistAxis);

	string ITwistCorrectionData.twistNodesProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_TwistNodes");

	private static Vector3 Convert(Axis axis)
	{
		return axis switch
		{
			Axis.X => Vector3.right, 
			Axis.Y => Vector3.up, 
			_ => Vector3.forward, 
		};
	}

	bool IAnimationJobData.IsValid()
	{
		if (m_Source == null)
		{
			return false;
		}
		for (int i = 0; i < m_TwistNodes.Count; i++)
		{
			if (m_TwistNodes[i].transform == null)
			{
				return false;
			}
		}
		return true;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_Source = null;
		m_TwistAxis = Axis.Z;
		m_TwistNodes.Clear();
	}
}
