using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class PurchaseChangesMap : IDisposable, IDictionary<string, MothershipEntitlementUpdate>, ICollection<KeyValuePair<string, MothershipEntitlementUpdate>>, IEnumerable<KeyValuePair<string, MothershipEntitlementUpdate>>, IEnumerable
{
	public sealed class PurchaseChangesMapEnumerator : IEnumerator, IEnumerator<KeyValuePair<string, MothershipEntitlementUpdate>>, IDisposable
	{
		private PurchaseChangesMap collectionRef;

		private IList<string> keyCollection;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public KeyValuePair<string, MothershipEntitlementUpdate> Current
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
				return (KeyValuePair<string, MothershipEntitlementUpdate>)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public PurchaseChangesMapEnumerator(PurchaseChangesMap collection)
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
					currentObject = new KeyValuePair<string, MothershipEntitlementUpdate>(key, collectionRef[key]);
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

	public MothershipEntitlementUpdate this[string key]
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

	public ICollection<MothershipEntitlementUpdate> Values
	{
		get
		{
			ICollection<MothershipEntitlementUpdate> collection = new List<MothershipEntitlementUpdate>();
			using PurchaseChangesMapEnumerator purchaseChangesMapEnumerator = GetEnumerator();
			while (purchaseChangesMapEnumerator.MoveNext())
			{
				collection.Add(purchaseChangesMapEnumerator.Current.Value);
			}
			return collection;
		}
	}

	internal PurchaseChangesMap(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(PurchaseChangesMap obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(PurchaseChangesMap obj)
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

	~PurchaseChangesMap()
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
					MothershipApiPINVOKE.delete_PurchaseChangesMap(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool TryGetValue(string key, out MothershipEntitlementUpdate value)
	{
		if (ContainsKey(key))
		{
			value = this[key];
			return true;
		}
		value = null;
		return false;
	}

	public void Add(KeyValuePair<string, MothershipEntitlementUpdate> item)
	{
		if (ContainsKey(item.Key))
		{
			throw new ArgumentOutOfRangeException("Key", "An item with the same key '" + item.Key + "' has already been added to the dictionary.");
		}
		Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<string, MothershipEntitlementUpdate> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public bool Contains(KeyValuePair<string, MothershipEntitlementUpdate> item)
	{
		if (this[item.Key] == item.Value)
		{
			return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<string, MothershipEntitlementUpdate>[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(KeyValuePair<string, MothershipEntitlementUpdate>[] array, int arrayIndex)
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
			array.SetValue(new KeyValuePair<string, MothershipEntitlementUpdate>(key, this[key]), arrayIndex + i);
		}
	}

	IEnumerator<KeyValuePair<string, MothershipEntitlementUpdate>> IEnumerable<KeyValuePair<string, MothershipEntitlementUpdate>>.GetEnumerator()
	{
		return new PurchaseChangesMapEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new PurchaseChangesMapEnumerator(this);
	}

	public PurchaseChangesMapEnumerator GetEnumerator()
	{
		return new PurchaseChangesMapEnumerator(this);
	}

	public PurchaseChangesMap()
		: this(MothershipApiPINVOKE.new_PurchaseChangesMap__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public PurchaseChangesMap(PurchaseChangesMap other)
		: this(MothershipApiPINVOKE.new_PurchaseChangesMap__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.PurchaseChangesMap_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool empty()
	{
		bool result = MothershipApiPINVOKE.PurchaseChangesMap_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		MothershipApiPINVOKE.PurchaseChangesMap_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MothershipEntitlementUpdate getitem(string key)
	{
		MothershipEntitlementUpdate result = new MothershipEntitlementUpdate(MothershipApiPINVOKE.PurchaseChangesMap_getitem(swigCPtr, key), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(string key, MothershipEntitlementUpdate x)
	{
		MothershipApiPINVOKE.PurchaseChangesMap_setitem(swigCPtr, key, MothershipEntitlementUpdate.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ContainsKey(string key)
	{
		bool result = MothershipApiPINVOKE.PurchaseChangesMap_ContainsKey(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Add(string key, MothershipEntitlementUpdate val)
	{
		MothershipApiPINVOKE.PurchaseChangesMap_Add(swigCPtr, key, MothershipEntitlementUpdate.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool Remove(string key)
	{
		bool result = MothershipApiPINVOKE.PurchaseChangesMap_Remove(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private IntPtr create_iterator_begin()
	{
		IntPtr result = MothershipApiPINVOKE.PurchaseChangesMap_create_iterator_begin(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private string get_next_key(IntPtr swigiterator)
	{
		string result = MothershipApiPINVOKE.PurchaseChangesMap_get_next_key(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void destroy_iterator(IntPtr swigiterator)
	{
		MothershipApiPINVOKE.PurchaseChangesMap_destroy_iterator(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
