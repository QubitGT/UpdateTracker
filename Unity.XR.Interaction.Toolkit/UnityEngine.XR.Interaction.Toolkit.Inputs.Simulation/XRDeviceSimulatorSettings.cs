using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[ScriptableSettingsPath("Assets/XRI/Settings")]
internal class XRDeviceSimulatorSettings : ScriptableSettings<XRDeviceSimulatorSettings>
{
	[SerializeField]
	private bool m_AutomaticallyInstantiateSimulatorPrefab;

	[SerializeField]
	private bool m_AutomaticallyInstantiateInEditorOnly = true;

	[SerializeField]
	private bool m_UseClassic;

	[SerializeField]
	private GameObject m_SimulatorPrefab;

	internal bool automaticallyInstantiateSimulatorPrefab
	{
		get
		{
			return m_AutomaticallyInstantiateSimulatorPrefab;
		}
		set
		{
			m_AutomaticallyInstantiateSimulatorPrefab = value;
		}
	}

	internal bool automaticallyInstantiateInEditorOnly
	{
		get
		{
			return m_AutomaticallyInstantiateInEditorOnly;
		}
		set
		{
			m_AutomaticallyInstantiateInEditorOnly = value;
		}
	}

	internal bool useClassic
	{
		get
		{
			return m_UseClassic;
		}
		set
		{
			m_UseClassic = value;
		}
	}

	internal GameObject simulatorPrefab
	{
		get
		{
			return m_SimulatorPrefab;
		}
		set
		{
			m_SimulatorPrefab = value;
		}
	}

	internal static XRDeviceSimulatorSettings GetInstanceOrLoadOnly()
	{
		if (ScriptableSettingsBase<XRDeviceSimulatorSettings>.BaseInstance != null)
		{
			return ScriptableSettingsBase<XRDeviceSimulatorSettings>.BaseInstance;
		}
		ScriptableSettingsBase<XRDeviceSimulatorSettings>.BaseInstance = Resources.Load(ScriptableSettingsBase<XRDeviceSimulatorSettings>.GetFilePath(), typeof(XRDeviceSimulatorSettings)) as XRDeviceSimulatorSettings;
		return ScriptableSettingsBase<XRDeviceSimulatorSettings>.BaseInstance;
	}
}
