using System;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering.RenderGraphModule;

internal readonly struct ResourceHandle : IEquatable<ResourceHandle>
{
	private const uint kValidityMask = 4294901760u;

	private const uint kIndexMask = 65535u;

	private readonly uint m_Value;

	private readonly int m_Version;

	private readonly RenderGraphResourceType m_Type;

	private static uint s_CurrentValidBit = 65536u;

	private static uint s_SharedResourceValidBit = 2147418112u;

	public int index
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)(m_Value & 0xFFFF);
		}
	}

	public int iType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (int)type;
		}
	}

	public int version
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Version;
		}
	}

	public RenderGraphResourceType type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Type;
		}
	}

	public bool IsVersioned
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Version >= 0;
		}
	}

	internal ResourceHandle(int value, RenderGraphResourceType type, bool shared)
	{
		m_Value = ((uint)value & 0xFFFFu) | (shared ? s_SharedResourceValidBit : s_CurrentValidBit);
		m_Type = type;
		m_Version = -1;
	}

	internal ResourceHandle(in ResourceHandle h, int version)
	{
		m_Value = h.m_Value;
		m_Type = h.type;
		m_Version = version;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsValid()
	{
		uint num = m_Value & 0xFFFF0000u;
		if (num != 0)
		{
			if (num != s_CurrentValidBit)
			{
				return num == s_SharedResourceValidBit;
			}
			return true;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsNull()
	{
		if (index == 0)
		{
			return true;
		}
		return false;
	}

	public static void NewFrame(int executionIndex)
	{
		uint num = s_CurrentValidBit;
		s_CurrentValidBit = (uint)(((executionIndex >> 16) ^ ((executionIndex & 0xFFFF) * 58546883)) << 16);
		if (s_CurrentValidBit == 0 || s_CurrentValidBit == s_SharedResourceValidBit)
		{
			uint num2;
			for (num2 = 1u; num == num2 << 16; num2++)
			{
			}
			s_CurrentValidBit = num2 << 16;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ResourceHandle hdl)
	{
		if (hdl.m_Value == m_Value && hdl.m_Version == m_Version)
		{
			return hdl.type == type;
		}
		return false;
	}
}
