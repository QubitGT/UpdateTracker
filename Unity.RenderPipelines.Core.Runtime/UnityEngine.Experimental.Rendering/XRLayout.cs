using System.Collections.Generic;
using System.Text;

namespace UnityEngine.Experimental.Rendering;

public class XRLayout
{
	private readonly List<(Camera, XRPass)> m_ActivePasses = new List<(Camera, XRPass)>();

	public void AddCamera(Camera camera, bool enableXR)
	{
		if (!(camera == null))
		{
			bool flag = (camera.cameraType == CameraType.Game || camera.cameraType == CameraType.VR) && camera.targetTexture == null && enableXR;
			if (XRSystem.displayActive && flag)
			{
				XRSystem.SetDisplayZRange(camera.nearClipPlane, camera.farClipPlane);
				XRSystem.CreateDefaultLayout(camera, this);
			}
			else
			{
				AddPass(camera, XRSystem.emptyPass);
			}
		}
	}

	public void ReconfigurePass(XRPass xrPass, Camera camera)
	{
		if (xrPass.enabled)
		{
			XRSystem.ReconfigurePass(xrPass, camera);
			xrPass.UpdateCombinedOcclusionMesh();
		}
	}

	public List<(Camera, XRPass)> GetActivePasses()
	{
		return m_ActivePasses;
	}

	internal void AddPass(Camera camera, XRPass xrPass)
	{
		xrPass.UpdateCombinedOcclusionMesh();
		m_ActivePasses.Add((camera, xrPass));
	}

	internal void Clear()
	{
		for (int i = 0; i < m_ActivePasses.Count; i++)
		{
			XRPass item = m_ActivePasses[m_ActivePasses.Count - i - 1].Item2;
			if (item != XRSystem.emptyPass)
			{
				item.Release();
			}
		}
		m_ActivePasses.Clear();
	}

	internal void LogDebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("XRSystem setup for frame {0}, active: {1}", Time.frameCount, XRSystem.displayActive);
		stringBuilder.AppendLine();
		for (int i = 0; i < m_ActivePasses.Count; i++)
		{
			XRPass item = m_ActivePasses[i].Item2;
			for (int j = 0; j < item.viewCount; j++)
			{
				Rect viewport = item.GetViewport(j);
				stringBuilder.AppendFormat("XR Pass {0} Cull {1} View {2} Slice {3} : {4} x {5}", item.multipassId, item.cullingPassId, j, item.GetTextureArraySlice(j), viewport.width, viewport.height);
				stringBuilder.AppendLine();
			}
		}
		Debug.Log(stringBuilder);
	}
}
