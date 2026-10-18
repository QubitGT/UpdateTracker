using System;
using System.Runtime.InteropServices;

namespace Photon.Voice;

public class ImageBufferNativeAlloc : ImageBufferNative, IDisposable
{
	private ImageBufferNativePool<ImageBufferNativeAlloc> pool;

	public ImageBufferNativeAlloc(ImageBufferNativePool<ImageBufferNativeAlloc> pool, ImageBufferInfo info)
		: base(info)
	{
		this.pool = pool;
		for (int i = 0; i < info.Stride.Length; i++)
		{
			Planes[i] = Marshal.AllocHGlobal(info.Stride[i] * info.Height);
		}
	}

	public override void Release()
	{
		if (pool != null)
		{
			pool.Release(this);
		}
	}

	public override void Dispose()
	{
		for (int i = 0; i < Info.Stride.Length; i++)
		{
			Marshal.FreeHGlobal(Planes[i]);
		}
	}
}
