using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Properties;

namespace Unity.Collections;

[NativeContainer]
[DebuggerDisplay("Length = {Length}")]
[GenerateTestsForBurstCompatibility]
public struct NativeText : INativeList<byte>, IIndexable<byte>, INativeDisposable, IDisposable, IUTF8Bytes, IComparable<string>, IEquatable<string>, IComparable<NativeText>, IEquatable<NativeText>, IComparable<FixedString32Bytes>, IEquatable<FixedString32Bytes>, IComparable<FixedString64Bytes>, IEquatable<FixedString64Bytes>, IComparable<FixedString128Bytes>, IEquatable<FixedString128Bytes>, IComparable<FixedString512Bytes>, IEquatable<FixedString512Bytes>, IComparable<FixedString4096Bytes>, IEquatable<FixedString4096Bytes>
{
	public struct Enumerator : IEnumerator<Unicode.Rune>, IEnumerator, IDisposable
	{
		private ReadOnly target;

		private int offset;

		private Unicode.Rune current;

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return Current;
			}
		}

		public Unicode.Rune Current => current;

		public Enumerator(NativeText source)
		{
			target = source.AsReadOnly();
			offset = 0;
			current = default(Unicode.Rune);
		}

		public Enumerator(ReadOnly source)
		{
			target = source;
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

	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct ReadOnly : INativeList<byte>, IIndexable<byte>, IUTF8Bytes, IComparable<string>, IEquatable<string>, IComparable<NativeText>, IEquatable<NativeText>, IComparable<FixedString32Bytes>, IEquatable<FixedString32Bytes>, IComparable<FixedString64Bytes>, IEquatable<FixedString64Bytes>, IComparable<FixedString128Bytes>, IEquatable<FixedString128Bytes>, IComparable<FixedString512Bytes>, IEquatable<FixedString512Bytes>, IComparable<FixedString4096Bytes>, IEquatable<FixedString4096Bytes>
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeText* m_Data;

		public unsafe int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get
			{
				return m_Data->Capacity;
			}
			set
			{
			}
		}

		public unsafe bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get
			{
				if (m_Data == null)
				{
					return true;
				}
				return m_Data->IsEmpty;
			}
			set
			{
			}
		}

		public unsafe int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get
			{
				return m_Data->Length;
			}
			set
			{
			}
		}

		public unsafe byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			readonly get
			{
				return m_Data->ElementAt(index);
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
			}
		}

		[CreateProperty]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[ExcludeFromBurstCompatTesting("Returns managed string")]
		public string Value => ToString();

		internal unsafe ReadOnly(UnsafeText* text)
		{
			m_Data = text;
		}

		public void Clear()
		{
		}

		public ref byte ElementAt(int index)
		{
			throw new NotSupportedException("Trying to retrieve non-readonly ref to NativeText.ReadOnly data. This is not permitted.");
		}

		public unsafe byte* GetUnsafePtr()
		{
			return m_Data->GetUnsafePtr();
		}

		public bool TryResize(int newLength, NativeArrayOptions clearOptions = NativeArrayOptions.ClearMemory)
		{
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		internal unsafe static void CheckNull(void* dataPtr)
		{
			if (dataPtr == null)
			{
				throw new InvalidOperationException("NativeText.ReadOnly has yet to be created or has been destroyed!");
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private readonly void CheckRead()
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void ErrorWrite()
		{
			throw new NotSupportedException("Trying to write to a NativeText.ReadOnly. Write operations are not permitted and are ignored.");
		}

		[ExcludeFromBurstCompatTesting("Takes managed string")]
		public unsafe int CompareTo(string other)
		{
			return m_Data->ToString().CompareTo(other);
		}

		[ExcludeFromBurstCompatTesting("Takes managed string")]
		public unsafe bool Equals(string other)
		{
			return m_Data->ToString().Equals(other);
		}

		public unsafe int CompareTo(ReadOnly other)
		{
			return FixedStringMethods.CompareTo(ref *m_Data, in *other.m_Data);
		}

		public unsafe bool Equals(ReadOnly other)
		{
			return FixedStringMethods.Equals(ref *m_Data, in *other.m_Data);
		}

		public unsafe int CompareTo(NativeText other)
		{
			return FixedStringMethods.CompareTo(ref this, in *other.m_Data);
		}

		public unsafe bool Equals(NativeText other)
		{
			return FixedStringMethods.Equals(ref this, in *other.m_Data);
		}

		public int CompareTo(FixedString32Bytes other)
		{
			return FixedStringMethods.CompareTo(ref this, in other);
		}

		public unsafe static bool operator ==(in ReadOnly a, in FixedString32Bytes b)
		{
			UnsafeText data = *a.m_Data;
			int length = data.Length;
			int utf8LengthInBytes = b.utf8LengthInBytes;
			byte* unsafePtr = data.GetUnsafePtr();
			byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
			return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
		}

		public static bool operator !=(in ReadOnly a, in FixedString32Bytes b)
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

		public unsafe static bool operator ==(in ReadOnly a, in FixedString64Bytes b)
		{
			UnsafeText data = *a.m_Data;
			int length = data.Length;
			int utf8LengthInBytes = b.utf8LengthInBytes;
			byte* unsafePtr = data.GetUnsafePtr();
			byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
			return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
		}

		public static bool operator !=(in ReadOnly a, in FixedString64Bytes b)
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

		public unsafe static bool operator ==(in ReadOnly a, in FixedString128Bytes b)
		{
			UnsafeText data = *a.m_Data;
			int length = data.Length;
			int utf8LengthInBytes = b.utf8LengthInBytes;
			byte* unsafePtr = data.GetUnsafePtr();
			byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
			return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
		}

		public static bool operator !=(in ReadOnly a, in FixedString128Bytes b)
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

		public unsafe static bool operator ==(in ReadOnly a, in FixedString512Bytes b)
		{
			UnsafeText data = *a.m_Data;
			int length = data.Length;
			int utf8LengthInBytes = b.utf8LengthInBytes;
			byte* unsafePtr = data.GetUnsafePtr();
			byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
			return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
		}

		public static bool operator !=(in ReadOnly a, in FixedString512Bytes b)
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

		public unsafe static bool operator ==(in ReadOnly a, in FixedString4096Bytes b)
		{
			UnsafeText data = *a.m_Data;
			int length = data.Length;
			int utf8LengthInBytes = b.utf8LengthInBytes;
			byte* unsafePtr = data.GetUnsafePtr();
			byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
			return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
		}

		public static bool operator !=(in ReadOnly a, in FixedString4096Bytes b)
		{
			return !(a == b);
		}

		public bool Equals(FixedString4096Bytes other)
		{
			return this == other;
		}

		[ExcludeFromBurstCompatTesting("Returns managed string")]
		public unsafe override string ToString()
		{
			if (m_Data == null)
			{
				return "";
			}
			return FixedStringMethods.ConvertToString(ref this);
		}

		public override int GetHashCode()
		{
			return FixedStringMethods.ComputeHashCode(ref this);
		}

		[ExcludeFromBurstCompatTesting("Takes managed object")]
		public override bool Equals(object other)
		{
			if (other == null)
			{
				return false;
			}
			if (other is string other2)
			{
				return Equals(other2);
			}
			if (other is NativeText other3)
			{
				return Equals(other3);
			}
			if (other is ReadOnly other4)
			{
				return Equals(other4);
			}
			if (other is FixedString32Bytes other5)
			{
				return Equals(other5);
			}
			if (other is FixedString64Bytes other6)
			{
				return Equals(other6);
			}
			if (other is FixedString128Bytes other7)
			{
				return Equals(other7);
			}
			if (other is FixedString512Bytes other8)
			{
				return Equals(other8);
			}
			if (other is FixedString4096Bytes other9)
			{
				return Equals(other9);
			}
			return false;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeText* m_Data;

	public unsafe int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_Data->Length;
		}
		set
		{
			m_Data->Length = value;
		}
	}

	public unsafe int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_Data->Capacity;
		}
		set
		{
			m_Data->Capacity = value;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (!IsCreated)
			{
				return true;
			}
			return m_Data->IsEmpty;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data != null;
		}
	}

	public unsafe byte this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Data->ElementAt(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Data->ElementAt(index) = value;
		}
	}

	[CreateProperty]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[ExcludeFromBurstCompatTesting("Returns managed string")]
	public string Value => ToString();

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public NativeText(string source, Allocator allocator)
		: this(source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	[ExcludeFromBurstCompatTesting("Takes managed string")]
	public unsafe NativeText(string source, AllocatorManager.AllocatorHandle allocator)
		: this(source.Length * 2, allocator)
	{
		Length = source.Length * 2;
		fixed (char* src = source)
		{
			if (UTF8ArrayUnsafeUtility.Copy(GetUnsafePtr(), out var destLength, Capacity, src, source.Length) != 0)
			{
				m_Data->Dispose();
				m_Data = UnsafeText.Alloc(allocator);
				*m_Data = default(UnsafeText);
			}
			Length = destLength;
		}
	}

	public NativeText(int capacity, Allocator allocator)
		: this(capacity, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public unsafe NativeText(int capacity, AllocatorManager.AllocatorHandle allocator)
	{
		m_Data = UnsafeText.Alloc(allocator);
		*m_Data = new UnsafeText(capacity, allocator);
	}

	public NativeText(Allocator allocator)
		: this((AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public NativeText(AllocatorManager.AllocatorHandle allocator)
		: this(512, allocator)
	{
	}

	public unsafe NativeText(in FixedString32Bytes source, AllocatorManager.AllocatorHandle allocator)
		: this(source.utf8LengthInBytes, allocator)
	{
		Length = source.utf8LengthInBytes;
		byte* source2 = (byte*)UnsafeUtilityExtensions.AddressOf(in source.bytes);
		UnsafeUtility.MemCpy(m_Data->GetUnsafePtr(), source2, source.utf8LengthInBytes);
	}

	public NativeText(in FixedString32Bytes source, Allocator allocator)
		: this(in source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public unsafe NativeText(in FixedString64Bytes source, AllocatorManager.AllocatorHandle allocator)
		: this(source.utf8LengthInBytes, allocator)
	{
		Length = source.utf8LengthInBytes;
		byte* source2 = (byte*)UnsafeUtilityExtensions.AddressOf(in source.bytes);
		UnsafeUtility.MemCpy(m_Data->GetUnsafePtr(), source2, source.utf8LengthInBytes);
	}

	public NativeText(in FixedString64Bytes source, Allocator allocator)
		: this(in source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public unsafe NativeText(in FixedString128Bytes source, AllocatorManager.AllocatorHandle allocator)
		: this(source.utf8LengthInBytes, allocator)
	{
		Length = source.utf8LengthInBytes;
		byte* source2 = (byte*)UnsafeUtilityExtensions.AddressOf(in source.bytes);
		UnsafeUtility.MemCpy(m_Data->GetUnsafePtr(), source2, source.utf8LengthInBytes);
	}

	public NativeText(in FixedString128Bytes source, Allocator allocator)
		: this(in source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public unsafe NativeText(in FixedString512Bytes source, AllocatorManager.AllocatorHandle allocator)
		: this(source.utf8LengthInBytes, allocator)
	{
		Length = source.utf8LengthInBytes;
		byte* source2 = (byte*)UnsafeUtilityExtensions.AddressOf(in source.bytes);
		UnsafeUtility.MemCpy(m_Data->GetUnsafePtr(), source2, source.utf8LengthInBytes);
	}

	public NativeText(in FixedString512Bytes source, Allocator allocator)
		: this(in source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public unsafe NativeText(in FixedString4096Bytes source, AllocatorManager.AllocatorHandle allocator)
		: this(source.utf8LengthInBytes, allocator)
	{
		Length = source.utf8LengthInBytes;
		byte* source2 = (byte*)UnsafeUtilityExtensions.AddressOf(in source.bytes);
		UnsafeUtility.MemCpy(m_Data->GetUnsafePtr(), source2, source.utf8LengthInBytes);
	}

	public NativeText(in FixedString4096Bytes source, Allocator allocator)
		: this(in source, (AllocatorManager.AllocatorHandle)allocator)
	{
	}

	public bool TryResize(int newLength, NativeArrayOptions clearOptions = NativeArrayOptions.ClearMemory)
	{
		Length = newLength;
		return true;
	}

	public unsafe byte* GetUnsafePtr()
	{
		return m_Data->GetUnsafePtr();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref byte ElementAt(int index)
	{
		return ref m_Data->ElementAt(index);
	}

	public void Clear()
	{
		Length = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(in byte value)
	{
		this[Length++] = value;
	}

	public unsafe int CompareTo(NativeText other)
	{
		return FixedStringMethods.CompareTo(ref this, in *other.m_Data);
	}

	public unsafe bool Equals(NativeText other)
	{
		return FixedStringMethods.Equals(ref this, in *other.m_Data);
	}

	public int CompareTo(ReadOnly other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public unsafe bool Equals(ReadOnly other)
	{
		return FixedStringMethods.Equals(ref this, in *other.m_Data);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeText.Free(m_Data);
			m_Data = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeTextDisposeJob jobData = default(NativeTextDisposeJob);
		jobData.Data = new NativeTextDispose
		{
			m_TextData = m_Data
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_Data = null;
		return result;
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
	public bool Equals(string other)
	{
		return ToString().Equals(other);
	}

	public int CompareTo(FixedString32Bytes other)
	{
		return FixedStringMethods.CompareTo(ref this, in other);
	}

	public unsafe static bool operator ==(in NativeText a, in FixedString32Bytes b)
	{
		NativeText nativeText = UnsafeUtilityExtensions.AsRef(in a);
		int length = nativeText.Length;
		int utf8LengthInBytes = b.utf8LengthInBytes;
		byte* unsafePtr = nativeText.GetUnsafePtr();
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
	}

	public static bool operator !=(in NativeText a, in FixedString32Bytes b)
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

	public unsafe static bool operator ==(in NativeText a, in FixedString64Bytes b)
	{
		NativeText nativeText = UnsafeUtilityExtensions.AsRef(in a);
		int length = nativeText.Length;
		int utf8LengthInBytes = b.utf8LengthInBytes;
		byte* unsafePtr = nativeText.GetUnsafePtr();
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
	}

	public static bool operator !=(in NativeText a, in FixedString64Bytes b)
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

	public unsafe static bool operator ==(in NativeText a, in FixedString128Bytes b)
	{
		NativeText nativeText = UnsafeUtilityExtensions.AsRef(in a);
		int length = nativeText.Length;
		int utf8LengthInBytes = b.utf8LengthInBytes;
		byte* unsafePtr = nativeText.GetUnsafePtr();
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
	}

	public static bool operator !=(in NativeText a, in FixedString128Bytes b)
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

	public unsafe static bool operator ==(in NativeText a, in FixedString512Bytes b)
	{
		NativeText nativeText = UnsafeUtilityExtensions.AsRef(in a);
		int length = nativeText.Length;
		int utf8LengthInBytes = b.utf8LengthInBytes;
		byte* unsafePtr = nativeText.GetUnsafePtr();
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
	}

	public static bool operator !=(in NativeText a, in FixedString512Bytes b)
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

	public unsafe static bool operator ==(in NativeText a, in FixedString4096Bytes b)
	{
		NativeText nativeText = UnsafeUtilityExtensions.AsRef(in a);
		int length = nativeText.Length;
		int utf8LengthInBytes = b.utf8LengthInBytes;
		byte* unsafePtr = nativeText.GetUnsafePtr();
		byte* bBytes = (byte*)UnsafeUtilityExtensions.AddressOf(in b.bytes);
		return UTF8ArrayUnsafeUtility.EqualsUTF8Bytes(unsafePtr, length, bBytes, utf8LengthInBytes);
	}

	public static bool operator !=(in NativeText a, in FixedString4096Bytes b)
	{
		return !(a == b);
	}

	public bool Equals(FixedString4096Bytes other)
	{
		return this == other;
	}

	[ExcludeFromBurstCompatTesting("Returns managed string")]
	public unsafe override string ToString()
	{
		if (m_Data == null)
		{
			return "";
		}
		return FixedStringMethods.ConvertToString(ref this);
	}

	public override int GetHashCode()
	{
		return FixedStringMethods.ComputeHashCode(ref this);
	}

	[ExcludeFromBurstCompatTesting("Takes managed object")]
	public override bool Equals(object other)
	{
		if (other == null)
		{
			return false;
		}
		if (other is string other2)
		{
			return Equals(other2);
		}
		if (other is NativeText other3)
		{
			return Equals(other3);
		}
		if (other is ReadOnly other4)
		{
			return Equals(other4);
		}
		if (other is FixedString32Bytes other5)
		{
			return Equals(other5);
		}
		if (other is FixedString64Bytes other6)
		{
			return Equals(other6);
		}
		if (other is FixedString128Bytes other7)
		{
			return Equals(other7);
		}
		if (other is FixedString512Bytes other8)
		{
			return Equals(other8);
		}
		if (other is FixedString4096Bytes other9)
		{
			return Equals(other9);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal unsafe static void CheckNull(void* dataPtr)
	{
		if (dataPtr == null)
		{
			throw new InvalidOperationException("NativeText has yet to be created or has been destroyed!");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckWrite()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckWriteAndBumpSecondaryVersion()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckIndexInRange(int index)
	{
		if (index < 0)
		{
			throw new IndexOutOfRangeException($"Index {index} must be positive.");
		}
		if (index >= Length)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of range in NativeText of {Length} length.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void ThrowCopyError(CopyError error, string source)
	{
		throw new ArgumentException($"NativeText: {error} while copying \"{source}\"");
	}

	public unsafe ReadOnly AsReadOnly()
	{
		return new ReadOnly(m_Data);
	}
}
