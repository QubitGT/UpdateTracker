using Meta.WitAi;
using Meta.WitAi.Configuration;
using Oculus.Voice.Dictation.Configuration;
using UnityEngine;

namespace Oculus.Voice.Dictation.Bindings.Android;

public class DictationConfigurationBinding
{
	private readonly WitDictationRuntimeConfiguration _runtimeConfiguration;

	private readonly DictationConfiguration _dictationConfiguration;

	private readonly int MAX_PLATFORM_SUPPORTED_RECORDING_TIME_SECONDS = 300;

	public DictationConfigurationBinding(WitDictationRuntimeConfiguration runtimeConfiguration)
	{
		if (runtimeConfiguration == null)
		{
			VLog.W("No dictation config has been defined. Using the default configuration.");
			_dictationConfiguration = new DictationConfiguration();
		}
		else
		{
			_dictationConfiguration = runtimeConfiguration.dictationConfiguration;
			_runtimeConfiguration = runtimeConfiguration;
		}
	}

	public AndroidJavaObject ToJavaObject()
	{
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.oculus.assistant.api.voicesdk.dictation.PlatformDictationConfiguration");
		androidJavaObject.Set("multiPhrase", _dictationConfiguration.multiPhrase);
		androidJavaObject.Set("scenario", _dictationConfiguration.scenario);
		androidJavaObject.Set("inputType", _dictationConfiguration.inputType);
		if (_runtimeConfiguration != null)
		{
			int num = (int)_runtimeConfiguration.maxRecordingTime;
			if (num < 0)
			{
				num = MAX_PLATFORM_SUPPORTED_RECORDING_TIME_SECONDS;
			}
			androidJavaObject.Set("interactionTimeoutSeconds", num);
		}
		return androidJavaObject;
	}
}
