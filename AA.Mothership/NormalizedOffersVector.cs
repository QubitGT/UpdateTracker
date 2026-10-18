using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class NormalizedOffersVector : IDisposable, IEnumerable, IEnumerable<MothershipNormalizedOffer>
{
	public sealed class NormalizedOffersVectorEnumerator : IEnumerator, IEnumerator<MothershipNormalizedOffer>, IDisposable
	{
		private NormalizedOffersVector collectionRef;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public MothershipNormalizedOffer Current
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
				return (MothershipNormalizedOffer)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public NormalizedOffersVectorEnumerator(NormalizedOffersVector collection)
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

	public MothershipNormalizedOffer this[int index]
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

	internal NormalizedOffersVector(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(NormalizedOffersVector obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(NormalizedOffersVector obj)
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

	~NormalizedOffersVector()
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
					MothershipApiPINVOKE.delete_NormalizedOffersVector(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public NormalizedOffersVector(IEnumerable c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipNormalizedOffer item in c)
		{
			Add(item);
		}
	}

	public NormalizedOffersVector(IEnumerable<MothershipNormalizedOffer> c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipNormalizedOffer item in c)
		{
			Add(item);
		}
	}

	public void Insert(int index, MothershipNormalizedOffer value)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to insert at an out of bounds index in this vector");
		}
		_insert(index, value);
	}

	public void InsertRange(int index, NormalizedOffersVector values)
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

	public void SetRange(int index, NormalizedOffersVector values)
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

	public void CopyTo(MothershipNormalizedOffer[] array)
	{
		CopyTo(0, array, 0, Count);
	}

	public void CopyTo(MothershipNormalizedOffer[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, Count);
	}

	public void CopyTo(int index, MothershipNormalizedOffer[] array, int arrayIndex, int count)
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

	public MothershipNormalizedOffer[] ToArray()
	{
		MothershipNormalizedOffer[] array = new MothershipNormalizedOffer[Count];
		CopyTo(array);
		return array;
	}

	IEnumerator<MothershipNormalizedOffer> IEnumerable<MothershipNormalizedOffer>.GetEnumerator()
	{
		return new NormalizedOffersVectorEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new NormalizedOffersVectorEnumerator(this);
	}

	public NormalizedOffersVectorEnumerator GetEnumerator()
	{
		return new NormalizedOffersVectorEnumerator(this);
	}

	public NormalizedOffersVector()
		: this(MothershipApiPINVOKE.new_NormalizedOffersVector__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public NormalizedOffersVector(NormalizedOffersVector other)
		: this(MothershipApiPINVOKE.new_NormalizedOffersVector__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Clear()
	{
		MothershipApiPINVOKE.NormalizedOffersVector_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Add(MothershipNormalizedOffer x)
	{
		MothershipApiPINVOKE.NormalizedOffersVector_Add(swigCPtr, MothershipNormalizedOffer.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.NormalizedOffersVector_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool empty()
	{
		bool result = MothershipApiPINVOKE.NormalizedOffersVector_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint capacity()
	{
		uint result = MothershipApiPINVOKE.NormalizedOffersVector_capacity(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void reserve(uint n)
	{
		MothershipApiPINVOKE.NormalizedOffersVector_reserve(swigCPtr, n);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public NormalizedOffersVector(int capacity)
		: this(MothershipApiPINVOKE.new_NormalizedOffersVector__SWIG_2(capacity), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MothershipNormalizedOffer getitemcopy(int index)
	{
		MothershipNormalizedOffer result = new MothershipNormalizedOffer(MothershipApiPINVOKE.NormalizedOffersVector_getitemcopy(swigCPtr, index), cMemoryOwn: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private MothershipNormalizedOffer getitem(int index)
	{
		MothershipNormalizedOffer result = new MothershipNormalizedOffer(MothershipApiPINVOKE.NormalizedOffersVector_getitem(swigCPtr, index), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(int index, MothershipNormalizedOffer val)
	{
		MothershipApiPINVOKE.NormalizedOffersVector_setitem(swigCPtr, index, MothershipNormalizedOffer.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void AddRange(NormalizedOffersVector values)
	{
		MothershipApiPINVOKE.NormalizedOffersVector_AddRange(swigCPtr, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insert(int index, MothershipNormalizedOffer x)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__insert(swigCPtr, index, MothershipNormalizedOffer.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insertRange(int index, NormalizedOffersVector values)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__insertRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeAt(int index)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__removeAt(swigCPtr, index);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeRange(int index, int count)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__removeRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Reverse()
	{
		MothershipApiPINVOKE.NormalizedOffersVector_Reverse(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _reverseRange(int index, int count)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__reverseRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _setRange(int index, NormalizedOffersVector values)
	{
		MothershipApiPINVOKE.NormalizedOffersVector__setRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
