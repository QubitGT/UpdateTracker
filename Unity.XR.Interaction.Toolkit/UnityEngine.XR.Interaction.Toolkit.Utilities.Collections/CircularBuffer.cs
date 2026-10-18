using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Collections;

internal class CircularBuffer<T>
{
	private readonly T[] m_Buffer;

	private int m_Start;

	private int m_Count;

	public int count => m_Count;

	public int capacity => m_Buffer.Length;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= m_Count)
			{
				throw new IndexOutOfRangeException();
			}
			return m_Buffer[(m_Start + index) % m_Buffer.Length];
		}
	}

	public CircularBuffer(int capacity)
	{
		m_Buffer = new T[capacity];
		m_Start = 0;
		m_Count = 0;
	}

	public void Add(T item)
	{
		int num = (m_Start + m_Count) % m_Buffer.Length;
		m_Buffer[num] = item;
		if (m_Count < m_Buffer.Length)
		{
			m_Count++;
		}
		else
		{
			m_Start = (m_Start + 1) % m_Buffer.Length;
		}
	}

	public void Clear()
	{
		m_Start = 0;
		m_Count = 0;
	}
}
