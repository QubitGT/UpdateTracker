using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal.Internal;

internal sealed class RenderTargetBufferSystem
{
	private struct SwapBuffer
	{
		public RTHandle rtMSAA;

		public RTHandle rtResolve;

		public string name;

		public int msaa;
	}

	private SwapBuffer m_A;

	private SwapBuffer m_B;

	private static bool m_AisBackBuffer = true;

	private static RenderTextureDescriptor m_Desc;

	private FilterMode m_FilterMode;

	private bool m_AllowMSAA = true;

	private ref SwapBuffer backBuffer
	{
		get
		{
			if (!m_AisBackBuffer)
			{
				return ref m_B;
			}
			return ref m_A;
		}
	}

	private ref SwapBuffer frontBuffer
	{
		get
		{
			if (!m_AisBackBuffer)
			{
				return ref m_A;
			}
			return ref m_B;
		}
	}

	public RenderTargetBufferSystem(string name)
	{
		m_A.name = name + "A";
		m_B.name = name + "B";
	}

	public void Dispose()
	{
		m_A.rtMSAA?.Release();
		m_B.rtMSAA?.Release();
		m_A.rtResolve?.Release();
		m_B.rtResolve?.Release();
	}

	public RTHandle PeekBackBuffer()
	{
		if (!m_AllowMSAA || backBuffer.msaa <= 1)
		{
			return backBuffer.rtResolve;
		}
		return backBuffer.rtMSAA;
	}

	public RTHandle GetBackBuffer(CommandBuffer cmd)
	{
		ReAllocate(cmd);
		return PeekBackBuffer();
	}

	public RTHandle GetFrontBuffer(CommandBuffer cmd)
	{
		if (!m_AllowMSAA && frontBuffer.msaa > 1)
		{
			frontBuffer.msaa = 1;
		}
		ReAllocate(cmd);
		if (!m_AllowMSAA || frontBuffer.msaa <= 1)
		{
			return frontBuffer.rtResolve;
		}
		return frontBuffer.rtMSAA;
	}

	public void Swap()
	{
		m_AisBackBuffer = !m_AisBackBuffer;
	}

	private void ReAllocate(CommandBuffer cmd)
	{
		RenderTextureDescriptor descriptor = m_Desc;
		descriptor.msaaSamples = m_A.msaa;
		if (descriptor.msaaSamples > 1)
		{
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_A.rtMSAA, in descriptor, m_FilterMode, TextureWrapMode.Clamp, 1, 0f, m_A.name);
		}
		descriptor.msaaSamples = m_B.msaa;
		if (descriptor.msaaSamples > 1)
		{
			RenderingUtils.ReAllocateHandleIfNeeded(ref m_B.rtMSAA, in descriptor, m_FilterMode, TextureWrapMode.Clamp, 1, 0f, m_B.name);
		}
		descriptor.msaaSamples = 1;
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_A.rtResolve, in descriptor, m_FilterMode, TextureWrapMode.Clamp, 1, 0f, m_A.name);
		RenderingUtils.ReAllocateHandleIfNeeded(ref m_B.rtResolve, in descriptor, m_FilterMode, TextureWrapMode.Clamp, 1, 0f, m_B.name);
		cmd.SetGlobalTexture(m_A.name, m_A.rtResolve);
		cmd.SetGlobalTexture(m_B.name, m_B.rtResolve);
	}

	public void Clear()
	{
		m_AisBackBuffer = true;
		m_AllowMSAA = m_A.msaa > 1 || m_B.msaa > 1;
	}

	public void SetCameraSettings(RenderTextureDescriptor desc, FilterMode filterMode)
	{
		desc.depthStencilFormat = GraphicsFormat.None;
		m_Desc = desc;
		m_FilterMode = filterMode;
		m_A.msaa = m_Desc.msaaSamples;
		m_B.msaa = m_Desc.msaaSamples;
		if (m_Desc.msaaSamples > 1)
		{
			EnableMSAA(enable: true);
		}
	}

	public RTHandle GetBufferA()
	{
		if (!m_AllowMSAA || m_A.msaa <= 1)
		{
			return m_A.rtResolve;
		}
		return m_A.rtMSAA;
	}

	public void EnableMSAA(bool enable)
	{
		m_AllowMSAA = enable;
		if (enable)
		{
			m_A.msaa = m_Desc.msaaSamples;
			m_B.msaa = m_Desc.msaaSamples;
		}
	}
}
