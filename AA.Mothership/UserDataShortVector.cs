using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class UserDataShortVector : IDisposable, IEnumerable, IEnumerable<MothershipUserDataShort>
{
	public sealed class UserDataShortVectorEnumerator : IEnumerator, IEnumerator<MothershipUserDataShort>, IDisposable
	{
		private UserDataShortVector collectionRef;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public MothershipUserDataShort Current
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
				return (MothershipUserDataShort)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public UserDataShortVectorEnumerator(UserDataShortVector collection)
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

	public MothershipUserDataShort this[int index]
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

	internal UserDataShortVector(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(UserDataShortVector obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(UserDataShortVector obj)
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

	~UserDataShortVector()
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
					MothershipApiPINVOKE.delete_UserDataShortVector(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public UserDataShortVector(IEnumerable c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipUserDataShort item in c)
		{
			Add(item);
		}
	}

	public UserDataShortVector(IEnumerable<MothershipUserDataShort> c)
		: this()
	{
		if (c == null)
		{
			throw new ArgumentNullException("c");
		}
		foreach (MothershipUserDataShort item in c)
		{
			Add(item);
		}
	}

	public void Insert(int index, MothershipUserDataShort value)
	{
		if (index < 0 || index > Count)
		{
			throw new ArgumentOutOfRangeException("index", "Tried to insert at an out of bounds index in this vector");
		}
		_insert(index, value);
	}

	public void InsertRange(int index, UserDataShortVector values)
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

	public void SetRange(int index, UserDataShortVector values)
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

	public void CopyTo(MothershipUserDataShort[] array)
	{
		CopyTo(0, array, 0, Count);
	}

	public void CopyTo(MothershipUserDataShort[] array, int arrayIndex)
	{
		CopyTo(0, array, arrayIndex, Count);
	}

	public void CopyTo(int index, MothershipUserDataShort[] array, int arrayIndex, int count)
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

	public MothershipUserDataShort[] ToArray()
	{
		MothershipUserDataShort[] array = new MothershipUserDataShort[Count];
		CopyTo(array);
		return array;
	}

	IEnumerator<MothershipUserDataShort> IEnumerable<MothershipUserDataShort>.GetEnumerator()
	{
		return new UserDataShortVectorEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new UserDataShortVectorEnumerator(this);
	}

	public UserDataShortVectorEnumerator GetEnumerator()
	{
		return new UserDataShortVectorEnumerator(this);
	}

	public UserDataShortVector()
		: this(MothershipApiPINVOKE.new_UserDataShortVector__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public UserDataShortVector(UserDataShortVector other)
		: this(MothershipApiPINVOKE.new_UserDataShortVector__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Clear()
	{
		MothershipApiPINVOKE.UserDataShortVector_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Add(MothershipUserDataShort x)
	{
		MothershipApiPINVOKE.UserDataShortVector_Add(swigCPtr, MothershipUserDataShort.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.UserDataShortVector_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private bool empty()
	{
		bool result = MothershipApiPINVOKE.UserDataShortVector_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private uint capacity()
	{
		uint result = MothershipApiPINVOKE.UserDataShortVector_capacity(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void reserve(uint n)
	{
		MothershipApiPINVOKE.UserDataShortVector_reserve(swigCPtr, n);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public UserDataShortVector(int capacity)
		: this(MothershipApiPINVOKE.new_UserDataShortVector__SWIG_2(capacity), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MothershipUserDataShort getitemcopy(int index)
	{
		MothershipUserDataShort result = new MothershipUserDataShort(MothershipApiPINVOKE.UserDataShortVector_getitemcopy(swigCPtr, index), cMemoryOwn: true);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private MothershipUserDataShort getitem(int index)
	{
		MothershipUserDataShort result = new MothershipUserDataShort(MothershipApiPINVOKE.UserDataShortVector_getitem(swigCPtr, index), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(int index, MothershipUserDataShort val)
	{
		MothershipApiPINVOKE.UserDataShortVector_setitem(swigCPtr, index, MothershipUserDataShort.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void AddRange(UserDataShortVector values)
	{
		MothershipApiPINVOKE.UserDataShortVector_AddRange(swigCPtr, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insert(int index, MothershipUserDataShort x)
	{
		MothershipApiPINVOKE.UserDataShortVector__insert(swigCPtr, index, MothershipUserDataShort.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _insertRange(int index, UserDataShortVector values)
	{
		MothershipApiPINVOKE.UserDataShortVector__insertRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeAt(int index)
	{
		MothershipApiPINVOKE.UserDataShortVector__removeAt(swigCPtr, index);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _removeRange(int index, int count)
	{
		MothershipApiPINVOKE.UserDataShortVector__removeRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public void Reverse()
	{
		MothershipApiPINVOKE.UserDataShortVector_Reverse(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _reverseRange(int index, int count)
	{
		MothershipApiPINVOKE.UserDataShortVector__reverseRange(swigCPtr, index, count);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private void _setRange(int index, UserDataShortVector values)
	{
		MothershipApiPINVOKE.UserDataShortVector__setRange(swigCPtr, index, getCPtr(values));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
