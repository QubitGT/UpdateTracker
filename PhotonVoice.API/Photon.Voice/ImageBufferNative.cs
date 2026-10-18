using System;

namespace Photon.Voice;

public class ImageBufferNative
{
	public struct PlaneSet
	{
		private IntPtr plane0;

		private IntPtr plane1;

		private IntPtr plane2;

		private IntPtr plane3;

		public IntPtr this[int key]
		{
			get
			{
				return key switch
				{
					0 => plane0, 
					1 => plane1, 
					2 => plane2, 
					3 => plane3, 
					_ => IntPtr.Zero, 
				};
			}
			set
			{
				switch (key)
				{
				case 0:
					plane0 = value;
					break;
				case 1:
					plane1 = value;
					break;
				case 2:
					plane2 = value;
					break;
				case 3:
					plane3 = value;
					break;
				}
			}
		}

		public int Length { get; private set; }

		public PlaneSet(int length, IntPtr p0 = default(IntPtr), IntPtr p1 = default(IntPtr), IntPtr p2 = default(IntPtr), IntPtr p3 = default(IntPtr))
		{
			Length = length;
			plane0 = p0;
			plane1 = p1;
			plane2 = p2;
			plane3 = p3;
		}
	}

	public ImageBufferInfo Info;

	public PlaneSet Planes;

	public ImageBufferNative(ImageBufferInfo info)
	{
		Info = info;
		Planes = new PlaneSet(info.Stride.Length, (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)0);
	}

	public ImageBufferNative(IntPtr buf, int width, int height, int stride, ImageFormat imageFormat)
	{
		Info = new ImageBufferInfo(width, height, new ImageBufferInfo.StrideSet(1, stride), imageFormat);
		Planes = new PlaneSet(1, buf, (IntPtr)0, (IntPtr)0, (IntPtr)0);
	}

	public virtual void Release()
	{
	}

	public virtual void Dispose()
	{
	}
}
