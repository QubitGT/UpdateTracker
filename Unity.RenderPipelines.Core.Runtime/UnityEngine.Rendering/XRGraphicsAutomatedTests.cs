using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering;

public static class XRGraphicsAutomatedTests
{
	public static bool running = false;

	private static bool activatedFromCommandLine => false;

	public static bool enabled { get; set; } = activatedFromCommandLine;


	internal static void OverrideLayout(XRLayout layout, Camera camera)
	{
		if (!enabled || !running)
		{
			return;
		}
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		if (!camera.TryGetCullingParameters(stereoAware: false, out var cullingParameters))
		{
			return;
		}
		cullingParameters.stereoProjectionMatrix = projectionMatrix;
		cullingParameters.stereoViewMatrix = worldToCameraMatrix;
		cullingParameters.stereoSeparationDistance = 0f;
		List<(Camera, XRPass)> activePasses = layout.GetActivePasses();
		for (int i = 0; i < activePasses.Count; i++)
		{
			XRPass item = activePasses[i].Item2;
			item.AssignCullingParams(item.cullingPassId, cullingParameters);
			for (int j = 0; j < item.viewCount; j++)
			{
				Matrix4x4 projMatrix = projectionMatrix;
				Matrix4x4 viewMatrix = worldToCameraMatrix;
				bool num = activePasses.Count == 2 && i == 0;
				bool flag = activePasses.Count == 1 && j == 0;
				if (num || flag)
				{
					FrustumPlanes decomposeProjection = projMatrix.decomposeProjection;
					decomposeProjection.left *= 0.44f;
					decomposeProjection.right *= 0.88f;
					decomposeProjection.top *= 0.11f;
					decomposeProjection.bottom *= 0.33f;
					projMatrix = Matrix4x4.Frustum(decomposeProjection);
					viewMatrix *= Matrix4x4.Translate(new Vector3(0.34f, 0.25f, -0.08f));
				}
				XRView xrView = new XRView(projMatrix, viewMatrix, Matrix4x4.identity, isPrevViewMatrixValid: false, item.GetViewport(j), null, null, item.GetTextureArraySlice(j));
				item.AssignView(j, xrView);
			}
		}
	}
}
