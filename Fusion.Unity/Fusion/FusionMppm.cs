using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Fusion;

public class FusionMppm
{
	public static readonly FusionMppmStatus Status;

	[CanBeNull]
	public static readonly FusionMppm MainEditor;

	[Conditional("UNITY_EDITOR")]
	public void Send<T>(T data) where T : FusionMppmCommand
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Obsolete("Use FusionMppm.Broadcaster?.Send instead")]
	public static void Broadcast<T>(T data) where T : FusionMppmCommand
	{
	}

	private FusionMppm()
	{
	}
}
