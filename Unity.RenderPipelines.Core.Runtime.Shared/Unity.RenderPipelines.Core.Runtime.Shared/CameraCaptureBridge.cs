using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.RenderPipelines.Core.Runtime.Shared;

internal static class CameraCaptureBridge
{
	public static IEnumerator<Action<RenderTargetIdentifier, CommandBuffer>> GetCachedCaptureActionsEnumerator(Camera camera)
	{
		return UnityEngine.Rendering.CameraCaptureBridge.GetCachedCaptureActionsEnumerator(camera);
	}
}
