#define DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Fusion;

internal static class ReaderWriterCache
{
	private static readonly Dictionary<Type, object> _readerWriters = new Dictionary<Type, object>();

	public static IElementReaderWriter<T> Get<T>(Type readerWriterType)
	{
		lock (_readerWriters)
		{
			if (_readerWriters.TryGetValue(readerWriterType, out var value))
			{
				return (IElementReaderWriter<T>)value;
			}
			MethodInfo method = readerWriterType.GetMethod("GetInstance", BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				throw new InvalidOperationException($"Can't find GetInstance method on {readerWriterType}");
			}
			object obj = method.Invoke(null, Array.Empty<object>());
			Assert.Check(obj);
			_readerWriters.Add(readerWriterType, obj);
			return (IElementReaderWriter<T>)obj;
		}
	}
}
