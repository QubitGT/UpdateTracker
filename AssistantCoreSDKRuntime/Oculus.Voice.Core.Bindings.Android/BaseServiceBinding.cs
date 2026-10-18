using UnityEngine;

namespace Oculus.Voice.Core.Bindings.Android;

public class BaseServiceBinding
{
	protected AndroidJavaObject binding;

	protected BaseServiceBinding(AndroidJavaObject sdkInstance)
	{
		binding = sdkInstance;
	}

	public void Shutdown()
	{
		binding.Call("shutdown");
	}
}
