using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class StorefrontResultsMap : IDisposable, IDictionary<string, MothershipBoundOfferDisplay>, ICollection<KeyValuePair<string, MothershipBoundOfferDisplay>>, IEnumerable<KeyValuePair<string, MothershipBoundOfferDisplay>>, IEnumerable
{
	public sealed class StorefrontResultsMapEnumerator : IEnumerator, IEnumerator<KeyValuePair<string, MothershipBoundOfferDisplay>>, IDisposable
	{
		private StorefrontResultsMap collectionRef;

		private IList<string> keyCollection;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public KeyValuePair<string, MothershipBoundOfferDisplay> Current
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
				return (KeyValuePair<string, MothershipBoundOfferDisplay>)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public StorefrontResultsMapEnumerator(StorefrontResultsMap collection)
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
					currentObject = new KeyValuePair<string, MothershipBoundOfferDisplay>(key, collectionRef[key]);
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

	public MothershipBoundOfferDisplay this[string key]
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

	public ICollection<MothershipBoundOfferDisplay> Values
	{
		get
		{
			ICollection<MothershipBoundOfferDisplay> collection = new List<MothershipBoundOfferDisplay>();
			using StorefrontResultsMapEnumerator storefrontResultsMapEnumerator = GetEnumerator();
			while (storefrontResultsMapEnumerator.MoveNext())
			{
				collection.Add(storefrontResultsMapEnumerator.Current.Value);
			}
			return collection;
		}
	}

	internal StorefrontResultsMap(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(StorefrontResultsMap obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(StorefrontResultsMap obj)
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

	~StorefrontResultsMap()
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
					MothershipApiPINVOKE.delete_StorefrontResultsMap(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool TryGetValue(string key, out MothershipBoundOfferDisplay value)
	{
		if (ContainsKey(key))
		{
			value = this[key];
			return true;
		}
		value = null;
		return false;
	}

	public void Add(KeyValuePair<string, MothershipBoundOfferDisplay> item)
	{
		if (ContainsKey(item.Key))
		{
			throw new ArgumentOutOfRangeException("Key", "An item with the same key '" + item.Key + "' has already been added to the dictionary.");
		}
		Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<string, MothershipBoundOfferDisplay> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public bool Contains(KeyValuePair<string, MothershipBoundOfferDisplay> item)
	{
		if (this[item.Key] == item.Value)
		{
			return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<string, MothershipBoundOfferDisplay>[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(KeyValuePair<string, MothershipBoundOfferDisplay>[] array, int arrayIndex)
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
			array.SetValue(new KeyValuePair<string, MothershipBoundOfferDisplay>(key, this[key]), arrayIndex + i);
		}
	}

	IEnumerator<KeyValuePair<string, MothershipBoundOfferDisplay>> IEnumerable<KeyValuePair<string, MothershipBoundOfferDisplay>>.GetEnumerator()
	{
		return new StorefrontResultsMapEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new StorefrontResultsMapEnumerator(this);
	}

	public StorefrontResultsMapEnumerator GetEnumerator()
	{
		return new StorefrontResultsMapEnumerator(this);
	}

	public StorefrontResultsMap()
		: this(MothershipApiPINVOKE.new_StorefrontResultsMap__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public StorefrontResultsMap(StorefrontResultsMap other)
		: this(MothershipApiPINVOKE.new_StorefrontResultsMap__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.StorefrontResultsMap_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool empty()
	{
		bool result = MothershipApiPINVOKE.StorefrontResultsMap_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		MothershipApiPINVOKE.StorefrontResultsMap_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MothershipBoundOfferDisplay getitem(string key)
	{
		MothershipBoundOfferDisplay result = new MothershipBoundOfferDisplay(MothershipApiPINVOKE.StorefrontResultsMap_getitem(swigCPtr, key), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(string key, MothershipBoundOfferDisplay x)
	{
		MothershipApiPINVOKE.StorefrontResultsMap_setitem(swigCPtr, key, MothershipBoundOfferDisplay.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ContainsKey(string key)
	{
		bool result = MothershipApiPINVOKE.StorefrontResultsMap_ContainsKey(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Add(string key, MothershipBoundOfferDisplay val)
	{
		MothershipApiPINVOKE.StorefrontResultsMap_Add(swigCPtr, key, MothershipBoundOfferDisplay.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool Remove(string key)
	{
		bool result = MothershipApiPINVOKE.StorefrontResultsMap_Remove(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private IntPtr create_iterator_begin()
	{
		IntPtr result = MothershipApiPINVOKE.StorefrontResultsMap_create_iterator_begin(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private string get_next_key(IntPtr swigiterator)
	{
		string result = MothershipApiPINVOKE.StorefrontResultsMap_get_next_key(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void destroy_iterator(IntPtr swigiterator)
	{
		MothershipApiPINVOKE.StorefrontResultsMap_destroy_iterator(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
