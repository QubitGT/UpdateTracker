using UnityEngine.Localization.Events;

namespace UnityEngine.Localization.Components;

[AddComponentMenu("Localization/Asset/Localize Prefab Event")]
public class LocalizedGameObjectEvent : LocalizedAssetEvent<GameObject, LocalizedGameObject, UnityEventGameObject>
{
	private GameObject m_Current;

	protected override void UpdateAsset(GameObject localizedAsset)
	{
		if (m_Current != null)
		{
			Object.Destroy(m_Current);
			m_Current = null;
		}
		if (localizedAsset != null)
		{
			m_Current = Object.Instantiate(localizedAsset, base.transform);
			m_Current.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
		}
		base.OnUpdateAsset.Invoke(m_Current);
	}
}
