using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.Rendering.Universal;

[MovedFrom(false, "UnityEngine.Experimental.Rendering.Universal", "com.unity.render-pipelines.universal", null)]
public abstract class ShadowCasterGroup2D : MonoBehaviour
{
	[SerializeField]
	internal int m_ShadowGroup;

	[SerializeField]
	internal int m_Priority;

	private List<ShadowCaster2D> m_ShadowCasters;

	internal virtual void CacheValues()
	{
		if (m_ShadowCasters != null)
		{
			for (int i = 0; i < m_ShadowCasters.Count; i++)
			{
				m_ShadowCasters[i].CacheValues();
			}
		}
	}

	public List<ShadowCaster2D> GetShadowCasters()
	{
		return m_ShadowCasters;
	}

	public int GetShadowGroup()
	{
		return m_ShadowGroup;
	}

	public void RegisterShadowCaster2D(ShadowCaster2D shadowCaster2D)
	{
		if (m_ShadowCasters == null)
		{
			m_ShadowCasters = new List<ShadowCaster2D>();
		}
		int num = 0;
		for (num = 0; num < m_ShadowCasters.Count && shadowCaster2D.m_Priority < m_ShadowCasters[num].m_Priority; num++)
		{
		}
		m_ShadowCasters.Insert(num, shadowCaster2D);
	}

	public void UnregisterShadowCaster2D(ShadowCaster2D shadowCaster2D)
	{
		if (m_ShadowCasters != null)
		{
			m_ShadowCasters.Remove(shadowCaster2D);
		}
	}
}
