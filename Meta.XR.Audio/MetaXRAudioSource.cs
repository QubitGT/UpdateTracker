using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MetaXRAudioSource : MonoBehaviour
{
	public enum NativeParameterIndex
	{
		P_GAIN,
		P_USEINVSQR,
		P_NEAR,
		P_FAR,
		P_RADIUS,
		P_DISABLE_RFL,
		P_AMBISTAT,
		P_READONLY_GLOBAL_RFL_ENABLED,
		P_READONLY_NUM_VOICES,
		P_HRTF_INTENSITY,
		P_REFLECTIONS_SEND,
		P_REVERB_SEND,
		P_DIRECTIVITY_ENABLED,
		P_DIRECTIVITY_INTENSITY,
		P_AMBI_DIRECT_ENABLED,
		P_REVERB_REACH,
		P_DIRECT_ENABLED,
		P_OCCLUSION_INTENSITY,
		P_MEDIUM_ABSORPTION,
		P_NUM
	}

	private AudioSource source_;

	private bool wasPlaying_;

	[SerializeField]
	[Tooltip("Enables HRTF Spatialization.")]
	private bool enableSpatialization = true;

	[SerializeField]
	[Tooltip("Additional gain beyond 0dB")]
	[Range(0f, 20f)]
	private float gainBoostDb;

	[SerializeField]
	[Tooltip("Enables room acoustics simulation (early reflections and reverberation) for this audio source only")]
	private bool enableAcoustics = true;

	[SerializeField]
	[Tooltip("Additional gain applied to reverb send for this audio source only")]
	[Range(-60f, 20f)]
	private float reverbSendDb;

	public bool EnableSpatialization
	{
		get
		{
			return enableSpatialization;
		}
		set
		{
			enableSpatialization = value;
		}
	}

	public float GainBoostDb
	{
		get
		{
			return gainBoostDb;
		}
		set
		{
			gainBoostDb = Mathf.Clamp(value, 0f, 20f);
		}
	}

	public bool EnableAcoustics
	{
		get
		{
			return enableAcoustics;
		}
		set
		{
			enableAcoustics = value;
		}
	}

	public float ReverbSendDb
	{
		get
		{
			return reverbSendDb;
		}
		set
		{
			reverbSendDb = Mathf.Clamp(value, -60f, 20f);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void OnBeforeSceneLoadRuntimeMethod()
	{
		Debug.Log($"Setting spatial voice limit: {MetaXRAudioSettings.Instance.voiceLimit}");
		MetaXRAudio_SetGlobalVoiceLimit(MetaXRAudioSettings.Instance.voiceLimit);
	}

	private void Awake()
	{
		source_ = GetComponent<AudioSource>();
		UpdateParameters();
	}

	private void Update()
	{
		if (source_ == null)
		{
			source_ = GetComponent<AudioSource>();
			if (source_ == null)
			{
				return;
			}
		}
		UpdateParameters();
		wasPlaying_ = source_.isPlaying;
	}

	public void UpdateParameters()
	{
		source_.spatialize = enableSpatialization;
		source_.SetSpatializerFloat(0, gainBoostDb);
		source_.SetSpatializerFloat(5, enableAcoustics ? 0f : 1f);
		source_.SetSpatializerFloat(11, reverbSendDb);
	}

	[DllImport("MetaXRAudioUnity")]
	private static extern int MetaXRAudio_SetGlobalVoiceLimit(int VoiceLimit);
}
