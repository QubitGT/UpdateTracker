using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(Recorder))]
public class MicAmplifier : VoiceComponent
{
	[SerializeField]
	private float boostValue;

	[SerializeField]
	private float amplificationFactor = 1f;

	private MicAmplifierFloat floatProcessor;

	private MicAmplifierShort shortProcessor;

	public float AmplificationFactor
	{
		get
		{
			return amplificationFactor;
		}
		set
		{
			if (!amplificationFactor.Equals(value))
			{
				amplificationFactor = value;
				if (floatProcessor != null)
				{
					floatProcessor.AmplificationFactor = amplificationFactor;
				}
				if (shortProcessor != null)
				{
					shortProcessor.AmplificationFactor = (short)amplificationFactor;
				}
			}
		}
	}

	public float BoostValue
	{
		get
		{
			return boostValue;
		}
		set
		{
			if (!boostValue.Equals(value))
			{
				boostValue = value;
				if (floatProcessor != null)
				{
					floatProcessor.BoostValue = boostValue;
				}
				if (shortProcessor != null)
				{
					shortProcessor.BoostValue = (short)boostValue;
				}
			}
		}
	}

	private void OnEnable()
	{
		if (floatProcessor != null)
		{
			floatProcessor.Disabled = false;
		}
		if (shortProcessor != null)
		{
			shortProcessor.Disabled = false;
		}
	}

	private void OnDisable()
	{
		if (floatProcessor != null)
		{
			floatProcessor.Disabled = true;
		}
		if (shortProcessor != null)
		{
			shortProcessor.Disabled = true;
		}
	}

	private void PhotonVoiceCreated(PhotonVoiceCreatedParams p)
	{
		if (p.Voice is LocalVoiceAudioFloat)
		{
			LocalVoiceAudioFloat obj = p.Voice as LocalVoiceAudioFloat;
			floatProcessor = new MicAmplifierFloat(AmplificationFactor, BoostValue);
			obj.AddPostProcessor(floatProcessor);
		}
		else if (p.Voice is LocalVoiceAudioShort)
		{
			LocalVoiceAudioShort obj2 = p.Voice as LocalVoiceAudioShort;
			shortProcessor = new MicAmplifierShort((short)AmplificationFactor, (short)BoostValue);
			obj2.AddPostProcessor(shortProcessor);
		}
		else if (base.Logger.IsErrorEnabled)
		{
			base.Logger.LogError("LocalVoice object has unexpected value/type: {0}", (p.Voice == null) ? "null" : p.Voice.GetType().ToString());
		}
	}
}
