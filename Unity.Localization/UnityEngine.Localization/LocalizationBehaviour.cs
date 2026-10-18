using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.Localization;

internal class LocalizationBehaviour : ComponentSingleton<LocalizationBehaviour>
{
	private Queue<(int frame, AsyncOperationHandle handle)> m_ReleaseQueue = new Queue<(int, AsyncOperationHandle)>();

	private const long k_MaxMsPerUpdate = 10L;

	private const bool k_DisableThrottling = false;

	protected override string GetGameObjectName()
	{
		return "Localization Resource Manager";
	}

	public static void ReleaseNextFrame(AsyncOperationHandle handle)
	{
		ComponentSingleton<LocalizationBehaviour>.Instance.DoReleaseNextFrame(handle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static long TimeSinceStartupMs()
	{
		return (long)(Time.realtimeSinceStartup * 1000f);
	}

	private void DoReleaseNextFrame(AsyncOperationHandle handle)
	{
		base.enabled = true;
		m_ReleaseQueue.Enqueue((Time.frameCount, handle));
	}

	private void LateUpdate()
	{
		int frameCount = Time.frameCount;
		long num = TimeSinceStartupMs() + 10;
		while (m_ReleaseQueue.Count > 0 && m_ReleaseQueue.Peek().frame < frameCount && TimeSinceStartupMs() < num)
		{
			AddressablesInterface.SafeRelease(m_ReleaseQueue.Dequeue().handle);
		}
		if (m_ReleaseQueue.Count == 0)
		{
			base.enabled = false;
		}
	}

	public static void ForceRelease()
	{
		foreach (var item in ComponentSingleton<LocalizationBehaviour>.Instance.m_ReleaseQueue)
		{
			AddressablesInterface.SafeRelease(item.handle);
		}
		ComponentSingleton<LocalizationBehaviour>.Instance.m_ReleaseQueue.Clear();
	}
}
