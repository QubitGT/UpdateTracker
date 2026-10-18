using System;
using UnityEngine.Serialization;

namespace UnityEngine.Animations.Rigging;

[Serializable]
public class OverrideRigLayer : IRigLayer
{
	[SerializeField]
	[FormerlySerializedAs("rig")]
	private Rig m_Rig;

	[SerializeField]
	[FormerlySerializedAs("active")]
	private bool m_Active = true;

	private IRigConstraint[] m_Constraints;

	private IAnimationJob[] m_Jobs;

	public Rig rig
	{
		get
		{
			return m_Rig;
		}
		private set
		{
			m_Rig = value;
		}
	}

	public bool active
	{
		get
		{
			return m_Active;
		}
		set
		{
			m_Active = value;
		}
	}

	public string name
	{
		get
		{
			if (!(rig != null))
			{
				return "no-name";
			}
			return rig.gameObject.name;
		}
	}

	public IRigConstraint[] constraints
	{
		get
		{
			if (!isInitialized)
			{
				return null;
			}
			return m_Constraints;
		}
	}

	public IAnimationJob[] jobs
	{
		get
		{
			if (!isInitialized)
			{
				return null;
			}
			return m_Jobs;
		}
	}

	public bool isInitialized { get; private set; }

	public OverrideRigLayer(Rig rig, IRigConstraint[] constraints, bool active = true)
	{
		this.rig = rig;
		this.active = active;
		m_Constraints = constraints;
	}

	public bool Initialize(Animator animator)
	{
		if (isInitialized)
		{
			return true;
		}
		if (rig == null)
		{
			return false;
		}
		if (m_Constraints == null || m_Constraints.Length == 0)
		{
			return false;
		}
		m_Jobs = new IAnimationJob[m_Constraints.Length];
		for (int i = 0; i < m_Constraints.Length; i++)
		{
			m_Jobs[i] = m_Constraints[i].CreateJob(animator);
		}
		return isInitialized = true;
	}

	public void Update()
	{
		if (isInitialized)
		{
			for (int i = 0; i < m_Constraints.Length; i++)
			{
				m_Constraints[i].UpdateJob(m_Jobs[i]);
			}
		}
	}

	public void Reset()
	{
		if (isInitialized)
		{
			int i = 0;
			for (int num = m_Constraints.Length; i < num; i++)
			{
				m_Constraints[i].DestroyJob(m_Jobs[i]);
			}
			m_Constraints = null;
			m_Jobs = null;
			isInitialized = false;
		}
	}

	public bool IsValid()
	{
		if (rig != null)
		{
			return isInitialized;
		}
		return false;
	}
}
