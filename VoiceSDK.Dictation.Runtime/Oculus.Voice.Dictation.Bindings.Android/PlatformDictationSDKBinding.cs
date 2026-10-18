using System;
using Oculus.Voice.Core.Bindings.Android;
using UnityEngine;

namespace Oculus.Voice.Dictation.Bindings.Android;

public class PlatformDictationSDKBinding : BaseServiceBinding
{
	public bool Active => binding.Call<bool>("isActive", Array.Empty<object>());

	public bool IsRequestActive => binding.Call<bool>("isRequestActive", Array.Empty<object>());

	public bool IsSupported => binding.Call<bool>("isSupported", Array.Empty<object>());

	public PlatformDictationSDKBinding(AndroidJavaObject sdkInstance)
		: base(sdkInstance)
	{
	}

	public void StartDictation(DictationConfigurationBinding configuration)
	{
		binding.Call("startDictation", configuration.ToJavaObject());
	}

	public void StopDictation()
	{
		binding.Call("stopDictation");
	}

	public void SetListener(DictationListenerBinding listenerBinding)
	{
		binding.Call("setListener", listenerBinding);
	}
}
