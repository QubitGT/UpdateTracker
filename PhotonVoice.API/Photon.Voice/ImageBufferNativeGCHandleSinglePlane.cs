using System;
using System.Runtime.InteropServices;

namespace Photon.Voice;

public class ImageBufferNativeGCHandleSinglePlane : ImageBufferNative, IDisposable
{
	private ImageBufferNativePool<ImageBufferNativeGCHandleSinglePlane> pool;

	private GCHandle planeHandle;

	public ImageBufferNativeGCHandleSinglePlane(ImageBufferNativePool<ImageBufferNativeGCHandleSinglePlane> pool, ImageBufferInfo info)
		: base(info)
	{
		if (info.Stride.Length != 1)
		{
			throw new Exception("ImageBufferNativeGCHandleSinglePlane wrong plane count " + info.Stride.Length);
		}
		this.pool = pool;
	}

	public void PinPlane(byte[] plane)
	{
		planeHandle = GCHandle.Alloc(plane, GCHandleType.Pinned);
		Planes[0] = planeHandle.AddrOfPinnedObject();
	}

	public override void Release()
	{
		planeHandle.Free();
		if (pool != null)
		{
			pool.Release(this);
		}
	}

	public override void Dispose()
	{
	}
}
