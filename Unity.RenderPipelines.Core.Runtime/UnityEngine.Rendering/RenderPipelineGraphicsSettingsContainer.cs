using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

[Serializable]
public class RenderPipelineGraphicsSettingsContainer : ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	private RenderPipelineGraphicsSettingsCollection m_RuntimeSettings = new RenderPipelineGraphicsSettingsCollection();

	public List<IRenderPipelineGraphicsSettings> settingsList => m_RuntimeSettings.settingsList;

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
