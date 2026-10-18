namespace UnityEngine.Rendering;

internal class ProbeVolumeBakingSetWeakReference
{
	public int m_InstanceID;

	public ProbeVolumeBakingSetWeakReference(ProbeVolumeBakingSet bakingSet)
	{
		Set(bakingSet);
	}

	public ProbeVolumeBakingSetWeakReference()
	{
		m_InstanceID = 0;
	}

	public void Set(ProbeVolumeBakingSet bakingSet)
	{
		if (bakingSet == null)
		{
			m_InstanceID = 0;
		}
		else
		{
			m_InstanceID = bakingSet.GetInstanceID();
		}
	}

	public ProbeVolumeBakingSet Get()
	{
		return Resources.InstanceIDToObject(m_InstanceID) as ProbeVolumeBakingSet;
	}

	public bool IsLoaded()
	{
		return Resources.InstanceIDIsValid(m_InstanceID);
	}

	public void Unload()
	{
		if (IsLoaded())
		{
			Resources.UnloadAsset(Get());
		}
	}
}
