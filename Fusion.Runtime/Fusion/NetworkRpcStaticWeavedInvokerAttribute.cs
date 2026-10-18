using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class NetworkRpcStaticWeavedInvokerAttribute : Attribute
{
	public string Key { get; }

	public NetworkRpcStaticWeavedInvokerAttribute(string key)
	{
		Key = key;
	}
}
