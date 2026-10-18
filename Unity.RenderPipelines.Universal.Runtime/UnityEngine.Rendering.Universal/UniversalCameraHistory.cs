using System;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

public class UniversalCameraHistory : ICameraHistoryReadAccess, ICameraHistoryWriteAccess, IPerFrameHistoryAccessTracker, IDisposable
{
	private static class TypeId<T>
	{
		public static uint value = s_TypeCount++;
	}

	private struct Item
	{
		public ContextItem storage;

		public int requestVersion;

		public int writeVersion;

		public void Reset()
		{
			storage?.Reset();
			requestVersion = -2;
			writeVersion = -2;
		}
	}

	private const int k_ValidVersionCount = 2;

	private static uint s_TypeCount;

	private Item[] m_Items = new Item[32];

	private int m_Version;

	private BufferedRTHandleSystem m_HistoryTextures = new BufferedRTHandleSystem();

	public event ICameraHistoryReadAccess.HistoryRequestDelegate OnGatherHistoryRequests;

	public void RequestAccess<Type>() where Type : ContextItem
	{
		uint value = TypeId<Type>.value;
		if (value >= m_Items.Length)
		{
			Item[] array = new Item[math.max(math.ceilpow2(s_TypeCount), m_Items.Length * 2)];
			for (int i = 0; i < m_Items.Length; i++)
			{
				array[i] = m_Items[i];
			}
			m_Items = array;
		}
		m_Items[value].requestVersion = m_Version;
	}

	public Type GetHistoryForRead<Type>() where Type : ContextItem
	{
		uint value = TypeId<Type>.value;
		if (value >= m_Items.Length)
		{
			return null;
		}
		if (!IsValid((int)value))
		{
			return null;
		}
		return (Type)m_Items[value].storage;
	}

	public bool IsAccessRequested<Type>() where Type : ContextItem
	{
		uint value = TypeId<Type>.value;
		if (value >= m_Items.Length)
		{
			return false;
		}
		return IsValidRequest((int)value);
	}

	public Type GetHistoryForWrite<Type>() where Type : ContextItem, new()
	{
		uint value = TypeId<Type>.value;
		if (value >= m_Items.Length)
		{
			return null;
		}
		if (!IsValidRequest((int)value))
		{
			return null;
		}
		if (m_Items[value].storage == null)
		{
			ref Item reference = ref m_Items[value];
			reference.storage = new Type();
			if (reference.storage is CameraHistoryItem cameraHistoryItem)
			{
				cameraHistoryItem.OnCreate(m_HistoryTextures, value);
			}
		}
		m_Items[value].writeVersion = m_Version;
		return (Type)m_Items[value].storage;
	}

	public bool IsWritten<Type>() where Type : ContextItem
	{
		uint value = TypeId<Type>.value;
		if (value >= m_Items.Length)
		{
			return false;
		}
		return m_Items[value].writeVersion == m_Version;
	}

	internal UniversalCameraHistory()
	{
		for (int i = 0; i < m_Items.Length; i++)
		{
			m_Items[i].Reset();
		}
	}

	public void Dispose()
	{
		for (int i = 0; i < m_Items.Length; i++)
		{
			m_Items[i].Reset();
		}
		m_HistoryTextures.ReleaseAll();
	}

	internal void GatherHistoryRequests()
	{
		this.OnGatherHistoryRequests?.Invoke(this);
	}

	private bool IsValidRequest(int i)
	{
		return m_Version - m_Items[i].requestVersion < 2;
	}

	private bool IsValid(int i)
	{
		return m_Version - m_Items[i].writeVersion < 2;
	}

	internal void ReleaseUnusedHistory()
	{
		for (int i = 0; i < m_Items.Length; i++)
		{
			if (!IsValidRequest(i) && !IsValid(i))
			{
				m_Items[i].Reset();
			}
		}
		m_Version++;
	}

	internal void SwapAndSetReferenceSize(int cameraWidth, int cameraHeight)
	{
		m_HistoryTextures.SwapAndSetReferenceSize(cameraWidth, cameraHeight);
	}
}
