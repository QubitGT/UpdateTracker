using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace g3;

public class DVector<T> : IEnumerable<T>, IEnumerable
{
	public struct DBlock
	{
		public T[] data;

		public int usedCount;
	}

	private List<T[]> Blocks;

	private int iCurBlock;

	private int iCurBlockUsed;

	private int nBlockSize = 2048;

	private const int nShiftBits = 11;

	private const int nBlockIndexBitmask = 2047;

	public int Length => iCurBlock * nBlockSize + iCurBlockUsed;

	public int BlockCount => nBlockSize;

	public int size => Length;

	public bool empty
	{
		get
		{
			if (iCurBlock == 0)
			{
				return iCurBlockUsed == 0;
			}
			return false;
		}
	}

	public int MemoryUsageBytes
	{
		get
		{
			if (Blocks.Count != 0)
			{
				return Blocks.Count * nBlockSize * Marshal.SizeOf(Blocks[0][0]);
			}
			return 0;
		}
	}

	public T this[int i]
	{
		get
		{
			return Blocks[i >> 11][i & 0x7FF];
		}
		set
		{
			Blocks[i >> 11][i & 0x7FF] = value;
		}
	}

	public T back
	{
		get
		{
			return Blocks[iCurBlock][iCurBlockUsed - 1];
		}
		set
		{
			Blocks[iCurBlock][iCurBlockUsed - 1] = value;
		}
	}

	public T front
	{
		get
		{
			return Blocks[0][0];
		}
		set
		{
			Blocks[0][0] = value;
		}
	}

	public DVector()
	{
		iCurBlock = 0;
		iCurBlockUsed = 0;
		Blocks = new List<T[]>();
		Blocks.Add(new T[nBlockSize]);
	}

	public DVector(DVector<T> copy)
	{
		nBlockSize = copy.nBlockSize;
		iCurBlock = copy.iCurBlock;
		iCurBlockUsed = copy.iCurBlockUsed;
		Blocks = new List<T[]>();
		for (int i = 0; i < copy.Blocks.Count; i++)
		{
			Blocks.Add(new T[nBlockSize]);
			Array.Copy(copy.Blocks[i], Blocks[i], copy.Blocks[i].Length);
		}
	}

	public DVector(T[] data)
	{
		Initialize(data);
	}

	public DVector(IEnumerable<T> init)
	{
		iCurBlock = 0;
		iCurBlockUsed = 0;
		Blocks = new List<T[]>();
		Blocks.Add(new T[nBlockSize]);
		foreach (T item in init)
		{
			Add(item);
		}
	}

	public void Add(T value)
	{
		if (iCurBlockUsed == nBlockSize)
		{
			if (iCurBlock == Blocks.Count - 1)
			{
				Blocks.Add(new T[nBlockSize]);
			}
			iCurBlock++;
			iCurBlockUsed = 0;
		}
		Blocks[iCurBlock][iCurBlockUsed] = value;
		iCurBlockUsed++;
	}

	public void Add(T value, int nRepeat)
	{
		for (int i = 0; i < nRepeat; i++)
		{
			Add(value);
		}
	}

	public void Add(T[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			Add(values[i]);
		}
	}

	public void Add(T[] values, int nRepeat)
	{
		for (int i = 0; i < nRepeat; i++)
		{
			for (int j = 0; j < values.Length; j++)
			{
				Add(values[j]);
			}
		}
	}

	public void push_back(T value)
	{
		Add(value);
	}

	public void pop_back()
	{
		if (iCurBlockUsed > 0)
		{
			iCurBlockUsed--;
		}
		if (iCurBlockUsed == 0 && iCurBlock > 0)
		{
			iCurBlock--;
			iCurBlockUsed = nBlockSize;
		}
	}

	public void insert(T value, int index)
	{
		insertAt(value, index);
	}

	public void insertAt(T value, int index)
	{
		int num = size;
		if (index == num)
		{
			push_back(value);
		}
		else if (index > num)
		{
			resize(index);
			push_back(value);
		}
		else
		{
			this[index] = value;
		}
	}

	public void resize(int count)
	{
		if (Length != count)
		{
			int num = 1 + count / nBlockSize;
			int count2 = Blocks.Count;
			for (int i = num; i < count2; i++)
			{
				Blocks[i] = null;
			}
			if (num >= Blocks.Count)
			{
				Blocks.Capacity = num;
			}
			else
			{
				Blocks.RemoveRange(num, Blocks.Count - num);
			}
			for (int j = count2; j < num; j++)
			{
				Blocks.Add(new T[nBlockSize]);
			}
			iCurBlockUsed = count - (num - 1) * nBlockSize;
			iCurBlock = num - 1;
		}
	}

	public void copy(DVector<T> copyIn)
	{
		if (Blocks != null && copyIn.Blocks.Count == Blocks.Count)
		{
			int count = copyIn.Blocks.Count;
			for (int i = 0; i < count; i++)
			{
				Array.Copy(copyIn.Blocks[i], Blocks[i], copyIn.Blocks[i].Length);
			}
			iCurBlock = copyIn.iCurBlock;
			iCurBlockUsed = copyIn.iCurBlockUsed;
			return;
		}
		resize(copyIn.size);
		int count2 = copyIn.Blocks.Count;
		for (int j = 0; j < count2; j++)
		{
			Array.Copy(copyIn.Blocks[j], Blocks[j], copyIn.Blocks[j].Length);
		}
		iCurBlock = copyIn.iCurBlock;
		iCurBlockUsed = copyIn.iCurBlockUsed;
	}

	public void GetBuffer(T[] data)
	{
		int length = Length;
		for (int i = 0; i < length; i++)
		{
			data[i] = this[i];
		}
	}

	public T[] GetBuffer()
	{
		T[] array = new T[Length];
		for (int i = 0; i < Length; i++)
		{
			array[i] = this[i];
		}
		return array;
	}

	public T[] ToArray()
	{
		return GetBuffer();
	}

	public T2[] GetBufferCast<T2>()
	{
		T2[] array = new T2[Length];
		for (int i = 0; i < Length; i++)
		{
			array[i] = (T2)Convert.ChangeType(this[i], typeof(T2));
		}
		return array;
	}

	public byte[] GetBytes()
	{
		int num = Marshal.SizeOf(typeof(T));
		byte[] array = new byte[Length * num];
		int num2 = 0;
		int count = Blocks.Count;
		for (int i = 0; i < count - 1; i++)
		{
			Buffer.BlockCopy(Blocks[i], 0, array, num2, nBlockSize * num);
			num2 += nBlockSize * num;
		}
		Buffer.BlockCopy(Blocks[count - 1], 0, array, num2, iCurBlockUsed * num);
		return array;
	}

	public void Initialize(T[] data)
	{
		int num = data.Length / nBlockSize;
		Blocks = new List<T[]>();
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			T[] array = new T[nBlockSize];
			Array.Copy(data, num2, array, 0, nBlockSize);
			Blocks.Add(array);
			num2 += nBlockSize;
		}
		iCurBlockUsed = data.Length - num2;
		if (iCurBlockUsed != 0)
		{
			T[] array2 = new T[nBlockSize];
			Array.Copy(data, num2, array2, 0, iCurBlockUsed);
			Blocks.Add(array2);
		}
		else
		{
			iCurBlockUsed = nBlockSize;
		}
		iCurBlock = Blocks.Count - 1;
	}

	public void Clear()
	{
		foreach (T[] block in Blocks)
		{
			Array.Clear(block, 0, block.Length);
		}
	}

	public void Apply(Action<T, int> applyF)
	{
		for (int i = 0; i < iCurBlock; i++)
		{
			T[] array = Blocks[i];
			for (int j = 0; j < nBlockSize; j++)
			{
				applyF(array[j], j);
			}
		}
		T[] array2 = Blocks[iCurBlock];
		for (int k = 0; k < iCurBlockUsed; k++)
		{
			applyF(array2[k], k);
		}
	}

	public void ApplyReplace(Func<T, int, T> applyF)
	{
		for (int i = 0; i < iCurBlock; i++)
		{
			T[] array = Blocks[i];
			for (int j = 0; j < nBlockSize; j++)
			{
				array[j] = applyF(array[j], j);
			}
		}
		T[] array2 = Blocks[iCurBlock];
		for (int k = 0; k < iCurBlockUsed; k++)
		{
			array2[k] = applyF(array2[k], k);
		}
	}

	public unsafe static void FastGetBuffer(DVector<double> v, double* pBuffer)
	{
		IntPtr destination = new IntPtr(pBuffer);
		int count = v.Blocks.Count;
		for (int i = 0; i < count - 1; i++)
		{
			Marshal.Copy(v.Blocks[i], 0, destination, v.nBlockSize);
			destination = new IntPtr(destination.ToInt64() + v.nBlockSize * 8);
		}
		Marshal.Copy(v.Blocks[count - 1], 0, destination, v.iCurBlockUsed);
	}

	public unsafe static void FastGetBuffer(DVector<float> v, float* pBuffer)
	{
		IntPtr destination = new IntPtr(pBuffer);
		int count = v.Blocks.Count;
		for (int i = 0; i < count - 1; i++)
		{
			Marshal.Copy(v.Blocks[i], 0, destination, v.nBlockSize);
			destination = new IntPtr(destination.ToInt64() + v.nBlockSize * 4);
		}
		Marshal.Copy(v.Blocks[count - 1], 0, destination, v.iCurBlockUsed);
	}

	public unsafe static void FastGetBuffer(DVector<int> v, int* pBuffer)
	{
		IntPtr destination = new IntPtr(pBuffer);
		int count = v.Blocks.Count;
		for (int i = 0; i < count - 1; i++)
		{
			Marshal.Copy(v.Blocks[i], 0, destination, v.nBlockSize);
			destination = new IntPtr(destination.ToInt64() + v.nBlockSize * 4);
		}
		Marshal.Copy(v.Blocks[count - 1], 0, destination, v.iCurBlockUsed);
	}

	public IEnumerator<T> GetEnumerator()
	{
		int bi2 = 0;
		while (bi2 < iCurBlock)
		{
			T[] block = Blocks[bi2];
			int num;
			for (int i = 0; i < nBlockSize; i = num)
			{
				yield return block[i];
				num = i + 1;
			}
			num = bi2 + 1;
			bi2 = num;
		}
		T[] lastblock = Blocks[iCurBlock];
		bi2 = 0;
		while (bi2 < iCurBlockUsed)
		{
			yield return lastblock[bi2];
			int num = bi2 + 1;
			bi2 = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerable<DBlock> BlockIterator()
	{
		int i = 0;
		while (i < iCurBlock)
		{
			yield return new DBlock
			{
				data = Blocks[i],
				usedCount = nBlockSize
			};
			int num = i + 1;
			i = num;
		}
		yield return new DBlock
		{
			data = Blocks[iCurBlock],
			usedCount = iCurBlockUsed
		};
	}
}
