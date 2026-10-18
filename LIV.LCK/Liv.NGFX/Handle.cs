using System;
using System.Runtime.InteropServices;

namespace Liv.NGFX;

public class Handle<T> : IDisposable
{
	private T m_data;

	private GCHandle m_handle;

	private bool m_valid;

	public Handle(T data)
	{
		m_data = data;
		m_handle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
		m_valid = true;
	}

	~Handle()
	{
		Dispose();
	}

	public IntPtr ptr()
	{
		return m_handle.AddrOfPinnedObject();
	}

	public T data()
	{
		return m_data;
	}

	public void Dispose()
	{
		if (m_valid)
		{
			m_handle.Free();
		}
		m_valid = false;
	}
}
