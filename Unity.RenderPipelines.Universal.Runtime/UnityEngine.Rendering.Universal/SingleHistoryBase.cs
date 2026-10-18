namespace UnityEngine.Rendering.Universal;

internal abstract class SingleHistoryBase : CameraHistoryItem
{
	private int m_Id;

	private RenderTextureDescriptor m_Descriptor;

	private Hash128 m_DescKey;

	public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
	{
		base.OnCreate(owner, typeId);
		m_Id = MakeId(0u);
	}

	public RTHandle GetTexture(int frameIndex = 0)
	{
		if ((uint)frameIndex >= GetHistoryFrameCount())
		{
			return null;
		}
		return base.storage.GetFrameRT(m_Id, frameIndex);
	}

	public RTHandle GetCurrentTexture()
	{
		return GetCurrentFrameRT(m_Id);
	}

	public RTHandle GetPreviousTexture()
	{
		return GetTexture(1);
	}

	internal bool IsAllocated()
	{
		return GetTexture() != null;
	}

	internal bool IsDirty(ref RenderTextureDescriptor desc)
	{
		return m_DescKey != Hash128.Compute(ref desc);
	}

	private void Alloc(ref RenderTextureDescriptor desc)
	{
		AllocHistoryFrameRT(m_Id, GetHistoryFrameCount(), ref desc, GetHistoryName());
		m_Descriptor = desc;
		m_DescKey = Hash128.Compute(ref desc);
	}

	public override void Reset()
	{
		ReleaseHistoryFrameRT(m_Id);
	}

	internal bool Update(ref RenderTextureDescriptor cameraDesc)
	{
		if (cameraDesc.width > 0 && cameraDesc.height > 0 && cameraDesc.graphicsFormat != 0)
		{
			RenderTextureDescriptor desc = GetHistoryDescriptor(ref cameraDesc);
			if (IsDirty(ref desc))
			{
				Reset();
			}
			if (!IsAllocated())
			{
				Alloc(ref desc);
				return true;
			}
		}
		return false;
	}

	protected abstract int GetHistoryFrameCount();

	protected abstract string GetHistoryName();

	protected abstract RenderTextureDescriptor GetHistoryDescriptor(ref RenderTextureDescriptor cameraDesc);
}
