using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering;

public static class CameraCaptureBridge
{
	private class CameraEntry
	{
		internal HashSet<Action<RenderTargetIdentifier, CommandBuffer>> actions;

		internal IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> cachedEnumerator;
	}

	private static Dictionary<Camera, CameraEntry> actionDict = new Dictionary<Camera, CameraEntry>();

	private static bool _enabled;

	public static bool enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			_enabled = value;
		}
	}

	public static IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> GetCaptureActions(Camera camera)
	{
		if (!actionDict.TryGetValue(camera, out var value) || value.actions.Count == 0)
		{
			return null;
		}
		return value.actions.GetEnumerator();
	}

	internal static IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> GetCachedCaptureActionsEnumerator(Camera camera)
	{
		if (!actionDict.TryGetValue(camera, out var value) || value.actions.Count == 0)
		{
			return null;
		}
		value.cachedEnumerator.Reset();
		return value.cachedEnumerator;
	}

	public static void AddCaptureAction(Camera camera, Action<RenderTargetIdentifier, CommandBuffer> action)
	{
		actionDict.TryGetValue(camera, out var value);
		if (value == null)
		{
			value = new CameraEntry
			{
				actions = new HashSet<Action<RenderTargetIdentifier, CommandBuffer>>()
			};
			actionDict.Add(camera, value);
		}
		value.actions.Add(action);
		value.cachedEnumerator = value.actions.GetEnumerator();
	}

	public static void RemoveCaptureAction(Camera camera, Action<RenderTargetIdentifier, CommandBuffer> action)
	{
		if (!(camera == null) && actionDict.TryGetValue(camera, out var value))
		{
			value.actions.Remove(action);
			value.cachedEnumerator = value.actions.GetEnumerator();
		}
	}
}
