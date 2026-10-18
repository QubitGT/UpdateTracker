using System;

namespace Liv.NGFX;

public class Context : IDisposable
{
	private IntPtr m_context = IntPtr.Zero;

	private bool m_valid;

	public IntPtr ptr => m_context;

	public bool valid => m_valid;

	public Context()
	{
		m_context = NI.ngfx_create_context();
		m_valid = m_context != IntPtr.Zero;
	}

	~Context()
	{
		Dispose();
	}

	public static implicit operator IntPtr(Context c)
	{
		return c.m_context;
	}

	public void Dispose()
	{
		if (m_valid)
		{
			NI.ngfx_destroy_context(m_context);
			m_context = IntPtr.Zero;
			m_valid = false;
		}
	}
}
