using System.Runtime.InteropServices;
using UnityEngine;

[RequireComponent(typeof(MetaXRAudioSource))]
public class MetaXRAudioSourceExperimentalFeatures : MonoBehaviour
{
	public enum DirectivityPatternType
	{
		None,
		HumanVoice
	}

	private AudioSource source_;

	[SerializeField]
	[Tooltip("How much of the HRTF EQ is applied to the sound. Interaural time delay (ITD) and interaural level differences (ILD) are kept the same.")]
	[Range(0f, 1f)]
	private float hrtfIntensity = 1f;

	[SerializeField]
	[Tooltip("Used to increase the spatial audio emitter radius. Useful for sounds that come from a large area rather than a precise point. If increased too large, users may end up inside the radius if the sound source is too close.")]
	private float volumetricRadius;

	[SerializeField]
	[Tooltip("Additional gain applied to early reflections for this audio source only")]
	[Range(-60f, 20f)]
	private float earlyReflectionsSendDb;

	[SerializeField]
	[Tooltip("Adjust how much the direct-to-reverberant ratio increases with distance")]
	[Range(0f, 1f)]
	private float reverbReach = 0.5f;

	[SerializeField]
	[Tooltip("Adjust how much the direct-to-reverberant ratio increases with distance")]
	[Range(0f, 1f)]
	private float occlusionIntensity = 1f;

	[SerializeField]
	[Tooltip("Intensity controller for Directvity , Value of 1 will apply full directivity")]
	[Range(0f, 1f)]
	private float directivityIntensity = 1f;

	[SerializeField]
	[Tooltip("Option for human voice directivity pattern that makes this sound more muffled when the source is facing away from listener")]
	private DirectivityPatternType directivityPattern;

	[SerializeField]
	[Tooltip("This switch can disable direct sound propagation, so only late reverberations is heard from this source")]
	private bool directSoundEnabled = true;

	[SerializeField]
	[Tooltip("This switch can disable direct sound propagation, so only late reverberations is heard from this source")]
	private bool mediumAbsorption = true;

	public float HrtfIntensity
	{
		get
		{
			return hrtfIntensity;
		}
		set
		{
			hrtfIntensity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float VolumetricRadius
	{
		get
		{
			return volumetricRadius;
		}
		set
		{
			volumetricRadius = Mathf.Max(value, 0f);
		}
	}

	public float EarlyReflectionsSendDb
	{
		get
		{
			return earlyReflectionsSendDb;
		}
		set
		{
			earlyReflectionsSendDb = Mathf.Clamp(value, -60f, 20f);
		}
	}

	public float ReverbReach
	{
		get
		{
			return reverbReach;
		}
		set
		{
			reverbReach = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float OcclusionIntensity
	{
		get
		{
			return occlusionIntensity;
		}
		set
		{
			occlusionIntensity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float DirectivityIntensity
	{
		get
		{
			return directivityIntensity;
		}
		set
		{
			directivityIntensity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public DirectivityPatternType DirectivityPattern
	{
		get
		{
			return directivityPattern;
		}
		set
		{
			directivityPattern = value;
		}
	}

	public bool DirectSoundEnabled
	{
		get
		{
			return directSoundEnabled;
		}
		set
		{
			directSoundEnabled = value;
		}
	}

	public bool MediumAbsorption
	{
		get
		{
			return mediumAbsorption;
		}
		set
		{
			mediumAbsorption = value;
		}
	}

	private void OnValidate()
	{
		volumetricRadius = Mathf.Max(volumetricRadius, 0f);
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
	}

	public void UpdateParameters()
	{
		source_.SetSpatializerFloat(9, hrtfIntensity);
		source_.SetSpatializerFloat(13, directivityIntensity);
		source_.SetSpatializerFloat(4, volumetricRadius);
		source_.SetSpatializerFloat(10, earlyReflectionsSendDb);
		source_.SetSpatializerFloat(12, (directivityPattern == DirectivityPatternType.None) ? 0f : 1f);
		source_.SetSpatializerFloat(15, reverbReach);
		source_.SetSpatializerFloat(16, directSoundEnabled ? 1f : 0f);
		source_.SetSpatializerFloat(17, occlusionIntensity);
		source_.SetSpatializerFloat(18, mediumAbsorption ? 1f : 0f);
	}

	[DllImport("MetaXRAudioUnity")]
	private static extern void MetaXRAudio_GetGlobalRoomReflectionValues(ref bool reflOn, ref bool reverbOn, ref float width, ref float height, ref float length);
}
