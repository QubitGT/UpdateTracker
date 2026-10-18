namespace UnityEngine.Rendering;

public abstract class CameraHistoryItem : ContextItem
{
	private BufferedRTHandleSystem m_owner;

	private uint m_TypeId = uint.MaxValue;

	protected BufferedRTHandleSystem storage => m_owner;

	public virtual void OnCreate(BufferedRTHandleSystem owner, uint typeId)
	{
		m_owner = owner;
		m_TypeId = typeId;
	}

	protected int MakeId(uint index)
	{
		return (int)(((m_TypeId & 0xFFFF) << 16) | (index & 0xFFFF));
	}

	protected RTHandle AllocHistoryFrameRT(int id, int count, ref RenderTextureDescriptor desc, string name = "")
	{
		return AllocHistoryFrameRT(id, count, ref desc, FilterMode.Bilinear, name);
	}

	protected RTHandle AllocHistoryFrameRT(int id, int count, ref RenderTextureDescriptor desc, FilterMode filterMode, string name = "")
	{
		m_owner.AllocBuffer(id, count, ref desc, filterMode, TextureWrapMode.Clamp, isShadowMap: false, 0, 0f, name);
		return GetCurrentFrameRT(0);
	}

	protected void ReleaseHistoryFrameRT(int id)
	{
		m_owner.ReleaseBuffer(id);
	}

	protected RTHandle GetPreviousFrameRT(int id)
	{
		return m_owner.GetFrameRT(id, 1);
	}

	protected RTHandle GetCurrentFrameRT(int id)
	{
		return m_owner.GetFrameRT(id, 0);
	}
}
