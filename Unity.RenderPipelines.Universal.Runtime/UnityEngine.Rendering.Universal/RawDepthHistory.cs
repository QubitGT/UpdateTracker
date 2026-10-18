namespace UnityEngine.Rendering.Universal;

public sealed class RawDepthHistory : CameraHistoryItem
{
	private int[] m_Ids = new int[2];

	private static readonly string[] m_Names = new string[2] { "RawDepthHistory0", "RawDepthHistory1" };

	private RenderTextureDescriptor m_Descriptor;

	private Hash128 m_DescKey;

	public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
	{
		base.OnCreate(owner, typeId);
		m_Ids[0] = MakeId(0u);
		m_Ids[1] = MakeId(1u);
	}

	public RTHandle GetCurrentTexture(int eyeIndex = 0)
	{
		if ((uint)eyeIndex >= m_Ids.Length)
		{
			return null;
		}
		return GetCurrentFrameRT(m_Ids[eyeIndex]);
	}

	public RTHandle GetPreviousTexture(int eyeIndex = 0)
	{
		if ((uint)eyeIndex >= m_Ids.Length)
		{
			return null;
		}
		return GetPreviousFrameRT(m_Ids[eyeIndex]);
	}

	private bool IsAllocated()
	{
		return GetCurrentTexture() != null;
	}

	private bool IsDirty(ref RenderTextureDescriptor desc)
	{
		return m_DescKey != Hash128.Compute(ref desc);
	}

	private void Alloc(ref RenderTextureDescriptor desc, bool xrMultipassEnabled)
	{
		AllocHistoryFrameRT(m_Ids[0], 2, ref desc, m_Names[0]);
		if (xrMultipassEnabled)
		{
			AllocHistoryFrameRT(m_Ids[1], 2, ref desc, m_Names[1]);
		}
		m_Descriptor = desc;
		m_DescKey = Hash128.Compute(ref desc);
	}

	public override void Reset()
	{
		for (int i = 0; i < m_Ids.Length; i++)
		{
			ReleaseHistoryFrameRT(m_Ids[i]);
		}
	}

	internal RenderTextureDescriptor GetHistoryDescriptor(ref RenderTextureDescriptor cameraDesc)
	{
		RenderTextureDescriptor result = cameraDesc;
		result.mipCount = 0;
		result.msaaSamples = 1;
		return result;
	}

	internal bool Update(ref RenderTextureDescriptor cameraDesc, bool xrMultipassEnabled)
	{
		if (cameraDesc.width > 0 && cameraDesc.height > 0 && (cameraDesc.depthStencilFormat != 0 || cameraDesc.graphicsFormat != 0))
		{
			RenderTextureDescriptor desc = GetHistoryDescriptor(ref cameraDesc);
			if (IsDirty(ref desc))
			{
				Reset();
			}
			if (!IsAllocated())
			{
				Alloc(ref desc, xrMultipassEnabled);
				return true;
			}
		}
		return false;
	}
}
