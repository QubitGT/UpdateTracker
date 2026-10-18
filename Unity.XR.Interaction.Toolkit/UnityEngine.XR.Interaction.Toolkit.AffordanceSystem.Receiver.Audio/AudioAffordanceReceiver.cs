using System;
using System.Text;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Audio;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Affordance System/Receiver/Audio/Audio Affordance Receiver", 12)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Audio.AudioAffordanceReceiver.html")]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class AudioAffordanceReceiver : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Affordance state provider component to subscribe to.")]
	private BaseAffordanceStateProvider m_AffordanceStateProvider;

	[SerializeField]
	[Tooltip("Audio Affordance Theme datum property used to map affordance state to a Audio affordance value. Can store an asset or a serialized value.")]
	private AudioAffordanceThemeDatumProperty m_AffordanceThemeDatum;

	[SerializeField]
	[Tooltip("Audio Source where the audio clip will be played.")]
	private AudioSource m_AudioSource;

	private readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

	private byte m_LastAffordanceStateIndex;

	public BaseAffordanceStateProvider affordanceStateProvider
	{
		get
		{
			return m_AffordanceStateProvider;
		}
		set
		{
			m_AffordanceStateProvider = value;
		}
	}

	public AudioAffordanceThemeDatumProperty affordanceThemeDatum
	{
		get
		{
			return m_AffordanceThemeDatum;
		}
		set
		{
			m_AffordanceThemeDatum = value;
		}
	}

	public AudioSource audioSource
	{
		get
		{
			return m_AudioSource;
		}
		set
		{
			m_AudioSource = value;
		}
	}

	protected void OnValidate()
	{
		if (m_AudioSource == null)
		{
			m_AudioSource = GetComponent<AudioSource>();
		}
	}

	protected void Awake()
	{
		if (m_AudioSource == null)
		{
			m_AudioSource = GetComponent<AudioSource>();
		}
		if (m_AffordanceThemeDatum != null && m_AffordanceThemeDatum.Value != null)
		{
			m_AffordanceThemeDatum.Value.ValidateTheme();
			LogIfMissingAffordanceStates(m_AffordanceThemeDatum.Value);
		}
	}

	protected void OnEnable()
	{
		if (m_AffordanceStateProvider == null)
		{
			XRLoggingUtils.LogError($"Missing Affordance State Provider reference. Please set one on {this}.", this);
			base.enabled = false;
		}
		else if (m_AffordanceThemeDatum == null || m_AffordanceThemeDatum.Value == null)
		{
			XRLoggingUtils.LogError($"Missing Audio Affordance Theme Datum on {this}.", this);
			base.enabled = false;
		}
		else
		{
			m_BindingsGroup.AddBinding(m_AffordanceStateProvider.currentAffordanceStateData.Subscribe(OnAffordanceStateUpdated));
		}
	}

	protected void OnDisable()
	{
		m_BindingsGroup.Clear();
	}

	private void LogIfMissingAffordanceStates(AudioAffordanceTheme theme)
	{
		if (theme.GetAffordanceThemeDataForIndex((byte)(AffordanceStateShortcuts.stateCount - 1)) != null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (byte b = 0; b < AffordanceStateShortcuts.stateCount; b++)
		{
			AudioAffordanceThemeData affordanceThemeDataForIndex = theme.GetAffordanceThemeDataForIndex(b);
			stringBuilder.Append($"Expected: {b} \"{AffordanceStateShortcuts.GetNameForIndex(b)}\",\tActual: ");
			stringBuilder.AppendLine((affordanceThemeDataForIndex != null) ? $"{b} \"{affordanceThemeDataForIndex.stateName}\"" : "<b>(None)</b>");
			if (affordanceThemeDataForIndex != null)
			{
				num++;
			}
		}
		Debug.LogWarning("Affordance Theme on affordance receiver is missing a potential affordance state. Expected states:" + $"\nExpected Count: {AffordanceStateShortcuts.stateCount}, Actual Count: {num}" + $"\n{stringBuilder}", this);
	}

	private void OnAffordanceStateUpdated(AffordanceStateData affordanceStateData)
	{
		byte stateIndex = affordanceStateData.stateIndex;
		if (stateIndex == m_LastAffordanceStateIndex)
		{
			return;
		}
		bool num = stateIndex == 5;
		bool flag = stateIndex == 2;
		bool flag2 = stateIndex == 4;
		bool flag3 = m_LastAffordanceStateIndex == 4;
		bool flag4 = m_LastAffordanceStateIndex == 5;
		bool num2 = num && flag3;
		bool flag5 = flag2 && flag4;
		bool flag6 = flag && flag3;
		bool flag7 = flag && flag3;
		if (!num2 && !flag6)
		{
			AudioAffordanceThemeData audioAffordanceThemeData = m_AffordanceThemeDatum.Value?.GetAffordanceThemeDataForIndex(m_LastAffordanceStateIndex);
			if (audioAffordanceThemeData != null)
			{
				PlayAudioClip(audioAffordanceThemeData.stateExited);
			}
		}
		if (!flag5 && !flag7)
		{
			AudioAffordanceThemeData audioAffordanceThemeData2 = m_AffordanceThemeDatum.Value?.GetAffordanceThemeDataForIndex(stateIndex);
			if (audioAffordanceThemeData2 != null)
			{
				PlayAudioClip(audioAffordanceThemeData2.stateEntered);
			}
			else
			{
				string nameForIndex = AffordanceStateShortcuts.GetNameForIndex(stateIndex);
				XRLoggingUtils.LogError($"Missing theme data for affordance state index {stateIndex} \"{nameForIndex}\" with {this}.", this);
			}
		}
		m_LastAffordanceStateIndex = stateIndex;
	}

	private void PlayAudioClip(AudioClip clipToPlay)
	{
		if (!(clipToPlay == null))
		{
			m_AudioSource.PlayOneShot(clipToPlay);
		}
	}
}
