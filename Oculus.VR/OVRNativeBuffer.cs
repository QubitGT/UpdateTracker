using System;
using System.Runtime.InteropServices;

public class OVRNativeBuffer : IDisposable
{
	protected bool disposed;

	protected int m_numBytes;

	protected IntPtr m_ptr = IntPtr.Zero;

	public OVRNativeBuffer(int numBytes)
	{
		Reallocate(numBytes);
	}

	~OVRNativeBuffer()
	{
		Dispose(disposing: false);
	}

	public void Reset(int numBytes)
	{
		Reallocate(numBytes);
	}

	public int GetCapacity()
	{
		return m_numBytes;
	}

	public IntPtr GetPointer(int byteOffset = 0)
	{
		if (byteOffset < 0 || byteOffset >= m_numBytes)
		{
			return IntPtr.Zero;
		}
		if (byteOffset != 0)
		{
			return new IntPtr(m_ptr.ToInt64() + byteOffset);
		}
		return m_ptr;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected void Dispose(bool disposing)
	{
		if (!disposed)
		{
			Release();
			disposed = true;
		}
	}

	protected void Reallocate(int numBytes)
	{
		Release();
		if (numBytes > 0)
		{
			m_ptr = Marshal.AllocHGlobal(numBytes);
			m_numBytes = numBytes;
		}
		else
		{
			m_ptr = IntPtr.Zero;
			m_numBytes = 0;
		}
	}

	protected void Release()
	{
		if (m_ptr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(m_ptr);
			m_ptr = IntPtr.Zero;
			m_numBytes = 0;
		}
	}
}
