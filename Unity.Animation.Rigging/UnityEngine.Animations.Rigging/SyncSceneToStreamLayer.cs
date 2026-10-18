using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging;

internal class SyncSceneToStreamLayer
{
	public IAnimationJob job;

	private IAnimationJobData m_Data;

	private List<int> m_RigIndices;

	public bool isInitialized { get; private set; }

	public bool Initialize(Animator animator, IList<IRigLayer> layers)
	{
		if (isInitialized)
		{
			return true;
		}
		m_RigIndices = new List<int>(layers.Count);
		for (int i = 0; i < layers.Count; i++)
		{
			if (layers[i].IsValid())
			{
				m_RigIndices.Add(i);
			}
		}
		m_Data = RigUtils.CreateSyncSceneToStreamData(animator, layers);
		if (!m_Data.IsValid())
		{
			return false;
		}
		job = RigUtils.syncSceneToStreamBinder.Create(animator, m_Data);
		return isInitialized = true;
	}

	public void Update(IList<IRigLayer> layers)
	{
		if (isInitialized && m_Data.IsValid())
		{
			IRigSyncSceneToStreamData rigSyncSceneToStreamData = (IRigSyncSceneToStreamData)m_Data;
			int i = 0;
			for (int count = m_RigIndices.Count; i < count; i++)
			{
				rigSyncSceneToStreamData.rigStates[i] = layers[m_RigIndices[i]].active;
			}
			RigUtils.syncSceneToStreamBinder.Update(job, m_Data);
		}
	}

	public void Reset()
	{
		if (isInitialized)
		{
			if (m_Data != null && m_Data.IsValid())
			{
				RigUtils.syncSceneToStreamBinder.Destroy(job);
				m_Data = null;
			}
			isInitialized = false;
		}
	}

	public bool IsValid()
	{
		if (job != null)
		{
			return m_Data != null;
		}
		return false;
	}
}
