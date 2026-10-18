#define DEBUG
using System;
using System.Collections.Generic;

namespace Fusion;

public static class NetworkStructUtils
{
	private static Dictionary<Type, int> _wordCounts = new Dictionary<Type, int>();

	internal static void ResetStatics()
	{
		_wordCounts.Clear();
	}

	public static int GetWordCount<T>() where T : unmanaged, INetworkStruct
	{
		return GetWordCount(typeof(T));
	}

	public static int GetWordCount(Type type)
	{
		Assert.Check(typeof(INetworkStruct).IsAssignableFrom(type));
		if (!_wordCounts.TryGetValue(type, out var value))
		{
			NetworkStructWeavedAttribute networkStructWeavedAttribute = (NetworkStructWeavedAttribute)type.GetCustomAttributes(typeof(NetworkStructWeavedAttribute), inherit: false)[0];
			int num = Native.SizeOf(type);
			int num2 = networkStructWeavedAttribute.WordCount * 4;
			if (networkStructWeavedAttribute.IsGenericComposite)
			{
				Assert.Always(type.IsGenericType, "Type not generic {0}", type);
				Type[] genericArguments = type.GetGenericArguments();
				foreach (Type type2 in genericArguments)
				{
					if (typeof(INetworkStruct).IsAssignableFrom(type2))
					{
						num2 += GetWordCount(type2) * 4;
					}
				}
			}
			if (num2 != num)
			{
				Assert.AlwaysFail($"Size of {type} is invalid, expected size {num2} but was size {num}");
			}
			_wordCounts.Add(type, value = networkStructWeavedAttribute.WordCount);
		}
		return value;
	}
}
