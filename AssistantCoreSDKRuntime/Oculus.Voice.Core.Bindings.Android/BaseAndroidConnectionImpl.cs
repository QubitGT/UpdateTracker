using System;
using UnityEngine;

namespace Oculus.Voice.Core.Bindings.Android;

public class BaseAndroidConnectionImpl<T> where T : BaseServiceBinding
{
	private string fragmentClassName;

	protected T service;

	protected readonly AndroidServiceConnection serviceConnection;

	public bool IsConnected => serviceConnection.IsConnected;

	public BaseAndroidConnectionImpl(string className)
	{
		fragmentClassName = className;
		serviceConnection = new AndroidServiceConnection(className, "getService");
	}

	public virtual void Connect(string version)
	{
		serviceConnection.Connect(version);
		AndroidJavaObject androidJavaObject = serviceConnection.GetService();
		if (androidJavaObject != null)
		{
			service = (T)Activator.CreateInstance(typeof(T), androidJavaObject);
		}
	}

	public virtual void Disconnect()
	{
		service.Shutdown();
		serviceConnection.Disconnect();
		service = null;
	}
}
