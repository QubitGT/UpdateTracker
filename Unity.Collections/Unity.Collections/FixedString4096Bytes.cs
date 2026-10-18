using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Properties;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[StructLayout(LayoutKind.Sequential, Size = 4096)]
[GenerateTestsForBurstCompatibility]
public struct FixedString4096Bytes : INativeList<byte>, IIndexable<byte>, IUTF8Bytes, IComparable<string>, IEquatable<string>, IComparable<FixedString32Bytes>, IEquatable<FixedString32Bytes>, IComparable<FixedString64Bytes>, IEquatable<FixedString64Bytes>, IComparable<FixedString128Bytes>, IEquatable<FixedString128Bytes>, IComparable<FixedString512Bytes>, IEquatable<FixedString512Bytes>, IComparable<FixedString4096Bytes>, IEquatable<FixedString4096Bytes>
{
	public struct Enumerator : IEnumerator
	{
		private FixedString4096Bytes target;

		private int offset;

		private Unicode.Rune current;

		public Unicode.Rune Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return current;
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(FixedString4096Bytes other)
		{
			target = other;
			offset = 0;
			current = default(Unicode.Rune);
		}

		public void Dispose()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool MoveNext()
		{
			if (offset >= target.Length)
			{
				return false;
			}
			Unicode.Utf8ToUcs(out current, target.GetUnsafePtr(), ref offset, target.Length);
			return true;
		}

		public void Reset()
		{
			offset = 0;
			current = default(Unicode.Rune);
		}
	}

	internal const ushort utf8MaxLengthInBytes = 4093;

	[SerializeField]
	internal ushort utf8LengthInBytes;

	[SerializeField]
	internal FixedBytes4094 bytes;

	public static int UTF8MaxLengthInBytes => 4093;

	[CreateProperty]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[ExcludeFromBurstCompatTesting("Returns managed string")]
	public string Value => ToString();

	public unsafe int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return utf8LengthInBytes;
		}
		set
		{
			utf8LengthInBytes = (ushort)value;
			GetUnsafePtr()[(int)utf8LengthInBytes] = 0;
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return 4093;
		}
		set
		{
		}
	}

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return utf8LengthInBytes == 0;
		}
	}

	public unsafe byte this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return GetUnsafePtr()[index];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			GetUnsafePtr()[index] = value;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe readonly byte* GetUnsafePtr()
	{
		fixed (FixedBytes4094* result = &bytes)
		{
			return (byte*)result;
		}
	}

	public unsafe bool TryResize(int newLength, NativeArrayOptions clearOptions = NativeArrayOptions.ClearMemory)
	{
		if (newLength < 0 || newLength > 4093)
		{
			return false;
		}
		if (newLength == utf8LengthInBytes)
		{
			return true;
		}
		if (clearOptions == NativeArrayOptions.ClearMemory)
		{
			if (newLength > utf8LengthInBytes)
			{
				UnsafeUtility.MemClear(GetUnsafePtr() + (int)utf8LengthInBytes, newLength - utf8LengthInBytes);
			}
			else
			{
				UnsafeUtility.MemClear(GetUnsafePtr() + newLength, utf8LengthInBytes - newLength);
			}
		}
		utf8LengthInBytes = (ushort)newLength;
		GetUnsafePtr()[(int)utf8LengthInBytes] = 0;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref byte ElementAt(int index)
	{
		return ref GetUnsafePtr()[index];
	}

	public void Clear()
	{
		Length = 0;
	}

	public void Add(in byte value)
	{
		this[Length++] = value;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public int CompareTo(string other)
	{
		return ToString().CompareTo(other);
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public unsafe bool Equals(string other)
	{
		int num = utf8LengthInBytes;
		int length = other.Length;
		byte* utf8Buffer = (byte*)UnsafeUtilityExtensions.AddressOf(in bytes);
		fixed (char* utf16Buffer = other)
		{
			return UTF8ArrayUnsafeUtility.StrCmp(utf8Buffer, num, utf16Buffer, length) == 0;
		}
	}

	public unsafe ref FixedList4096Bytes<byte> AsFixedList()
	{
		return ref UnsafeUtility.AsRef<FixedList4096Bytes<byte>>(UnsafeUtility.AddressOf(ref this));
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public FixedString4096Bytes(string source)
	{
		this = default(FixedString4096Bytes);
		Initialize(source);
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	internal CopyError Initialize(string source)
	{
		return FixedStringMethods.CopyFromTruncated(ref this, source);
	}

	public FixedString4096Bytes(Unicode.Rune rune, int count = 1)
	{
		this = default(FixedString4096Bytes);
		Initialize(rune, count);
	}

	internal FormatError Initialize(Unicode.Rune rune, int count = 1)
	{
		this = default(FixedString4096Bytes);
		return FixedStringMethods.Append(ref this, rune, count);
	}

	internal unsafe FormatError Initialize(byte* srcBytes, int srcLength)
	{
		bytes = default(FixedBytes4094);
		utf8LengthInBytes = 0;
		int destLength = 0;
		FormatError formatError = UTF8ArrayUnsafeUtility.AppendUTF8Bytes(GetUnsafePtr(), ref destLength, 4093, srcBytes, srcLength);
		if (formatError != 0)
		{
			return formatError;
		}
		Length = destLength;
		return FormatError.None;
	}

	public unsafe FixedString4096Bytes(NativeText.ReadOnly other)
	{
		this = default(FixedString4096Bytes);
		Initialize(other.GetUnsafePtr(), other.Length);
	}

	public unsafe FixedString4096Bytes(in UnsafeText other)
	{
		this = default(FixedString4096Bytes);
		Initialize(other.GetUnsafePtr(), other.Length);
	}

	public int CompareTo(FixedString32Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public FixedString4096Bytes(in FixedString32Bytes other)
	{
		this = default(FixedString4096Bytes);
		Initialize(in other);
	}

	internal unsafe FormatError Initialize(in FixedString32Bytes other)
	{
		return Initialize((byte*)UnsafeUtilityExtensions.AddressOf(in other.bytes), other.utf8LengthInBytes);
	}

	public unsafe static bool operator ==(in FixedString4096Bytes a, in FixedString32Bytes b)
	{
		int aLength = a.utf8LengthInBytes;
		int bLength = b.utf8LengthInBytes;
		byte* aBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in a.bytes);
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(aBytes, aLength, bBytes, bLength);
	}

	public static bool operator !=(in FixedString4096Bytes a, in FixedString32Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString32Bytes other)
	{
		return this == other;
	}

	public int CompareTo(FixedString64Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public FixedString4096Bytes(in FixedString64Bytes other)
	{
		this = default(FixedString4096Bytes);
		Initialize(in other);
	}

	internal unsafe FormatError Initialize(in FixedString64Bytes other)
	{
		return Initialize((byte*)UnsafeUtilityExtensions.AddressOf(in other.bytes), other.utf8LengthInBytes);
	}

	public unsafe static bool operator ==(in FixedString4096Bytes a, in FixedString64Bytes b)
	{
		int aLength = a.utf8LengthInBytes;
		int bLength = b.utf8LengthInBytes;
		byte* aBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in a.bytes);
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(aBytes, aLength, bBytes, bLength);
	}

	public static bool operator !=(in FixedString4096Bytes a, in FixedString64Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString64Bytes other)
	{
		return this == other;
	}

	public int CompareTo(FixedString128Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public FixedString4096Bytes(in FixedString128Bytes other)
	{
		this = default(FixedString4096Bytes);
		Initialize(in other);
	}

	internal unsafe FormatError Initialize(in FixedString128Bytes other)
	{
		return Initialize((byte*)UnsafeUtilityExtensions.AddressOf(in other.bytes), other.utf8LengthInBytes);
	}

	public unsafe static bool operator ==(in FixedString4096Bytes a, in FixedString128Bytes b)
	{
		int aLength = a.utf8LengthInBytes;
		int bLength = b.utf8LengthInBytes;
		byte* aBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in a.bytes);
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(aBytes, aLength, bBytes, bLength);
	}

	public static bool operator !=(in FixedString4096Bytes a, in FixedString128Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString128Bytes other)
	{
		return this == other;
	}

	public int CompareTo(FixedString512Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public FixedString4096Bytes(in FixedString512Bytes other)
	{
		this = default(FixedString4096Bytes);
		Initialize(in other);
	}

	internal unsafe FormatError Initialize(in FixedString512Bytes other)
	{
		return Initialize((byte*)UnsafeUtilityExtensions.AddressOf(in other.bytes), other.utf8LengthInBytes);
	}

	public unsafe static bool operator ==(in FixedString4096Bytes a, in FixedString512Bytes b)
	{
		int aLength = a.utf8LengthInBytes;
		int bLength = b.utf8LengthInBytes;
		byte* aBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in a.bytes);
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(aBytes, aLength, bBytes, bLength);
	}

	public static bool operator !=(in FixedString4096Bytes a, in FixedString512Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString512Bytes other)
	{
		return this == other;
	}

	public int CompareTo(FixedString4096Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public FixedString4096Bytes(in FixedString4096Bytes other)
	{
		this = default(FixedString4096Bytes);
		Initialize(in other);
	}

	internal unsafe FormatError Initialize(in FixedString4096Bytes other)
	{
		return Initialize((byte*)UnsafeUtilityExtensions.AddressOf(in other.bytes), other.utf8LengthInBytes);
	}

	public unsafe static bool operator ==(in FixedString4096Bytes a, in FixedString4096Bytes b)
	{
		int aLength = a.utf8LengthInBytes;
		int bLength = b.utf8LengthInBytes;
		byte* aBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in a.bytes);
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(aBytes, aLength, bBytes, bLength);
	}

	public static bool operator !=(in FixedString4096Bytes a, in FixedString4096Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString4096Bytes other)
	{
		return this == other;
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public static implicit operator FixedString4096Bytes(string b)
	{
		return new FixedString4096Bytes(b);
	}

	[ExcludeFromBurstCompatTesting("Returns managed string")]
	public override string ToString()
	{
		return FixedStringMethods.ConvertToString(ref this);
	}

	public override int GetHashCode()
	{
		return FixedStringMethods.ComputeHashCode(ref this);
	}

	[ExcludeFromBurstCompatTesting("Takes managed object")]
	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is string other)
		{
			return Equals(other);
		}
		if (obj is FixedString32Bytes other2)
		{
			return Equals(other2);
		}
		if (obj is FixedString64Bytes other3)
		{
			return Equals(other3);
		}
		if (obj is FixedString128Bytes other4)
		{
			return Equals(other4);
		}
		if (obj is FixedString512Bytes other5)
		{
			return Equals(other5);
		}
		if (obj is FixedString4096Bytes other6)
		{
			return Equals(other6);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private readonly void CheckIndexInRange(int index)
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException($"Index {index} must be positive.");
		}
		if (index >= utf8LengthInBytes)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of range in FixedString4096Bytes of '{utf8LengthInBytes}' Length.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckLengthInRange(int length)
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
		}
		if (length > 4093)
		{
			throw new ArgumentOutOfRangeException($"Length {length} is out of range in FixedString4096Bytes of '{(ushort)4093}' Capacity.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckCapacityInRange(int capacity)
	{
		if (capacity > 4093)
		{
			throw new ArgumentOutOfRangeException($"Capacity {capacity} must be lower than {(ushort)4093}.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckCopyError(CopyError error, string source)
	{
		if (error != 0)
		{
			throw new ArgumentException($"FixedString4096Bytes: {error} while copying \"{source}\"");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckFormatError(FormatError error)
	{
		if (error != 0)
		{
			throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}
}
