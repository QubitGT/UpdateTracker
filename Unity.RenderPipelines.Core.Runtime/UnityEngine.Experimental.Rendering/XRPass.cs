using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering;

public class XRPass
{
	private readonly List<XRView> m_Views;

	private readonly XROcclusionMesh m_OcclusionMesh;

	private readonly XRVisibleMesh m_VisibleMesh;

	public bool enabled => viewCount > 0;

	public bool supportsFoveatedRendering
	{
		get
		{
			if (enabled && foveatedRenderingInfo != IntPtr.Zero)
			{
				return XRSystem.foveatedRenderingCaps != FoveatedRenderingCaps.None;
			}
			return false;
		}
	}

	public bool copyDepth { get; private set; }

	public bool hasMotionVectorPass { get; private set; }

	public bool spaceWarpRightHandedNDC { get; private set; }

	public bool isFirstCameraPass => multipassId == 0;

	public bool isLastCameraPass
	{
		get
		{
			if ((multipassId != 1 || viewCount > 1) && (multipassId != 0 || viewCount <= 1))
			{
				if (multipassId == 0)
				{
					return viewCount == 0;
				}
				return false;
			}
			return true;
		}
	}

	public int multipassId { get; private set; }

	public int cullingPassId { get; private set; }

	public int renderTargetScaledWidth { get; private set; }

	public int renderTargetScaledHeight { get; private set; }

	public RenderTargetIdentifier renderTarget { get; private set; }

	public RenderTextureDescriptor renderTargetDesc { get; private set; }

	public RenderTargetIdentifier motionVectorRenderTarget { get; private set; }

	public RenderTextureDescriptor motionVectorRenderTargetDesc { get; private set; }

	public ScriptableCullingParameters cullingParams { get; private set; }

	public int viewCount => m_Views.Count;

	public bool singlePassEnabled => viewCount > 1;

	public IntPtr foveatedRenderingInfo { get; private set; }

	public bool isHDRDisplayOutputActive => XRSystem.GetActiveDisplay().hdrOutputSettings?.active ?? false;

	public ColorGamut hdrDisplayOutputColorGamut => XRSystem.GetActiveDisplay().hdrOutputSettings?.displayColorGamut ?? ColorGamut.sRGB;

	public HDROutputUtils.HDRDisplayInformation hdrDisplayOutputInformation => new HDROutputUtils.HDRDisplayInformation(XRSystem.GetActiveDisplay().hdrOutputSettings?.maxFullFrameToneMapLuminance ?? (-1), XRSystem.GetActiveDisplay().hdrOutputSettings?.maxToneMapLuminance ?? (-1), XRSystem.GetActiveDisplay().hdrOutputSettings?.minToneMapLuminance ?? (-1), XRSystem.GetActiveDisplay().hdrOutputSettings?.paperWhiteNits ?? 160f);

	public float occlusionMeshScale { get; private set; }

	public bool hasValidOcclusionMesh => m_OcclusionMesh.hasValidOcclusionMesh;

	public bool hasValidVisibleMesh
	{
		get
		{
			if (m_VisibleMesh.hasValidVisibleMesh)
			{
				return XRSystem.GetUseVisibilityMesh();
			}
			return false;
		}
	}

	public XRPass()
	{
		m_Views = new List<XRView>(2);
		m_OcclusionMesh = new XROcclusionMesh(this);
		m_VisibleMesh = new XRVisibleMesh(this);
	}

	public static XRPass CreateDefault(XRPassCreateInfo createInfo)
	{
		XRPass xRPass = GenericPool<XRPass>.Get();
		xRPass.InitBase(createInfo);
		return xRPass;
	}

	public virtual void Release()
	{
		m_VisibleMesh.Dispose();
		GenericPool<XRPass>.Release(this);
	}

	public Matrix4x4 GetProjMatrix(int viewIndex = 0)
	{
		return m_Views[viewIndex].projMatrix;
	}

	public Matrix4x4 GetViewMatrix(int viewIndex = 0)
	{
		return m_Views[viewIndex].viewMatrix;
	}

	public bool GetPrevViewValid(int viewIndex = 0)
	{
		return m_Views[viewIndex].isPrevViewMatrixValid;
	}

	public Matrix4x4 GetPrevViewMatrix(int viewIndex = 0)
	{
		return m_Views[viewIndex].prevViewMatrix;
	}

	public Rect GetViewport(int viewIndex = 0)
	{
		return m_Views[viewIndex].viewport;
	}

	public Mesh GetOcclusionMesh(int viewIndex = 0)
	{
		return m_Views[viewIndex].occlusionMesh;
	}

	public Mesh GetVisibleMesh(int viewIndex = 0)
	{
		return m_Views[viewIndex].visibleMesh;
	}

	public int GetTextureArraySlice(int viewIndex = 0)
	{
		return m_Views[viewIndex].textureArraySlice;
	}

	public void StartSinglePass(CommandBuffer cmd)
	{
		if (enabled && singlePassEnabled)
		{
			if (viewCount > TextureXR.slices)
			{
				throw new NotImplementedException($"Invalid XR setup for single-pass, trying to render too many views! Max supported: {TextureXR.slices}");
			}
			if (SystemInfo.supportsMultiview)
			{
				cmd.EnableKeyword(in SinglepassKeywords.STEREO_MULTIVIEW_ON);
				return;
			}
			cmd.EnableKeyword(in SinglepassKeywords.STEREO_INSTANCING_ON);
			cmd.SetInstanceMultiplier((uint)viewCount);
		}
	}

	public void StartSinglePass(IRasterCommandBuffer cmd)
	{
		StartSinglePass((cmd as BaseCommandBuffer).m_WrappedCommandBuffer);
	}

	public void StopSinglePass(CommandBuffer cmd)
	{
		if (enabled && singlePassEnabled)
		{
			if (SystemInfo.supportsMultiview)
			{
				cmd.DisableKeyword(in SinglepassKeywords.STEREO_MULTIVIEW_ON);
				return;
			}
			cmd.DisableKeyword(in SinglepassKeywords.STEREO_INSTANCING_ON);
			cmd.SetInstanceMultiplier(1u);
		}
	}

	public void StopSinglePass(BaseCommandBuffer cmd)
	{
		StopSinglePass(cmd.m_WrappedCommandBuffer);
	}

	public void RenderOcclusionMesh(CommandBuffer cmd, bool renderIntoTexture = false)
	{
		if (occlusionMeshScale > 0f)
		{
			m_OcclusionMesh.RenderOcclusionMesh(cmd, occlusionMeshScale, renderIntoTexture);
		}
	}

	public void RenderOcclusionMesh(RasterCommandBuffer cmd, bool renderIntoTexture = false)
	{
		if (occlusionMeshScale > 0f)
		{
			m_OcclusionMesh.RenderOcclusionMesh(cmd.m_WrappedCommandBuffer, occlusionMeshScale, renderIntoTexture);
		}
	}

	public void RenderVisibleMeshCustomMaterial(RasterCommandBuffer cmd, float occlusionMeshScale, Material material, MaterialPropertyBlock materialBlock, int shaderPass, bool renderIntoTexture = false)
	{
		if (occlusionMeshScale > 0f)
		{
			m_VisibleMesh.RenderVisibleMeshCustomMaterial(cmd.m_WrappedCommandBuffer, occlusionMeshScale, material, materialBlock, shaderPass, renderIntoTexture);
		}
	}

	public void RenderVisibleMeshCustomMaterial(CommandBuffer cmd, float occlusionMeshScale, Material material, MaterialPropertyBlock materialBlock, int shaderPass = 0, bool renderIntoTexture = false)
	{
		if (occlusionMeshScale > 0f)
		{
			m_VisibleMesh.RenderVisibleMeshCustomMaterial(cmd, occlusionMeshScale, material, materialBlock, shaderPass, renderIntoTexture);
		}
	}

	public void RenderDebugXRViewsFrustum()
	{
		for (int i = 0; i < m_Views.Count; i++)
		{
			XRView xRView = m_Views[i];
			Vector3[] array = CoreUtils.CalculateViewSpaceCorners(xRView.projMatrix, 10f);
			Vector3 start = -xRView.viewMatrix.GetColumn(3);
			for (int j = 0; j < 4; j++)
			{
				Debug.DrawLine(start, xRView.viewMatrix.MultiplyPoint(array[j]), (i == 0) ? Color.green : Color.red);
			}
		}
	}

	public Vector4 ApplyXRViewCenterOffset(Vector2 center)
	{
		Vector4 zero = Vector4.zero;
		float num = 0.5f - center.x;
		float num2 = 0.5f - center.y;
		zero.x = m_Views[0].eyeCenterUV.x - num;
		zero.y = m_Views[0].eyeCenterUV.y - num2;
		if (singlePassEnabled)
		{
			zero.z = m_Views[1].eyeCenterUV.x - num;
			zero.w = m_Views[1].eyeCenterUV.y - num2;
		}
		return zero;
	}

	internal void AssignView(int viewId, XRView xrView)
	{
		if (viewId < 0 || viewId >= m_Views.Count)
		{
			throw new ArgumentOutOfRangeException("viewId");
		}
		m_Views[viewId] = xrView;
	}

	internal void AssignCullingParams(int cullingPassId, ScriptableCullingParameters cullingParams)
	{
		cullingParams.cullingOptions &= ~CullingOptions.Stereo;
		this.cullingPassId = cullingPassId;
		this.cullingParams = cullingParams;
	}

	internal void UpdateCombinedOcclusionMesh()
	{
		m_OcclusionMesh.UpdateCombinedMesh();
		m_VisibleMesh.UpdateCombinedMesh();
	}

	public void InitBase(XRPassCreateInfo createInfo)
	{
		m_Views.Clear();
		copyDepth = createInfo.copyDepth;
		multipassId = createInfo.multipassId;
		AssignCullingParams(createInfo.cullingPassId, createInfo.cullingParameters);
		renderTarget = new RenderTargetIdentifier(createInfo.renderTarget, 0, CubemapFace.Unknown, -1);
		renderTargetDesc = createInfo.renderTargetDesc;
		renderTargetScaledWidth = createInfo.renderTargetScaledWidth;
		renderTargetScaledHeight = createInfo.renderTargetScaledHeight;
		motionVectorRenderTarget = new RenderTargetIdentifier(createInfo.motionVectorRenderTarget, 0, CubemapFace.Unknown, -1);
		motionVectorRenderTargetDesc = createInfo.motionVectorRenderTargetDesc;
		hasMotionVectorPass = createInfo.hasMotionVectorPass;
		spaceWarpRightHandedNDC = createInfo.spaceWarpRightHandedNDC;
		m_OcclusionMesh.SetMaterial(createInfo.occlusionMeshMaterial);
		occlusionMeshScale = createInfo.occlusionMeshScale;
		foveatedRenderingInfo = createInfo.foveatedRenderingInfo;
	}

	internal void AddView(XRView xrView)
	{
		if (m_Views.Count < TextureXR.slices)
		{
			m_Views.Add(xrView);
			return;
		}
		throw new NotImplementedException($"Invalid XR setup for single-pass, trying to add too many views! Max supported: {TextureXR.slices}");
	}
}
