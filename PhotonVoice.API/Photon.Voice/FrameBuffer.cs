using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Photon.Voice;

public struct FrameBuffer
{
	private readonly byte[] array;

	private readonly int offset;

	private readonly int count;

	private readonly IDisposable disposer;

	private bool disposed;

	private int refCnt;

	private GCHandle gcHandle;

	private IntPtr ptr;

	private bool pinned;

	internal static int statDisposerCreated;

	internal static int statDisposerDisposed;

	internal static int statPinned;

	internal static int statUnpinned;

	public IntPtr Ptr
	{
		get
		{
			if (!pinned)
			{
				gcHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
				ptr = IntPtr.Add(gcHandle.AddrOfPinnedObject(), offset);
				pinned = true;
				Interlocked.Increment(ref statPinned);
			}
			return ptr;
		}
	}

	public byte[] Array => array;

	public int Length => count;

	public int Offset => offset;

	public FrameFlags Flags { get; }

	public FrameBuffer(byte[] array, int offset, int count, FrameFlags flags, IDisposable disposer)
	{
		this.array = array;
		this.offset = offset;
		this.count = count;
		Flags = flags;
		this.disposer = disposer;
		disposed = false;
		refCnt = 1;
		gcHandle = default(GCHandle);
		ptr = IntPtr.Zero;
		pinned = false;
		if (disposer != null)
		{
			Interlocked.Increment(ref statDisposerCreated);
		}
	}

	public FrameBuffer(byte[] array, FrameFlags flags)
	{
		this.array = array;
		offset = 0;
		count = ((array != null) ? array.Length : 0);
		Flags = flags;
		disposer = null;
		disposed = false;
		refCnt = 1;
		gcHandle = default(GCHandle);
		ptr = IntPtr.Zero;
		pinned = false;
		if (disposer != null)
		{
			Interlocked.Increment(ref statDisposerCreated);
		}
	}

	public void Retain()
	{
		refCnt++;
	}

	public void Release()
	{
		refCnt--;
		if (refCnt <= 0)
		{
			Dispose();
		}
	}

	private void Dispose()
	{
		if (pinned)
		{
			gcHandle.Free();
			pinned = false;
			Interlocked.Increment(ref statUnpinned);
		}
		if (disposer != null && !disposed)
		{
			disposer.Dispose();
			disposed = true;
			Interlocked.Increment(ref statDisposerDisposed);
		}
	}
}
