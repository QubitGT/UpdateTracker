using System;
using System.Text;

namespace UnityEngine.Rendering.Universal;

internal struct ShaderBitArray
{
	private const int k_BitsPerElement = 32;

	private const int k_ElementShift = 5;

	private const int k_ElementMask = 31;

	private float[] m_Data;

	public int elemLength
	{
		get
		{
			if (m_Data != null)
			{
				return m_Data.Length;
			}
			return 0;
		}
	}

	public int bitCapacity => elemLength * 32;

	public float[] data => m_Data;

	public unsafe bool this[int index]
	{
		get
		{
			GetElementIndexAndBitOffset(index, out var elemIndex, out var bitOffset);
			fixed (float* ptr = m_Data)
			{
				uint* ptr2 = (uint*)(ptr + elemIndex);
				return (*ptr2 & (uint)(1 << bitOffset)) != 0;
			}
		}
		set
		{
			GetElementIndexAndBitOffset(index, out var elemIndex, out var bitOffset);
			fixed (float* ptr = m_Data)
			{
				uint* ptr2 = (uint*)(ptr + elemIndex);
				if (value)
				{
					*ptr2 |= (uint)(1 << bitOffset);
				}
				else
				{
					*ptr2 &= (uint)(~(1 << bitOffset));
				}
			}
		}
	}

	public void Resize(int bitCount)
	{
		if (bitCapacity > bitCount)
		{
			return;
		}
		int num = (bitCount + 31) / 32;
		if (num == m_Data?.Length)
		{
			return;
		}
		float[] array = new float[num];
		if (m_Data != null)
		{
			for (int i = 0; i < m_Data.Length; i++)
			{
				array[i] = m_Data[i];
			}
		}
		m_Data = array;
	}

	public void Clear()
	{
		for (int i = 0; i < m_Data.Length; i++)
		{
			m_Data[i] = 0f;
		}
	}

	private void GetElementIndexAndBitOffset(int index, out int elemIndex, out int bitOffset)
	{
		elemIndex = index >> 5;
		bitOffset = index & 0x1F;
	}

	public unsafe override string ToString()
	{
		int num = Math.Min(bitCapacity, 4096);
		byte* ptr = stackalloc byte[(int)(uint)num];
		for (int i = 0; i < num; i++)
		{
			ptr[i] = (byte)(this[i] ? 49u : 48u);
		}
		return new string((sbyte*)ptr, 0, num, Encoding.UTF8);
	}
}
