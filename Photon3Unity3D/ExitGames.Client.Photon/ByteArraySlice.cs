using System;

namespace ExitGames.Client.Photon;

public class ByteArraySlice : IDisposable
{
	public byte[] Buffer;

	public int Offset;

	public int Count;

	private readonly ByteArraySlicePool returnPool;

	private readonly int stackIndex;

	internal ByteArraySlice(ByteArraySlicePool returnPool, int stackIndex)
	{
		Buffer = ((stackIndex == 0) ? null : new byte[1 << stackIndex]);
		this.returnPool = returnPool;
		this.stackIndex = stackIndex;
	}

	public ByteArraySlice(byte[] buffer, int offset = 0, int count = 0)
	{
		Buffer = buffer;
		Count = count;
		Offset = offset;
		returnPool = null;
		stackIndex = -1;
	}

	public ByteArraySlice()
	{
		returnPool = null;
		stackIndex = -1;
	}

	public void Dispose()
	{
		Release();
	}

	public bool Release()
	{
		if (stackIndex < 0)
		{
			return false;
		}
		Count = 0;
		Offset = 0;
		return returnPool.Release(this, stackIndex);
	}

	public void Reset()
	{
		Count = 0;
		Offset = 0;
	}
}
