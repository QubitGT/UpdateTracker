using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MothershipBanDataVector : IDisposable, IEnumerable, IEnumerable<MothershipBanData>
{
	public sealed class MothershipBanDataVectorEnumerator : IEnumerator, IEnumerator<MothershipBanData>, IDisposable
	{
		private MothershipBanDataVector collectionRef;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public MothershipBanData Current
		{
			get
			{
				if (currentIndex == -1)
				{
					throw new InvalidOperationException("Enumeration not started.");
				}
				if (currentIndex > currentSize - 1)
				{
					throw new InvalidOperationException("Enumeration finished.");
				}
				if (currentObject == null)
				{
					throw new InvalidOperationException("Collection modified.");
				}
				return (MothershipBanData)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public MothershipBanDataVectorEnumerator(MothershipBanDataVector collection)
		{
			collectionRef = collection;
			currentIndex = -1;
			currentObject = null;
			currentSize = collectionRef.Count;
		}

		public bool MoveNext()
		{
			int count = collectionRef.Count;
			int num;
			if (currentIndex + 1 < count)
			{
				num = ((count == currentSize) ? 1 : 0);
				if (num != 0)
				{
					currentIndex++;
					currentObject = collectionRef[currentIndex];
					return (byte)num != 0;
				}
			}
			else
			{
				num = 0;
			}
			currentObject = null;
			return (byte)num != 0;
		}

		public void Reset()
		{
			currentIndex = -1;
			currentObject = null;
			if (collectionRef.Count != currentSize)
			{
				throw new InvalidOperationException("Collection modified.");
			}
		}

		public void Dispose()
		{
			currentIndex = -1;
			currentObject = null;
		}
	}

	private HandleRef swigCPtr;

	protected bool swigCMemOwn;

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public MothershipBanData this[int index]
	{
		get
		{
			if (index < 0 || index > Count)
			{
				throw new ArgumentOutOfRangeException("Tried to get an out of bounds element from this vector");
			}
			return getitem(index);
		}
		set
		{
			if (index < 0 || index > Count)
			{
				throw new ArgumentOutOfRangeException("Tried to set an out of bounds element from this vector");
			}
			setitem(index, value);
		}
	}

	public int Capacity
	{
		get
		{
			return (int)capacity();
		}
		set
		{
			if (value < 0 || (uint)value < size())
			{
				throw new ArgumentOutOfRangeException("Capacity");
			}
			reserve((uint)value);
		}
	}

	public bool IsEmpty => empty();

	public int Count => (int)size();

	public bool IsSynchronized => false;

	internal MothershipBanDataVector(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MothershipBanDataVector obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MothershipBanDataVector obj)
	{
		if (obj != null)
		{
			if (!obj.swigCMemOwn)
			{
				throw new ApplicationException("Cannot release ownership as memory is not owned");
			}
			HandleRef result = obj.swigCPtr;
			obj.swigCMemOwn = false;
			obj.Dispose();
			return result;
		}
		return new HandleRef(null, IntPtr.Zero);
	}

	~MothershipBanDataVector()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		lock (this)
		{
			if (swigCPtr.Handle != IntPtr.Zero)
			{
				if (swigCMemOwn)
				{
					swigCMemOwn = false;
					MothershipApiPINVOKE.delete_MothershipBanDataVector(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public MothershipBanDataVector(IEnumerable c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipBanData item in c)
		{
			Add(item);
		}
	}

	public MothershipBanDataVector(IEnumerable<MothershipBanData> c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipBanData item in c)
		{
			Add(item);
		}
	}

	public void Insert(int index, MothershipBanData value)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to insert at an out of bounds index in this vector");
		}
		_insert(index, value);
	}

	public void InsertRange(int index, MothershipBanDataVector values)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to insert at an out of bounds index in this vector");
		}
		_insertRange(index, values);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to remove an out of bounds index in this vector");
		}
		_removeAt(index);
	}

	public void RemoveRange(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to remove an out of bounds index in this vector");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Tried to remove a negative count from this vector");
		}
		if (index < 0 || index > Count || index + count > Count)
		{
			throw new ArgumentOutOfRangeException("invalid range");
		}
		_removeRange(index, count);
	}

	public void ReverseRange(int index, int count)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to reverse from an out of bounds index in this vector");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to reverse an invalid count in this vector");
		}
		if (index < 0 || index > Count || index + count > Count)
		{
			throw new ArgumentOutOfRangeException("invalid range");
		}
		_reverseRange(index, count);
	}

	public void SetRange(int index, MothershipBanDataVector values)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to set an out of bounds index in this vector");
		}
		if (index < 0 || index > Count || index + values.Count > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to set an out of bounds index in this vector");
		}
		_setRange(index, values);
	}

	public void CopyTo(MothershipBanData[] array)
	{
		CopyTo(0, array, 0, Count);
	}

	public void CopyTo(MothershipBanData[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, Count);
	}

	public void CopyTo(int index, MothershipBanData[] array, int arrayIndex, int count)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", "Value is less than zero");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count", "Value is less than zero");
		}
		if (array.Rank > 1)
		{
			throw new ArgumentException("Multi dimensional array.", "array");
		}
		if (index + count > Count || arrayIndex + count > array.Length)
		{
			throw new ArgumentException("Number of elements to copy is too large.");
		}
		for (int i = 0; i < count; i++)
		{
			if (i > array.Length - 1)
			{
				throw new ArgumentOutOfRangeException("Somehow we tried to copy to an array index that's out of bounds");
			}
			array.SetValue(getitemcopy(index + i), arrayIndex + i);
		}
	}

	public MothershipBanData[] ToArray()
	{
		MothershipBanData[] array = new MothershipBanData[Count];
		CopyTo(array);
		return array;
	}

	IEnumerator<MothershipBanData> IEnumerable<MothershipBanData>.GetEnumerator()
	{
		return new MothershipBanDataVectorEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new MothershipBanDataVectorEnumerator(this);
	}

	public MothershipBanDataVectorEnumerator GetEnumerator()
	{
		return new MothershipBanDataVectorEnumerator(this);
	}

	public MothershipBanDataVector()
		: this(MothershipApiPINVOKE.new_MothershipBanDataVector__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MothershipBanDataVector(MothershipBanDataVector other)
		: this(MothershipApiPINVOKE.new_MothershipBanDataVector__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Clear()
	{
		MothershipApiPINVOKE.MothershipBanDataVector_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Add(MothershipBanData x)
	{
		MothershipApiPINVOKE.MothershipBanDataVector_Add(swigCPtr, MothershipBanData.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.MothershipBanDataVector_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool empty()
	{
		bool result = MothershipApiPINVOKE.MothershipBanDataVector_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint capacity()
	{
		uint result = MothershipApiPINVOKE.MothershipBanDataVector_capacity(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void reserve(uint n)
	{
		MothershipApiPINVOKE.MothershipBanDataVector_reserve(swigCPtr, n);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MothershipBanDataVector(int capacity)
		: this(MothershipApiPINVOKE.new_MothershipBanDataVector__SWIG_2(capacity), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MothershipBanData getitemcopy(int index)
	{
		MothershipBanData result = new MothershipBanData(MothershipApiPINVOKE.MothershipBanDataVector_getitemcopy(swigCPtr, index), cMemoryOwn: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private MothershipBanData getitem(int index)
	{
		MothershipBanData result = new MothershipBanData(MothershipApiPINVOKE.MothershipBanDataVector_getitem(swigCPtr, index), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(int index, MothershipBanData val)
	{
		MothershipApiPINVOKE.MothershipBanDataVector_setitem(swigCPtr, index, MothershipBanData.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void AddRange(MothershipBanDataVector values)
	{
		MothershipApiPINVOKE.MothershipBanDataVector_AddRange(swigCPtr, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insert(int index, MothershipBanData x)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__insert(swigCPtr, index, MothershipBanData.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insertRange(int index, MothershipBanDataVector values)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__insertRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeAt(int index)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__removeAt(swigCPtr, index);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeRange(int index, int count)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__removeRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Reverse()
	{
		MothershipApiPINVOKE.MothershipBanDataVector_Reverse(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _reverseRange(int index, int count)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__reverseRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _setRange(int index, MothershipBanDataVector values)
	{
		MothershipApiPINVOKE.MothershipBanDataVector__setRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
