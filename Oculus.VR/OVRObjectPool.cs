using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal static class OVRObjectPool
{
	public interface IPoolObject
	{
		void OnGet();

		void OnReturn();
	}

	private static class Storage<T> where T : class, new()
	{
		private static readonly HashSet<T> s_hashSet = new HashSet<T>();

		public static readonly Action Clear = delegate
		{
			s_hashSet.Clear();
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Remove(T item)
		{
			return s_hashSet.Remove(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Add(T item)
		{
			return s_hashSet.Add(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetOrCreate()
		{
			using HashSet<T>.Enumerator enumerator = s_hashSet.GetEnumerator();
			if (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				Remove(current);
				return current;
			}
			return new T();
		}
	}

	public struct ListScope<T> : IDisposable
	{
		private List<T> _list;

		public ListScope(out List<T> list)
		{
			_list = (list = List<T>());
		}

		public ListScope(IEnumerable<T> source, out List<T> list)
		{
			_list = (list = List(source));
		}

		public void Dispose()
		{
			Return(_list);
		}
	}

	public struct TaskScope<T> : IDisposable
	{
		private ListScope<OVRTask<T>> _tasks;

		private ListScope<T> _results;

		public TaskScope(out List<OVRTask<T>> tasks, out List<T> results)
		{
			_tasks = new ListScope<OVRTask<T>>(out tasks);
			_results = new ListScope<T>(out results);
		}

		public void Dispose()
		{
			_tasks.Dispose();
			_results.Dispose();
		}
	}

	public readonly struct DictionaryScope<TKey, TValue> : IDisposable
	{
		private readonly Dictionary<TKey, TValue> _dictionary;

		public DictionaryScope(out Dictionary<TKey, TValue> dictionary)
		{
			_dictionary = (dictionary = Dictionary<TKey, TValue>());
		}

		public void Dispose()
		{
			Return(_dictionary);
		}
	}

	public readonly struct HashSetScope<T> : IDisposable
	{
		private readonly HashSet<T> _set;

		public HashSetScope(out HashSet<T> set)
		{
			_set = (set = HashSet<T>());
		}

		public void Dispose()
		{
			Return(_set);
		}
	}

	public readonly struct StackScope<T> : IDisposable
	{
		private readonly Stack<T> _stack;

		public StackScope(out Stack<T> stack)
		{
			_stack = (stack = Stack<T>());
		}

		public void Dispose()
		{
			Return(_stack);
		}
	}

	public readonly struct QueueScope<T> : IDisposable
	{
		private readonly Queue<T> _queue;

		public QueueScope(out Queue<T> queue)
		{
			_queue = (queue = Queue<T>());
		}

		public void Dispose()
		{
			Return(_queue);
		}
	}

	public readonly struct ItemScope<T> : IDisposable where T : class, new()
	{
		private readonly T _item;

		public ItemScope(out T item)
		{
			_item = (item = Get<T>());
		}

		public void Dispose()
		{
			Return(_item);
		}
	}

	public static T Get<T>() where T : class, new()
	{
		T orCreate = Storage<T>.GetOrCreate();
		if (orCreate is IList list)
		{
			list.Clear();
		}
		else if (orCreate is IDictionary dictionary)
		{
			dictionary.Clear();
		}
		(orCreate as IPoolObject)?.OnGet();
		return orCreate;
	}

	public static List<T> List<T>()
	{
		return Get<List<T>>();
	}

	public static List<T> List<T>(IEnumerable<T> source)
	{
		List<T> list = Get<List<T>>();
		foreach (T item in source.ToNonAlloc())
		{
			list.Add(item);
		}
		return list;
	}

	public static Dictionary<TKey, TValue> Dictionary<TKey, TValue>()
	{
		return Get<Dictionary<TKey, TValue>>();
	}

	public static HashSet<T> HashSet<T>()
	{
		HashSet<T> hashSet = Get<HashSet<T>>();
		hashSet.Clear();
		return hashSet;
	}

	public static Stack<T> Stack<T>()
	{
		Stack<T> stack = Get<Stack<T>>();
		stack.Clear();
		return stack;
	}

	public static Queue<T> Queue<T>()
	{
		Queue<T> queue = Get<Queue<T>>();
		queue.Clear();
		return queue;
	}

	public static void Return<T>(T obj) where T : class, new()
	{
		if (obj == null)
		{
			return;
		}
		if (!(obj is IList list))
		{
			if (!(obj is IDictionary dictionary))
			{
				if (obj is IPoolObject poolObject)
				{
					poolObject.OnReturn();
				}
			}
			else
			{
				dictionary.Clear();
			}
		}
		else
		{
			list.Clear();
		}
		Storage<T>.Add(obj);
	}

	public static void Return<T>(HashSet<T> set)
	{
		set?.Clear();
		OVRObjectPool.Return<HashSet<T>>(set);
	}

	public static void Return<T>(Stack<T> stack)
	{
		stack?.Clear();
		OVRObjectPool.Return<Stack<T>>(stack);
	}

	public static void Return<T>(Queue<T> queue)
	{
		queue?.Clear();
		OVRObjectPool.Return<Queue<T>>(queue);
	}
}
