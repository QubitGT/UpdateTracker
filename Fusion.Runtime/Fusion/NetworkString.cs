using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 4)]
[DebuggerDisplay("{Value}")]
[NetworkStructWeaved(1, true)]
public struct NetworkString<TSize> : INetworkString, INetworkStruct, IEquatable<NetworkString<TSize>>, IEnumerable<char>, IEnumerable where TSize : unmanaged, IFixedStorage
{
	[SerializeField]
	internal int _length;

	[SerializeField]
	internal TSize _data;

	public unsafe int Capacity => sizeof(TSize) / 4;

	public string Value
	{
		get
		{
			string cache = null;
			Get(ref cache);
			return cache;
		}
		set
		{
			Set(value);
		}
	}

	public int Length => _length;

	public unsafe ref uint this[int index]
	{
		get
		{
			fixed (TSize* ptr = &_data)
			{
				return ref *(uint*)((byte*)ptr + (nint)SafeIndex(index) * (nint)4);
			}
		}
	}

	private int SafeLength
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_length < 0 || _length > Capacity)
			{
				throw new InvalidOperationException($"Invalid Length: {_length}");
			}
			return _length;
		}
	}

	public NetworkString(string value)
	{
		this = default(NetworkString<TSize>);
		Value = value;
	}

	public static implicit operator NetworkString<TSize>(string str)
	{
		NetworkString<TSize> result = default(NetworkString<TSize>);
		result.Set(str);
		return result;
	}

	public static explicit operator string(NetworkString<TSize> str)
	{
		return str.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkString<TSize> a, NetworkString<TSize> b)
	{
		return !a.Equals(ref b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(string a, NetworkString<TSize> b)
	{
		return !b.Equals(a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkString<TSize> a, string b)
	{
		return !a.Equals(b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkString<TSize> a, NetworkString<TSize> b)
	{
		return a.Equals(ref b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(string a, NetworkString<TSize> b)
	{
		return b.Equals(a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkString<TSize> a, string b)
	{
		return a.Equals(b);
	}

	public unsafe bool Get(ref string cache)
	{
		if (cache != null && Compare(cache) == 0)
		{
			return false;
		}
		int safeLength = SafeLength;
		if (safeLength == 0)
		{
			cache = string.Empty;
		}
		else
		{
			fixed (TSize* value = &_data)
			{
				cache = new string((sbyte*)value, 0, safeLength * 4, Encoding.UTF32);
			}
		}
		return true;
	}

	public unsafe bool Set(string value)
	{
		value = value ?? string.Empty;
		fixed (char* ptr = value)
		{
			fixed (TSize* dst = &_data)
			{
				UTF32Tools.ConversionResult conversionResult = UTF32Tools.Convert(value, (uint*)dst, Capacity);
				_length = conversionResult.CodePointCount;
				return conversionResult.CharacterCount == value.Length;
			}
		}
	}

	public int IndexOf(char c, int startIndex = 0)
	{
		return IndexOf((uint)c, startIndex, Length - startIndex);
	}

	public int IndexOf(char c, int startIndex, int count)
	{
		return IndexOf((uint)c, startIndex, count);
	}

	public int IndexOf(uint codePoint, int startIndex = 0)
	{
		return IndexOf(codePoint, startIndex, Length - startIndex);
	}

	public unsafe int IndexOf(uint codePoint, int startIndex, int count)
	{
		int safeLength = SafeLength;
		if (startIndex < 0 || startIndex > safeLength)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (count < 0 || startIndex + count > safeLength)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		fixed (TSize* ptr = &_data)
		{
			uint* ptr2 = (uint*)((byte*)ptr + (nint)startIndex * (nint)4);
			for (int i = 0; i < count; i++)
			{
				if (ptr2[i] == codePoint)
				{
					return startIndex + i;
				}
			}
			return -1;
		}
	}

	public int IndexOf(string str, int startIndex = 0)
	{
		return IndexOf(str, startIndex, SafeLength - startIndex);
	}

	public unsafe int IndexOf(string str, int startIndex, int count)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		if (startIndex < 0 || startIndex > SafeLength)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (count < 0 || startIndex + count > SafeLength)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count < str.Length)
		{
		}
		fixed (TSize* ptr = &_data)
		{
			int num = UTF32Tools.IndexOf((uint*)((byte*)ptr + (nint)startIndex * (nint)4), count, str);
			if (num < 0)
			{
				return num;
			}
			return num + startIndex;
		}
	}

	public int IndexOf<TOtherSize>(NetworkString<TOtherSize> str, int startIndex = 0) where TOtherSize : unmanaged, IFixedStorage
	{
		return IndexOf(ref str, startIndex, SafeLength - startIndex);
	}

	public int IndexOf<TOtherSize>(NetworkString<TOtherSize> str, int startIndex, int count) where TOtherSize : unmanaged, IFixedStorage
	{
		return IndexOf(ref str, startIndex, count);
	}

	public int IndexOf<TOtherSize>(ref NetworkString<TOtherSize> str, int startIndex = 0) where TOtherSize : unmanaged, IFixedStorage
	{
		return IndexOf(ref str, startIndex, SafeLength - startIndex);
	}

	public unsafe int IndexOf<TOtherSize>(ref NetworkString<TOtherSize> str, int startIndex, int count) where TOtherSize : unmanaged, IFixedStorage
	{
		if (startIndex < 0 || startIndex > SafeLength)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (count < 0 || startIndex + count > SafeLength)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count < str.SafeLength)
		{
			return -1;
		}
		fixed (TOtherSize* pattern = &str._data)
		{
			fixed (TSize* ptr = &_data)
			{
				int num = UTF32Tools.IndexOf((uint*)((byte*)ptr + (nint)startIndex * (nint)4), count, (uint*)pattern, str.SafeLength);
				if (num < 0)
				{
					return num;
				}
				return num + startIndex;
			}
		}
	}

	public bool Contains(char c)
	{
		return IndexOf(c) >= 0;
	}

	public bool Contains(uint codePoint)
	{
		return IndexOf(codePoint) >= 0;
	}

	public bool Contains(string str)
	{
		return IndexOf(str) >= 0;
	}

	public bool Contains<TOtherSize>(NetworkString<TOtherSize> str) where TOtherSize : unmanaged, IFixedStorage
	{
		return IndexOf(ref str) >= 0;
	}

	public bool Contains<TOtherSize>(ref NetworkString<TOtherSize> str) where TOtherSize : unmanaged, IFixedStorage
	{
		return IndexOf(ref str) >= 0;
	}

	public NetworkString<TSize> Substring(int startIndex)
	{
		return Substring(startIndex, SafeLength - startIndex);
	}

	public unsafe NetworkString<TSize> Substring(int startIndex, int length)
	{
		if (startIndex < 0 || startIndex >= SafeLength)
		{
			throw new ArgumentOutOfRangeException("startIndex");
		}
		if (length < 0 || startIndex + length > SafeLength)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		NetworkString<TSize> result = default(NetworkString<TSize>);
		fixed (TSize* ptr = &_data)
		{
			result._length = length;
			Native.MemCpy(&result._data, (byte*)ptr + (nint)startIndex * (nint)4, length * 4);
		}
		return result;
	}

	public unsafe NetworkString<TSize> ToLower()
	{
		NetworkString<TSize> result = default(NetworkString<TSize>);
		fixed (TSize* src = &_data)
		{
			UTF32Tools.ToLowerInvariant((uint*)src, (uint*)(&result._data), SafeLength);
			result._length = SafeLength;
		}
		return result;
	}

	public unsafe NetworkString<TSize> ToUpper()
	{
		NetworkString<TSize> result = default(NetworkString<TSize>);
		fixed (TSize* src = &_data)
		{
			UTF32Tools.ToUpperInvariant((uint*)src, (uint*)(&result._data), SafeLength);
			result._length = SafeLength;
		}
		return result;
	}

	public unsafe int GetCharCount()
	{
		fixed (TSize* bytes = &_data)
		{
			return Encoding.UTF32.GetCharCount((byte*)bytes, Length * 4);
		}
	}

	public unsafe int Compare(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (TSize* strB = &_data)
		{
			return UTF32Tools.CompareOrdinal(s, (uint*)strB, SafeLength);
		}
	}

	public unsafe int Compare(NetworkString<TSize> s)
	{
		fixed (TSize* strA = &_data)
		{
			return UTF32Tools.CompareOrdinal((uint*)strA, SafeLength, (uint*)(&s._data), s.SafeLength, ignoreCase: false);
		}
	}

	public unsafe int Compare(ref NetworkString<TSize> s)
	{
		fixed (TSize* strA = &_data)
		{
			fixed (TSize* strB = &s._data)
			{
				return UTF32Tools.CompareOrdinal((uint*)strA, SafeLength, (uint*)strB, s.SafeLength, ignoreCase: false);
			}
		}
	}

	public int Compare<TOtherSize>(NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		return Compare(ref other);
	}

	public unsafe int Compare<TOtherSize>(ref NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		fixed (TOtherSize* strB = &other._data)
		{
			fixed (TSize* strA = &_data)
			{
				return UTF32Tools.CompareOrdinal((uint*)strA, SafeLength, (uint*)strB, other.SafeLength, ignoreCase: false);
			}
		}
	}

	public bool Equals(string s)
	{
		return Compare(s) == 0;
	}

	public override bool Equals(object obj)
	{
		if (obj is INetworkString networkString)
		{
			return networkString.Equals(ref this);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(NetworkString<TSize> other)
	{
		return Compare(ref other) == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(ref NetworkString<TSize> other)
	{
		return Compare(ref other) == 0;
	}

	public bool Equals<TOtherSize>(NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		return Compare(ref other) == 0;
	}

	public bool Equals<TOtherSize>(ref NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		return Compare(ref other) == 0;
	}

	public void Assign(string value)
	{
		Value = value;
	}

	public unsafe bool StartsWith(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (TSize* strA = &_data)
		{
			return UTF32Tools.StartsWithOrdinal((uint*)strA, SafeLength, s);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool StartsWith<TOtherSize>(ref NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		fixed (TOtherSize* strB = &other._data)
		{
			fixed (TSize* strA = &_data)
			{
				return UTF32Tools.StartsWithOrdinal((uint*)strA, SafeLength, (uint*)strB, other.SafeLength);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool EndsWith<TOtherSize>(ref NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage
	{
		fixed (TOtherSize* bStr = &other._data)
		{
			fixed (TSize* strA = &_data)
			{
				return UTF32Tools.EndsWithOrdinal((uint*)strA, SafeLength, (uint*)bStr, other.SafeLength);
			}
		}
	}

	public unsafe bool EndsWith(string s)
	{
		if (s == null)
		{
			throw new ArgumentNullException("s");
		}
		fixed (TSize* strA = &_data)
		{
			return UTF32Tools.EndsWithOrdinal((uint*)strA, SafeLength, s);
		}
	}

	public unsafe override int GetHashCode()
	{
		fixed (TSize* str = &_data)
		{
			return UTF32Tools.GetHashDeterministic((uint*)str, SafeLength);
		}
	}

	public override string ToString()
	{
		return Value;
	}

	public unsafe UTF32Tools.CharEnumerator GetEnumerator()
	{
		fixed (TSize* utf = &_data)
		{
			return new UTF32Tools.CharEnumerator((uint*)utf, Length);
		}
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private int SafeIndex(int index)
	{
		int safeLength = SafeLength;
		if (index < 0 || index >= safeLength)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return index;
	}
}
public static class NetworkString
{
	public unsafe static int GetCapacity<TSize>() where TSize : unmanaged, IFixedStorage
	{
		return sizeof(TSize) / 4;
	}
}
