#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon.StructWrapping;

namespace ExitGames.Client.Photon;

public class SupportClass
{
	[Obsolete("Use a Stopwatch (or equivalent) instead.")]
	public delegate int IntegerMillisecondsDelegate();

	public class ThreadSafeRandom
	{
		private static readonly Random _r = new Random();

		public static int Next()
		{
			lock (_r)
			{
				return _r.Next();
			}
		}
	}

	private static List<Thread> threadList;

	private static readonly object ThreadListLock = new object();

	[Obsolete("Use a Stopwatch (or equivalent) instead.")]
	protected internal static IntegerMillisecondsDelegate IntegerMilliseconds = () => Environment.TickCount;

	private static uint[] crcLookupTable;

	public static List<MethodInfo> GetMethods(Type type, Type attribute)
	{
		List<MethodInfo> list = new List<MethodInfo>();
		if (type == null)
		{
			return list;
		}
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (attribute == null || methodInfo.IsDefined(attribute, inherit: false))
			{
				list.Add(methodInfo);
			}
		}
		return list;
	}

	[Obsolete("Use a Stopwatch (or equivalent) instead.")]
	public static int GetTickCount()
	{
		return IntegerMilliseconds();
	}

	public static byte StartBackgroundCalls(Func<bool> myThread, int millisecondsInterval = 100, string taskName = "")
	{
		lock (ThreadListLock)
		{
			if (threadList == null)
			{
				threadList = new List<Thread>();
			}
			Thread thread = new Thread((ThreadStart)delegate
			{
				try
				{
					while (myThread())
					{
						Thread.Sleep(millisecondsInterval);
					}
				}
				catch (ThreadAbortException)
				{
				}
			});
			if (!string.IsNullOrEmpty(taskName))
			{
				thread.Name = taskName;
			}
			thread.IsBackground = true;
			thread.Start();
			for (int i = 0; i < threadList.Count; i++)
			{
				if (threadList[i] == null)
				{
					threadList[i] = thread;
					return (byte)i;
				}
			}
			if (threadList.Count >= 255)
			{
				throw new NotSupportedException("StartBackgroundCalls() can run a maximum of 255 threads.");
			}
			threadList.Add(thread);
			return (byte)(threadList.Count - 1);
		}
	}

	public static bool StopBackgroundCalls(byte id)
	{
		lock (ThreadListLock)
		{
			if (threadList == null || id >= threadList.Count || threadList[id] == null)
			{
				return false;
			}
			threadList[id].Abort();
			threadList[id] = null;
			return true;
		}
	}

	public static bool StopAllBackgroundCalls()
	{
		lock (ThreadListLock)
		{
			if (threadList == null)
			{
				return false;
			}
			foreach (Thread thread in threadList)
			{
				thread?.Abort();
			}
			threadList.Clear();
		}
		return true;
	}

	public static void WriteStackTrace(Exception throwable, TextWriter stream)
	{
		if (stream != null)
		{
			stream.WriteLine(throwable.ToString());
			stream.WriteLine(throwable.StackTrace);
			stream.Flush();
		}
		else
		{
			Debug.WriteLine(throwable.ToString());
			Debug.WriteLine(throwable.StackTrace);
		}
	}

	public static void WriteStackTrace(Exception throwable)
	{
		WriteStackTrace(throwable, null);
	}

	public static string DictionaryToString(IDictionary dictionary, bool includeTypes = true)
	{
		if (dictionary == null)
		{
			return "null";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		foreach (object key in dictionary.Keys)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(", ");
			}
			Type type;
			string text;
			if (dictionary[key] == null)
			{
				type = typeof(object);
				text = "null";
			}
			else
			{
				type = dictionary[key].GetType();
				text = dictionary[key].ToString();
			}
			if (type == typeof(IDictionary) || type == typeof(Hashtable))
			{
				text = DictionaryToString((IDictionary)dictionary[key]);
			}
			else if (type == typeof(NonAllocDictionary<byte, object>))
			{
				text = DictionaryToString((NonAllocDictionary<byte, object>)dictionary[key]);
			}
			else if (type == typeof(string[]))
			{
				text = string.Format("{{{0}}}", string.Join(",", (string[])dictionary[key]));
			}
			else if (type == typeof(byte[]))
			{
				text = $"byte[{((byte[])dictionary[key]).Length}]";
			}
			else if (dictionary[key] is StructWrapper structWrapper)
			{
				stringBuilder.AppendFormat("{0}={1}", key, structWrapper.ToString(includeTypes));
				continue;
			}
			if (includeTypes)
			{
				stringBuilder.AppendFormat("({0}){1}=({2}){3}", key.GetType().Name, key, type.Name, text);
			}
			else
			{
				stringBuilder.AppendFormat("{0}={1}", key, text);
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	public static string DictionaryToString(NonAllocDictionary<byte, object> dictionary, bool includeTypes = true)
	{
		if (dictionary == null)
		{
			return "null";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{");
		foreach (byte key in dictionary.Keys)
		{
			if (stringBuilder.Length > 1)
			{
				stringBuilder.Append(", ");
			}
			Type type;
			string text;
			if (dictionary[key] == null)
			{
				type = typeof(object);
				text = "null";
			}
			else
			{
				type = dictionary[key].GetType();
				text = dictionary[key].ToString();
			}
			if (type == typeof(IDictionary) || type == typeof(Hashtable))
			{
				text = DictionaryToString((IDictionary)dictionary[key]);
			}
			else if (type == typeof(NonAllocDictionary<byte, object>))
			{
				text = DictionaryToString((NonAllocDictionary<byte, object>)dictionary[key]);
			}
			else if (type == typeof(string[]))
			{
				text = string.Format("{{{0}}}", string.Join(",", (string[])dictionary[key]));
			}
			else if (type == typeof(byte[]))
			{
				text = $"byte[{((byte[])dictionary[key]).Length}]";
			}
			else if (dictionary[key] is StructWrapper structWrapper)
			{
				stringBuilder.AppendFormat("{0}={1}", key, structWrapper.ToString(includeTypes));
				continue;
			}
			if (includeTypes)
			{
				stringBuilder.AppendFormat("({0}){1}=({2}){3}", key.GetType().Name, key, type.Name, text);
			}
			else
			{
				stringBuilder.AppendFormat("{0}={1}", key, text);
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	[Obsolete("Use DictionaryToString() instead.")]
	public static string HashtableToString(Hashtable hash)
	{
		return DictionaryToString(hash);
	}

	public static string ByteArrayToString(byte[] list, int length = -1)
	{
		if (list == null)
		{
			return string.Empty;
		}
		if (length < 0 || length > list.Length)
		{
			length = list.Length;
		}
		return BitConverter.ToString(list, 0, length);
	}

	private static uint[] InitializeTable(uint polynomial)
	{
		uint[] array = new uint[256];
		for (int i = 0; i < 256; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) != 1) ? (num >> 1) : ((num >> 1) ^ polynomial));
			}
			array[i] = num;
		}
		return array;
	}

	public static uint CalculateCrc(byte[] buffer, int length)
	{
		uint num = uint.MaxValue;
		uint polynomial = 3988292384u;
		if (crcLookupTable == null)
		{
			crcLookupTable = InitializeTable(polynomial);
		}
		for (int i = 0; i < length; i++)
		{
			num = (num >> 8) ^ crcLookupTable[buffer[i] ^ (num & 0xFF)];
		}
		return num;
	}
}
