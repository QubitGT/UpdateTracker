using System;
using System.Runtime.InteropServices;

namespace Unity.Media;

public class RefHandle<T> : IDisposable where T : class
{
	private GCHandle m_Handle;

	private bool Disposed;

	public bool IsCreated => m_Handle.IsAllocated;

	public T Target
	{
		get
		{
			if (!IsCreated)
			{
				return null;
			}
			return m_Handle.Target as T;
		}
		set
		{
			if (IsCreated)
			{
				m_Handle.Free();
			}
			if (value != null)
			{
				m_Handle = GCHandle.Alloc(value, GCHandleType.Normal);
			}
		}
	}

	public RefHandle()
	{
	}

	public RefHandle(T target)
	{
		m_Handle = default(GCHandle);
		Target = target;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			if (IsCreated)
			{
				m_Handle.Free();
			}
			Disposed = true;
		}
	}

	~RefHandle()
	{
		Dispose(disposing: false);
	}
}
