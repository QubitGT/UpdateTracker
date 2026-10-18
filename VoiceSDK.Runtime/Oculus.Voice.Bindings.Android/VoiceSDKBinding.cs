using System;
using Meta.WitAi.Configuration;
using Oculus.Voice.Core.Bindings.Android;
using UnityEngine;
using UnityEngine.Scripting;

namespace Oculus.Voice.Bindings.Android;

public class VoiceSDKBinding : BaseServiceBinding
{
	public bool Active => binding.Call<bool>("isActive", Array.Empty<object>());

	public bool IsRequestActive => binding.Call<bool>("isRequestActive", Array.Empty<object>());

	public bool MicActive => binding.Call<bool>("isMicActive", Array.Empty<object>());

	public bool PlatformSupportsWit => binding.Call<bool>("isSupported", Array.Empty<object>());

	[Preserve]
	public VoiceSDKBinding(AndroidJavaObject sdkInstance)
		: base(sdkInstance)
	{
	}

	public void Activate(string text, WitRequestOptions options)
	{
		binding.Call("activate", text, options.ToJsonString());
	}

	public void Activate(WitRequestOptions options)
	{
		binding.Call("activate", options.ToJsonString());
	}

	public void ActivateImmediately(WitRequestOptions options)
	{
		binding.Call("activateImmediately", options.ToJsonString());
	}

	public void Deactivate()
	{
		binding.Call("deactivate");
	}

	public void DeactivateAndAbortRequest()
	{
		binding.Call("deactivateAndAbortRequest");
	}

	public void Deactivate(string requestID)
	{
		binding.Call("deactivate", requestID);
	}

	public void DeactivateAndAbortRequest(string requestID)
	{
		binding.Call("deactivateAndAbortRequest", requestID);
	}

	public void SetRuntimeConfiguration(WitRuntimeConfiguration configuration)
	{
		binding.Call("setRuntimeConfig", new VoiceSDKConfigBinding(configuration).ToJavaObject());
	}

	public void SetListener(VoiceSDKListenerBinding listener)
	{
		binding.Call("setListener", listener);
	}

	public void Connect()
	{
		binding.Call<bool>("connect", Array.Empty<object>());
	}
}
