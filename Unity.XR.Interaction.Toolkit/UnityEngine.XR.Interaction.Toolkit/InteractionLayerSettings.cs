using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit;

[ScriptableSettingsPath("Assets/XRI/Settings")]
internal class InteractionLayerSettings : ScriptableSettings<InteractionLayerSettings>, ISerializationCallbackReceiver
{
	private const string k_DefaultLayerName = "Default";

	internal const int layerSize = 32;

	internal const int builtInLayerSize = 1;

	[SerializeField]
	private string[] m_LayerNames;

	internal static InteractionLayerSettings GetInstanceOrLoadOnly()
	{
		if (ScriptableSettingsBase<InteractionLayerSettings>.BaseInstance != null)
		{
			return ScriptableSettingsBase<InteractionLayerSettings>.BaseInstance;
		}
		ScriptableSettingsBase<InteractionLayerSettings>.BaseInstance = Resources.Load(ScriptableSettingsBase<InteractionLayerSettings>.GetFilePath(), typeof(InteractionLayerSettings)) as InteractionLayerSettings;
		return ScriptableSettingsBase<InteractionLayerSettings>.BaseInstance;
	}

	internal bool IsLayerEmpty(int index)
	{
		if (m_LayerNames != null)
		{
			return string.IsNullOrEmpty(m_LayerNames[index]);
		}
		return true;
	}

	internal void SetLayerNameAt(int index, string layerName)
	{
		if (m_LayerNames != null && index < m_LayerNames.Length)
		{
			m_LayerNames[index] = layerName;
		}
	}

	internal string GetLayerNameAt(int index)
	{
		if (m_LayerNames == null || index >= m_LayerNames.Length)
		{
			return string.Empty;
		}
		return m_LayerNames[index];
	}

	internal int GetLayer(string layerName)
	{
		if (m_LayerNames == null)
		{
			return -1;
		}
		for (int i = 0; i < m_LayerNames.Length; i++)
		{
			if (string.Equals(layerName, m_LayerNames[i]))
			{
				return i;
			}
		}
		return -1;
	}

	internal void GetLayerNamesAndValues(List<string> names, List<int> values)
	{
		if (m_LayerNames == null)
		{
			return;
		}
		for (int i = 0; i < m_LayerNames.Length; i++)
		{
			string text = m_LayerNames[i];
			if (!string.IsNullOrEmpty(text))
			{
				names.Add(text);
				values.Add(i);
			}
		}
	}

	public void OnBeforeSerialize()
	{
		if (m_LayerNames == null)
		{
			m_LayerNames = new string[32];
		}
		if (m_LayerNames.Length != 32)
		{
			Array.Resize(ref m_LayerNames, 32);
		}
		if (!string.Equals(m_LayerNames[0], "Default"))
		{
			m_LayerNames[0] = "Default";
		}
	}

	public void OnAfterDeserialize()
	{
	}
}
