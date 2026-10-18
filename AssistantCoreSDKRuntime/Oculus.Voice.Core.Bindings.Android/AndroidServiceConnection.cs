using System;
using Oculus.Voice.Core.Bindings.Interfaces;
using UnityEngine;

namespace Oculus.Voice.Core.Bindings.Android;

public class AndroidServiceConnection : IConnection
{
	private AndroidJavaObject mAssistantServiceConnection;

	private string serviceFragmentClass;

	private string serviceGetter;

	public bool IsConnected => mAssistantServiceConnection != null;

	public AndroidJavaObject AssistantServiceConnection => mAssistantServiceConnection;

	public AndroidServiceConnection(string serviceFragmentClassName, string serviceGetterMethodName)
	{
		serviceFragmentClass = serviceFragmentClassName;
		serviceGetter = serviceGetterMethodName;
	}

	public void Connect(string version)
	{
		if (mAssistantServiceConnection != null)
		{
			return;
		}
		try
		{
			AndroidJNIHelper.debug = true;
			AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			using AndroidJavaClass androidJavaClass = new AndroidJavaClass(serviceFragmentClass);
			mAssistantServiceConnection = androidJavaClass.CallStatic<AndroidJavaObject>("createAndAttach", new object[2] { @static, version });
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("AndroidServiceConnection Connect Failed\nService: {0}\nException:\n{1}\n\n", serviceFragmentClass, ex);
		}
	}

	public void Disconnect()
	{
		mAssistantServiceConnection?.Call("detach");
	}

	public AndroidJavaObject GetService()
	{
		return mAssistantServiceConnection?.Call<AndroidJavaObject>(serviceGetter, Array.Empty<object>());
	}
}
