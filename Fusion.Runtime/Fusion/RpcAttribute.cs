using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class RpcAttribute : Attribute
{
	public const int MaxPayloadSize = 512;

	public int Sources { get; }

	public int Targets { get; }

	public bool InvokeLocal { get; set; } = true;


	public RpcChannel Channel { get; set; } = RpcChannel.Reliable;


	public bool TickAligned { get; set; } = true;


	public RpcHostMode HostMode { get; set; } = RpcHostMode.SourceIsServer;


	public RpcAttribute()
	{
	}

	public RpcAttribute(RpcSources sources, RpcTargets targets)
	{
		Sources = (int)sources;
		Targets = (int)targets;
	}
}
