using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

public sealed class WingedEdgeEnumerator : IEnumerator<WingedEdge>, IEnumerator, IDisposable
{
	private WingedEdge m_Start;

	private WingedEdge m_Current;

	public WingedEdge Current
	{
		get
		{
			try
			{
				return m_Current;
			}
			catch (IndexOutOfRangeException)
			{
				throw new InvalidOperationException();
			}
		}
	}

	object IEnumerator.Current => Current;

	public WingedEdgeEnumerator(WingedEdge start)
	{
		m_Start = start;
		m_Current = null;
	}

	public bool MoveNext()
	{
		if (m_Current == null)
		{
			m_Current = m_Start;
			return m_Current != null;
		}
		m_Current = m_Current.next;
		if (m_Current != null)
		{
			return m_Current != m_Start;
		}
		return false;
	}

	public void Reset()
	{
		m_Current = null;
	}

	public void Dispose()
	{
	}
}
