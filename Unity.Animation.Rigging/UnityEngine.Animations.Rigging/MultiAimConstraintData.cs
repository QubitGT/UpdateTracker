using System;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public struct MultiAimConstraintData : IAnimationJobData, IMultiAimConstraintData
{
	public enum Axis
	{
		X,
		X_NEG,
		Y,
		Y_NEG,
		Z,
		Z_NEG
	}

	public enum WorldUpType
	{
		None,
		SceneUp,
		ObjectUp,
		ObjectRotationUp,
		Vector
	}

	internal const float k_MinAngularLimit = -180f;

	internal const float k_MaxAngularLimit = 180f;

	[SerializeField]
	private Transform m_ConstrainedObject;

	[SyncSceneToStream]
	[SerializeField]
	[WeightRange(0f, 1f)]
	private WeightedTransformArray m_SourceObjects;

	[SyncSceneToStream]
	[SerializeField]
	private Vector3 m_Offset;

	[SyncSceneToStream]
	[SerializeField]
	[Range(-180f, 180f)]
	private float m_MinLimit;

	[SyncSceneToStream]
	[SerializeField]
	[Range(-180f, 180f)]
	private float m_MaxLimit;

	[NotKeyable]
	[SerializeField]
	private Axis m_AimAxis;

	[NotKeyable]
	[SerializeField]
	private Axis m_UpAxis;

	[NotKeyable]
	[SerializeField]
	private WorldUpType m_WorldUpType;

	[SyncSceneToStream]
	[SerializeField]
	private Transform m_WorldUpObject;

	[NotKeyable]
	[SerializeField]
	private Axis m_WorldUpAxis;

	[NotKeyable]
	[SerializeField]
	private bool m_MaintainOffset;

	[NotKeyable]
	[SerializeField]
	private Vector3Bool m_ConstrainedAxes;

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

	public Vector2 limits
	{
		get
		{
			return new Vector2(m_MinLimit, m_MaxLimit);
		}
		set
		{
			m_MinLimit = Mathf.Clamp(value.x, -180f, 180f);
			m_MaxLimit = Mathf.Clamp(value.y, -180f, 180f);
		}
	}

	public Axis aimAxis
	{
		get
		{
			return m_AimAxis;
		}
		set
		{
			m_AimAxis = value;
		}
	}

	public Axis upAxis
	{
		get
		{
			return m_UpAxis;
		}
		set
		{
			m_UpAxis = value;
		}
	}

	public WorldUpType worldUpType
	{
		get
		{
			return m_WorldUpType;
		}
		set
		{
			m_WorldUpType = value;
		}
	}

	public Axis worldUpAxis
	{
		get
		{
			return m_WorldUpAxis;
		}
		set
		{
			m_WorldUpAxis = value;
		}
	}

	public Transform worldUpObject
	{
		get
		{
			return m_WorldUpObject;
		}
		set
		{
			m_WorldUpObject = value;
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

	Vector3 IMultiAimConstraintData.aimAxis => Convert(m_AimAxis);

	Vector3 IMultiAimConstraintData.upAxis => Convert(m_UpAxis);

	int IMultiAimConstraintData.worldUpType => (int)m_WorldUpType;

	Vector3 IMultiAimConstraintData.worldUpAxis => Convert(m_WorldUpAxis);

	string IMultiAimConstraintData.offsetVector3Property => ConstraintsUtils.ConstructConstraintDataPropertyName("m_Offset");

	string IMultiAimConstraintData.minLimitFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_MinLimit");

	string IMultiAimConstraintData.maxLimitFloatProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_MaxLimit");

	string IMultiAimConstraintData.sourceObjectsProperty => ConstraintsUtils.ConstructConstraintDataPropertyName("m_SourceObjects");

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
		m_UpAxis = Axis.Y;
		m_AimAxis = Axis.Z;
		m_WorldUpType = WorldUpType.None;
		m_WorldUpAxis = Axis.Y;
		m_WorldUpObject = null;
		m_SourceObjects.Clear();
		m_MaintainOffset = false;
		m_Offset = Vector3.zero;
		m_ConstrainedAxes = new Vector3Bool(val: true);
		m_MinLimit = -180f;
		m_MaxLimit = 180f;
	}

	private static Vector3 Convert(Axis axis)
	{
		return axis switch
		{
			Axis.X => Vector3.right, 
			Axis.X_NEG => Vector3.left, 
			Axis.Y => Vector3.up, 
			Axis.Y_NEG => Vector3.down, 
			Axis.Z => Vector3.forward, 
			Axis.Z_NEG => Vector3.back, 
			_ => Vector3.up, 
		};
	}
}
