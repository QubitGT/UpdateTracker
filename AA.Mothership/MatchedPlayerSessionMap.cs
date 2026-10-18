using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MatchedPlayerSessionMap : IDisposable, IDictionary<string, MatchedPlayerSession>, ICollection<KeyValuePair<string, MatchedPlayerSession>>, IEnumerable<KeyValuePair<string, MatchedPlayerSession>>, IEnumerable
{
	public sealed class MatchedPlayerSessionMapEnumerator : IEnumerator, IEnumerator<KeyValuePair<string, MatchedPlayerSession>>, IDisposable
	{
		private MatchedPlayerSessionMap collectionRef;

		private IList<string> keyCollection;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public KeyValuePair<string, MatchedPlayerSession> Current
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
				return (KeyValuePair<string, MatchedPlayerSession>)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public MatchedPlayerSessionMapEnumerator(MatchedPlayerSessionMap collection)
		{
			collectionRef = collection;
			keyCollection = new List<string>(collection.Keys);
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
					string key = keyCollection[currentIndex];
					currentObject = new KeyValuePair<string, MatchedPlayerSession>(key, collectionRef[key]);
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

	public MatchedPlayerSession this[string key]
	{
		get
		{
			if (!ContainsKey(key))
			{
				throw new ArgumentOutOfRangeException("key", "The given key '" + key + "' was not present in the dictionary.");
			}
			return getitem(key);
		}
		set
		{
			setitem(key, value);
		}
	}

	public int Count => (int)size();

	public bool IsReadOnly => false;

	public ICollection<string> Keys
	{
		get
		{
			ICollection<string> collection = new List<string>();
			int count = Count;
			if (count > 0)
			{
				IntPtr swigiterator = create_iterator_begin();
				for (int i = 0; i < count; i++)
				{
					collection.Add(get_next_key(swigiterator));
				}
				destroy_iterator(swigiterator);
			}
			return collection;
		}
	}

	public ICollection<MatchedPlayerSession> Values
	{
		get
		{
			ICollection<MatchedPlayerSession> collection = new List<MatchedPlayerSession>();
			using MatchedPlayerSessionMapEnumerator matchedPlayerSessionMapEnumerator = GetEnumerator();
			while (matchedPlayerSessionMapEnumerator.MoveNext())
			{
				collection.Add(matchedPlayerSessionMapEnumerator.Current.Value);
			}
			return collection;
		}
	}

	internal MatchedPlayerSessionMap(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MatchedPlayerSessionMap obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MatchedPlayerSessionMap obj)
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

	~MatchedPlayerSessionMap()
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
					MothershipApiPINVOKE.delete_MatchedPlayerSessionMap(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool TryGetValue(string key, out MatchedPlayerSession value)
	{
		if (ContainsKey(key))
		{
			value = this[key];
			return true;
		}
		value = null;
		return false;
	}

	public void Add(KeyValuePair<string, MatchedPlayerSession> item)
	{
		if (ContainsKey(item.Key))
		{
			throw new ArgumentOutOfRangeException("Key", "An item with the same key '" + item.Key + "' has already been added to the dictionary.");
		}
		Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<string, MatchedPlayerSession> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public bool Contains(KeyValuePair<string, MatchedPlayerSession> item)
	{
		if (this[item.Key] == item.Value)
		{
			return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<string, MatchedPlayerSession>[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(KeyValuePair<string, MatchedPlayerSession>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", "Value is less than zero");
		}
		if (array.Rank > 1)
		{
			throw new ArgumentException("Multi dimensional array.", "array");
		}
		if (arrayIndex + Count > array.Length)
		{
			throw new ArgumentException("Number of elements to copy is too large.");
		}
		IList<string> list = new List<string>(Keys);
		for (int i = 0; i < list.Count; i++)
		{
			string key = list[i];
			array.SetValue(new KeyValuePair<string, MatchedPlayerSession>(key, this[key]), arrayIndex + i);
		}
	}

	IEnumerator<KeyValuePair<string, MatchedPlayerSession>> IEnumerable<KeyValuePair<string, MatchedPlayerSession>>.GetEnumerator()
	{
		return new MatchedPlayerSessionMapEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new MatchedPlayerSessionMapEnumerator(this);
	}

	public MatchedPlayerSessionMapEnumerator GetEnumerator()
	{
		return new MatchedPlayerSessionMapEnumerator(this);
	}

	public MatchedPlayerSessionMap()
		: this(MothershipApiPINVOKE.new_MatchedPlayerSessionMap__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MatchedPlayerSessionMap(MatchedPlayerSessionMap other)
		: this(MothershipApiPINVOKE.new_MatchedPlayerSessionMap__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.MatchedPlayerSessionMap_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool empty()
	{
		bool result = MothershipApiPINVOKE.MatchedPlayerSessionMap_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		MothershipApiPINVOKE.MatchedPlayerSessionMap_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MatchedPlayerSession getitem(string key)
	{
		MatchedPlayerSession result = new MatchedPlayerSession(MothershipApiPINVOKE.MatchedPlayerSessionMap_getitem(swigCPtr, key), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(string key, MatchedPlayerSession x)
	{
		MothershipApiPINVOKE.MatchedPlayerSessionMap_setitem(swigCPtr, key, MatchedPlayerSession.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ContainsKey(string key)
	{
		bool result = MothershipApiPINVOKE.MatchedPlayerSessionMap_ContainsKey(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Add(string key, MatchedPlayerSession val)
	{
		MothershipApiPINVOKE.MatchedPlayerSessionMap_Add(swigCPtr, key, MatchedPlayerSession.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool Remove(string key)
	{
		bool result = MothershipApiPINVOKE.MatchedPlayerSessionMap_Remove(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private IntPtr create_iterator_begin()
	{
		IntPtr result = MothershipApiPINVOKE.MatchedPlayerSessionMap_create_iterator_begin(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private string get_next_key(IntPtr swigiterator)
	{
		string result = MothershipApiPINVOKE.MatchedPlayerSessionMap_get_next_key(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void destroy_iterator(IntPtr swigiterator)
	{
		MothershipApiPINVOKE.MatchedPlayerSessionMap_destroy_iterator(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
