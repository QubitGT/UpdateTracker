using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct MultiPositionConstraintData : IAnimationJobData, IMultiPositionConstraintData
{
	[SerializeField]
	private Transform m_ConstrainedObject;

	[SyncSceneToStream]
	[SerializeField]
	[WeightRange(0f, 1f)]
	private WeightedTransformArray m_SourceObjects;

	[SyncSceneToStream]
	[SerializeField]
	private Vector3 m_Offset;

	[NotKeyable]
	[SerializeField]
	private Vector3Bool m_ConstrainedAxes;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainOffset;

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

	public WeightedTransformArray sourceObjects
	{
		get
		{
			return m_SourceObjects;
		}
		set
		{
			m_SourceObjects = value;
		}
	}

	public bool maintainOffset
	{
		get
		{
			return m_MaintainOffset;
		}
		set
		{
			m_MaintainOffset = value;
		}
	}

	public Vector3 offset
	{
		get
		{
			return m_Offset;
		}
		set
		{
			m_Offset = value;
		}
	}

	public bool constrainedXAxis
	{
		get
		{
			return m_ConstrainedAxes.x;
		}
		set
		{
			m_ConstrainedAxes.x = value;
		}
	}

	public bool constrainedYAxis
	{
		get
		{
			return m_ConstrainedAxes.y;
		}
		set
		{
			m_ConstrainedAxes.y = value;
		}
	}

	public bool constrainedZAxis
	{
		get
		{
			return m_ConstrainedAxes.z;
		}
		set
		{
			m_ConstrainedAxes.z = value;
		}
	}

	string IMultiPositionConstraintData.offsetVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName("m_Offset");

	string IMultiPositionConstraintData.sourceObjectsProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_SourceObjects");

	bool IAnimationJobData.IsValid()
	{
		if (m_ConstrainedObject == null || m_SourceObjects.Count == 0)
		{
			return false;
		}
		foreach (WeightedTransform sourceObject in m_SourceObjects)
		{
			if (sourceObject.transform == null)
			{
				return false;
			}
		}
		return true;
	}

	void IAnimationJobData.SetDefaultValues()
	{
		m_ConstrainedObject = null;
		m_ConstrainedAxes = new Vector3Bool(val: true);
		m_SourceObjects.Clear();
		m_MaintainOffset = false;
		m_Offset = Vector3.zero;
	}
}
