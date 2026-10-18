using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MatchmakingTicketPlayerMap : IDisposable, IDictionary<string, MatchmakingTicketPlayer>, ICollection<KeyValuePair<string, MatchmakingTicketPlayer>>, IEnumerable<KeyValuePair<string, MatchmakingTicketPlayer>>, IEnumerable
{
	public sealed class MatchmakingTicketPlayerMapEnumerator : IEnumerator, IEnumerator<KeyValuePair<string, MatchmakingTicketPlayer>>, IDisposable
	{
		private MatchmakingTicketPlayerMap collectionRef;

		private IList<string> keyCollection;

		private int currentIndex;

		private object currentObject;

		private int currentSize;

		public KeyValuePair<string, MatchmakingTicketPlayer> Current
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
				return (KeyValuePair<string, MatchmakingTicketPlayer>)currentObject;
			}
		}

		object IEnumerator.Current => Current;

		public MatchmakingTicketPlayerMapEnumerator(MatchmakingTicketPlayerMap collection)
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
					currentObject = new KeyValuePair<string, MatchmakingTicketPlayer>(key, collectionRef[key]);
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

	public MatchmakingTicketPlayer this[string key]
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

	public ICollection<MatchmakingTicketPlayer> Values
	{
		get
		{
			ICollection<MatchmakingTicketPlayer> collection = new List<MatchmakingTicketPlayer>();
			using MatchmakingTicketPlayerMapEnumerator matchmakingTicketPlayerMapEnumerator = GetEnumerator();
			while (matchmakingTicketPlayerMapEnumerator.MoveNext())
			{
				collection.Add(matchmakingTicketPlayerMapEnumerator.Current.Value);
			}
			return collection;
		}
	}

	internal MatchmakingTicketPlayerMap(IntPtr cPtr, bool cMemoryOwn)
	{
		swigCMemOwn = cMemoryOwn;
		swigCPtr = new HandleRef(this, cPtr);
	}

	internal static HandleRef getCPtr(MatchmakingTicketPlayerMap obj)
	{
		return obj?.swigCPtr ?? new HandleRef(null, IntPtr.Zero);
	}

	internal static HandleRef swigRelease(MatchmakingTicketPlayerMap obj)
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

	~MatchmakingTicketPlayerMap()
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
					MothershipApiPINVOKE.delete_MatchmakingTicketPlayerMap(swigCPtr);
				}
				swigCPtr = new HandleRef(null, IntPtr.Zero);
			}
		}
	}

	public bool TryGetValue(string key, out MatchmakingTicketPlayer value)
	{
		if (ContainsKey(key))
		{
			value = this[key];
			return true;
		}
		value = null;
		return false;
	}

	public void Add(KeyValuePair<string, MatchmakingTicketPlayer> item)
	{
		if (ContainsKey(item.Key))
		{
			throw new ArgumentOutOfRangeException("Key", "An item with the same key '" + item.Key + "' has already been added to the dictionary.");
		}
		Add(item.Key, item.Value);
	}

	public bool Remove(KeyValuePair<string, MatchmakingTicketPlayer> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public bool Contains(KeyValuePair<string, MatchmakingTicketPlayer> item)
	{
		if (this[item.Key] == item.Value)
		{
			return true;
		}
		return false;
	}

	public void CopyTo(KeyValuePair<string, MatchmakingTicketPlayer>[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(KeyValuePair<string, MatchmakingTicketPlayer>[] array, int arrayIndex)
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
			array.SetValue(new KeyValuePair<string, MatchmakingTicketPlayer>(key, this[key]), arrayIndex + i);
		}
	}

	IEnumerator<KeyValuePair<string, MatchmakingTicketPlayer>> IEnumerable<KeyValuePair<string, MatchmakingTicketPlayer>>.GetEnumerator()
	{
		return new MatchmakingTicketPlayerMapEnumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new MatchmakingTicketPlayerMapEnumerator(this);
	}

	public MatchmakingTicketPlayerMapEnumerator GetEnumerator()
	{
		return new MatchmakingTicketPlayerMapEnumerator(this);
	}

	public MatchmakingTicketPlayerMap()
		: this(MothershipApiPINVOKE.new_MatchmakingTicketPlayerMap__SWIG_0(), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public MatchmakingTicketPlayerMap(MatchmakingTicketPlayerMap other)
		: this(MothershipApiPINVOKE.new_MatchmakingTicketPlayerMap__SWIG_1(getCPtr(other)), cMemoryOwn: true)
	{
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private uint size()
	{
		uint result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_size(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public bool empty()
	{
		bool result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_empty(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Clear()
	{
		MothershipApiPINVOKE.MatchmakingTicketPlayerMap_Clear(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	private MatchmakingTicketPlayer getitem(string key)
	{
		MatchmakingTicketPlayer result = new MatchmakingTicketPlayer(MothershipApiPINVOKE.MatchmakingTicketPlayerMap_getitem(swigCPtr, key), cMemoryOwn: false);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void setitem(string key, MatchmakingTicketPlayer x)
	{
		MothershipApiPINVOKE.MatchmakingTicketPlayerMap_setitem(swigCPtr, key, MatchmakingTicketPlayer.getCPtr(x));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool ContainsKey(string key)
	{
		bool result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_ContainsKey(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	public void Add(string key, MatchmakingTicketPlayer val)
	{
		MothershipApiPINVOKE.MatchmakingTicketPlayerMap_Add(swigCPtr, key, MatchmakingTicketPlayer.getCPtr(val));
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}

	public bool Remove(string key)
	{
		bool result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_Remove(swigCPtr, key);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private IntPtr create_iterator_begin()
	{
		IntPtr result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_create_iterator_begin(swigCPtr);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private string get_next_key(IntPtr swigiterator)
	{
		string result = MothershipApiPINVOKE.MatchmakingTicketPlayerMap_get_next_key(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
		return result;
	}

	private void destroy_iterator(IntPtr swigiterator)
	{
		MothershipApiPINVOKE.MatchmakingTicketPlayerMap_destroy_iterator(swigCPtr, swigiterator);
		if (MothershipApiPINVOKE.SWIGPendingException.Pending)
		{
			throw MothershipApiPINVOKE.SWIGPendingException.Retrieve();
		}
	}
}
