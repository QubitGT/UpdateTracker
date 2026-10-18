using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct MultiParentConstraintData : IAnimationJobData, IMultiParentConstraintData
{
	[SerializeField]
	private Transform m_ConstrainedObject;

	[SerializeField]
	[SyncSceneToStream]
	[WeightRange(0f, 1f)]
	private WeightedTransformArray m_SourceObjects;

	[NotKeyable]
	[SerializeField]
	private Vector3Bool m_ConstrainedPositionAxes;

	[NotKeyable]
	[SerializeField]
	private Vector3Bool m_ConstrainedRotationAxes;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainPositionOffset;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainRotationOffset;

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

	public bool maintainPositionOffset
	{
		get
		{
			return m_MaintainPositionOffset;
		}
		set
		{
			m_MaintainPositionOffset = value;
		}
	}

	public bool maintainRotationOffset
	{
		get
		{
			return m_MaintainRotationOffset;
		}
		set
		{
			m_MaintainRotationOffset = value;
		}
	}

	public bool constrainedPositionXAxis
	{
		get
		{
			return m_ConstrainedPositionAxes.x;
		}
		set
		{
			m_ConstrainedPositionAxes.x = value;
		}
	}

	public bool constrainedPositionYAxis
	{
		get
		{
			return m_ConstrainedPositionAxes.y;
		}
		set
		{
			m_ConstrainedPositionAxes.y = value;
		}
	}

	public bool constrainedPositionZAxis
	{
		get
		{
			return m_ConstrainedPositionAxes.z;
		}
		set
		{
			m_ConstrainedPositionAxes.z = value;
		}
	}

	public bool constrainedRotationXAxis
	{
		get
		{
			return m_ConstrainedRotationAxes.x;
		}
		set
		{
			m_ConstrainedRotationAxes.x = value;
		}
	}

	public bool constrainedRotationYAxis
	{
		get
		{
			return m_ConstrainedRotationAxes.y;
		}
		set
		{
			m_ConstrainedRotationAxes.y = value;
		}
	}

	public bool constrainedRotationZAxis
	{
		get
		{
			return m_ConstrainedRotationAxes.z;
		}
		set
		{
			m_ConstrainedRotationAxes.z = value;
		}
	}

	string IMultiParentConstraintData.sourceObjectsProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_SourceObjects");

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
		m_ConstrainedPositionAxes = new Vector3Bool(val: true);
		m_ConstrainedRotationAxes = new Vector3Bool(val: true);
		m_SourceObjects.Clear();
		m_MaintainPositionOffset = false;
		m_MaintainRotationOffset = false;
	}
}
