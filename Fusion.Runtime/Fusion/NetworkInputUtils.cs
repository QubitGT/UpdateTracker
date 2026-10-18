#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fusion;

public static class NetworkInputUtils
{
	private static bool _initialized;

	private static Dictionary<Type, int> _wordCount;

	private static Dictionary<Type, int> _typeKey;

	private static void LoadTypes()
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_wordCount = new Dictionary<Type, int>();
		_typeKey = new Dictionary<Type, int>();
		List<(Type, NetworkInputWeavedAttribute)> list = new List<(Type, NetworkInputWeavedAttribute)>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetCustomAttribute<NetworkAssemblyIgnoreAttribute>() != null)
			{
				continue;
			}
			try
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (!type.IsValueType || !typeof(INetworkInput).IsAssignableFrom(type))
					{
						continue;
					}
					try
					{
						object[] customAttributes = type.GetCustomAttributes(typeof(NetworkInputWeavedAttribute), inherit: false);
						if (customAttributes.Length == 1 && customAttributes[0] is NetworkInputWeavedAttribute item)
						{
							list.Add((type, item));
						}
					}
					catch (Exception error)
					{
						InternalLogStreams.LogException?.Log(error);
					}
				}
			}
			catch
			{
			}
		}
		list.Sort(delegate((Type, NetworkInputWeavedAttribute) a, (Type, NetworkInputWeavedAttribute) b)
		{
			int num = StringComparer.Ordinal.Compare(a.Item1.AssemblyQualifiedName, b.Item1.AssemblyQualifiedName);
			if (num == 0)
			{
				Assert.AlwaysFail("order == 0");
			}
			return num;
		});
		for (int k = 0; k < list.Count; k++)
		{
			var (key, networkInputWeavedAttribute) = list[k];
			_typeKey.Add(key, k + 1);
			_wordCount.Add(key, networkInputWeavedAttribute.WordCount);
		}
	}

	public static int GetMaxWordCount()
	{
		LoadTypes();
		if (_wordCount.Count == 0)
		{
			return 0;
		}
		int num = _wordCount.Values.Max();
		foreach (KeyValuePair<Type, int> item in _wordCount)
		{
			Assert.Check(Native.SizeOf(item.Key) <= num * 4, item.Key);
		}
		return num + 1;
	}

	public static int GetWordCount(Type type)
	{
		Assert.Check(typeof(INetworkInput).IsAssignableFrom(type));
		LoadTypes();
		if (_wordCount.TryGetValue(type, out var value))
		{
			return value;
		}
		Assert.AlwaysFail($"GetWordCount for {type}");
		return -1;
	}

	public static int GetTypeKey(Type type)
	{
		Assert.Check(typeof(INetworkInput).IsAssignableFrom(type));
		LoadTypes();
		if (_typeKey.TryGetValue(type, out var value))
		{
			return value;
		}
		Assert.AlwaysFail($"GetTypeKey for {type}");
		return -1;
	}

	public static Type GetType(int typeKey)
	{
		LoadTypes();
		foreach (KeyValuePair<Type, int> item in _typeKey)
		{
			if (item.Value == typeKey)
			{
				return item.Key;
			}
		}
		return null;
	}

	internal static void ResetStatics()
	{
		_initialized = false;
		_wordCount = null;
		_typeKey = null;
	}
}
