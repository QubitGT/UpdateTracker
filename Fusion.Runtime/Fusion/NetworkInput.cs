#define DEBUG
using System;

namespace Fusion;

public struct NetworkInput
{
	private unsafe uint* _ptr;

	private int _wordCount;

	public int WordCount => _wordCount;

	public unsafe uint* Data => (_ptr == null) ? null : (_ptr + 1);

	public unsafe bool IsValid => _ptr != null;

	internal unsafe uint* Ptr => _ptr;

	internal unsafe int TypeKey
	{
		get
		{
			return (_ptr == null) ? (-1) : ((int)(*_ptr));
		}
		set
		{
			if (_ptr != null)
			{
				*_ptr = (uint)value;
			}
		}
	}

	public Type Type
	{
		get
		{
			int typeKey = TypeKey;
			if (typeKey == -1)
			{
				return null;
			}
			return NetworkInputUtils.GetType(typeKey);
		}
	}

	internal unsafe static NetworkInput FromRaw(uint* ptr, int wordCount)
	{
		NetworkInput result = default(NetworkInput);
		result._ptr = ptr;
		result._wordCount = wordCount;
		return result;
	}

	internal unsafe static NetworkInput FromRaw(int* ptr, int wordCount)
	{
		NetworkInput result = default(NetworkInput);
		result._ptr = (uint*)ptr;
		result._wordCount = wordCount;
		return result;
	}

	public unsafe bool TryGet<T>(out T input) where T : unmanaged, INetworkInput
	{
		Assert.Check(IsValid);
		if (_ptr == null || TypeKey != NetworkInputUtils.GetTypeKey(typeof(T)))
		{
			input = default(T);
			return false;
		}
		input = *(T*)Data;
		return true;
	}

	public unsafe bool TrySet<T>(T input) where T : unmanaged, INetworkInput
	{
		Assert.Check(IsValid);
		if (_ptr == null || TypeKey != NetworkInputUtils.GetTypeKey(typeof(T)))
		{
			return false;
		}
		*(T*)Data = input;
		return true;
	}

	public unsafe T Get<T>() where T : unmanaged, INetworkInput
	{
		Assert.Check(IsValid);
		Convert<T>();
		return *(T*)Data;
	}

	public unsafe bool Set<T>(T value) where T : unmanaged, INetworkInput
	{
		Assert.Check(IsValid);
		bool result = Convert<T>();
		*(T*)Data = value;
		return result;
	}

	internal unsafe bool Set(Type type, void* value)
	{
		Assert.Check(IsValid);
		int wordCount = NetworkInputUtils.GetWordCount(type);
		if (wordCount >= _wordCount)
		{
			throw new ArgumentException($"Expected max {_wordCount}, got: {wordCount}", "type");
		}
		bool flag = Convert(type);
		Native.MemCpy(Data, value, wordCount * 4);
		return true;
	}

	public bool Convert<T>() where T : unmanaged, INetworkInput
	{
		return Convert(typeof(T));
	}

	public unsafe bool Convert(Type type)
	{
		Assert.Check(IsValid);
		int typeKey = NetworkInputUtils.GetTypeKey(type);
		if (typeKey != TypeKey)
		{
			Native.MemClear(_ptr, _wordCount * 4);
			TypeKey = typeKey;
			return true;
		}
		return false;
	}

	public bool Is<T>() where T : unmanaged, INetworkInput
	{
		Assert.Check(IsValid);
		return TypeKey == NetworkInputUtils.GetTypeKey(typeof(T));
	}
}
