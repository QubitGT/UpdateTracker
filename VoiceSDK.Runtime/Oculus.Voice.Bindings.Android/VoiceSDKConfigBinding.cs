using Meta.WitAi.Configuration;
using UnityEngine;

namespace Oculus.Voice.Bindings.Android;

public class VoiceSDKConfigBinding
{
	private WitRuntimeConfiguration configuration;

	public VoiceSDKConfigBinding(WitRuntimeConfiguration config)
	{
		configuration = config;
	}

	public AndroidJavaObject ToJavaObject()
	{
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("com.oculus.assistant.api.voicesdk.immersivevoicecommands.WitConfiguration");
		androidJavaObject.Set("clientAccessToken", configuration.witConfiguration.GetClientAccessToken());
		AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("com.oculus.assistant.api.voicesdk.immersivevoicecommands.WitRuntimeConfiguration");
		androidJavaObject2.Set("witConfiguration", androidJavaObject);
		androidJavaObject2.Set("minKeepAliveVolume", configuration.minKeepAliveVolume);
		androidJavaObject2.Set("minKeepAliveTimeInSeconds", configuration.minKeepAliveTimeInSeconds);
		androidJavaObject2.Set("minTranscriptionKeepAliveTimeInSeconds", configuration.minTranscriptionKeepAliveTimeInSeconds);
		androidJavaObject2.Set("maxRecordingTime", configuration.maxRecordingTime);
		androidJavaObject2.Set("soundWakeThreshold", configuration.soundWakeThreshold);
		androidJavaObject2.Set("sampleLengthInMs", configuration.sampleLengthInMs);
		androidJavaObject2.Set("micBufferLengthInSeconds", configuration.micBufferLengthInSeconds);
		androidJavaObject2.Set("sendAudioToWit", configuration.sendAudioToWit);
		androidJavaObject2.Set("preferredActivationOffset", configuration.preferredActivationOffset);
		androidJavaObject2.Set("clientName", "wit-unity");
		androidJavaObject2.Set("serverVersion", "20250213");
		return androidJavaObject2;
	}
}
