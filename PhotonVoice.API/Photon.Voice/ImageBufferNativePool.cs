namespace Photon.Voice;

public class ImageBufferNativePool<T> : ObjectPool<T, ImageBufferInfo> where T : ImageBufferNative
{
	public delegate T Factory(ImageBufferNativePool<T> pool, ImageBufferInfo info);

	private Factory factory;

	public ImageBufferNativePool(int capacity, Factory factory, string name)
		: base(capacity, name)
	{
		this.factory = factory;
	}

	public ImageBufferNativePool(int capacity, Factory factory, string name, ImageBufferInfo info)
		: base(capacity, name, info)
	{
		this.factory = factory;
	}

	protected override T createObject(ImageBufferInfo info)
	{
		return factory(this, info);
	}

	protected override void destroyObject(T obj)
	{
		obj.Dispose();
	}

	protected override bool infosMatch(ImageBufferInfo i0, ImageBufferInfo i1)
	{
		if (i0.Height != i1.Height)
		{
			return false;
		}
		ImageBufferInfo.StrideSet stride = i0.Stride;
		ImageBufferInfo.StrideSet stride2 = i1.Stride;
		if (stride.Length != stride2.Length)
		{
			return false;
		}
		switch (i0.Stride.Length)
		{
		case 1:
			return stride[0] == stride2[0];
		case 2:
			if (stride[0] == stride2[0])
			{
				return stride[1] == stride2[1];
			}
			return false;
		case 3:
			if (stride[0] == stride2[0] && stride[1] == stride2[1])
			{
				return stride[2] == stride2[2];
			}
			return false;
		default:
		{
			for (int j = 0; j < stride.Length; j++)
			{
				if (stride[j] != stride2[j])
				{
					return false;
				}
			}
			return true;
		}
		}
	}
}
