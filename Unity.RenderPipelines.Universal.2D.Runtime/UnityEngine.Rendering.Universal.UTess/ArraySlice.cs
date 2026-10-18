using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.Universal.UTess;

[DebuggerDisplay("Length = {Length}")]
[DebuggerTypeProxy(typeof(ArraySliceDebugView<>))]
internal struct ArraySlice<T> : IEquatable<ArraySlice<T>> where T : struct
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe byte* m_Buffer;

	internal int m_Stride;

	internal int m_Length;

	public unsafe T this[int index]
	{
		get
		{
			return UnsafeUtility.ReadArrayElementWithStride<T>(m_Buffer, index, m_Stride);
		}
		[WriteAccessRequired]
		set
		{
			UnsafeUtility.WriteArrayElementWithStride(m_Buffer, index, m_Stride, value);
		}
	}

	public int Stride => m_Stride;

	public int Length => m_Length;

	public unsafe ArraySlice(NativeArray<T> array, int start, int length)
	{
		m_Stride = UnsafeUtility.SizeOf<T>();
		byte* buffer = (byte*)array.GetUnsafePtr() + m_Stride * start;
		m_Buffer = buffer;
		m_Length = length;
	}

	public unsafe bool Equals(ArraySlice<T> other)
	{
		if (m_Buffer == other.m_Buffer && m_Stride == other.m_Stride)
		{
			return m_Length == other.m_Length;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is ArraySlice<T>)
		{
			return Equals((ArraySlice<T>)obj);
		}
		return false;
	}

	public unsafe override int GetHashCode()
	{
		return ((((int)m_Buffer * 397) ^ m_Stride) * 397) ^ m_Length;
	}

	public static bool operator ==(ArraySlice<T> left, ArraySlice<T> right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ArraySlice<T> left, ArraySlice<T> right)
	{
		return !left.Equals(right);
	}

	public unsafe static ArraySlice<T> ConvertExistingDataToArraySlice(void* dataPointer, int stride, int length)
	{
		if (length < 0)
		{
			throw new ArgumentException($"Invalid length of '{length}'. It must be greater than 0.", "length");
		}
		if (stride < 0)
		{
			throw new ArgumentException($"Invalid stride '{stride}'. It must be greater than 0.", "stride");
		}
		ArraySlice<T> result = default(ArraySlice<T>);
		result.m_Stride = stride;
		result.m_Buffer = (byte*)dataPointer;
		result.m_Length = length;
		return result;
	}

	internal unsafe void* GetUnsafeReadOnlyPtr()
	{
		return m_Buffer;
	}

	internal unsafe void CopyTo(T[] array)
	{
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		IntPtr intPtr = gCHandle.AddrOfPinnedObject();
		int num = UnsafeUtility.SizeOf<T>();
		UnsafeUtility.MemCpyStride((void*)intPtr, num, GetUnsafeReadOnlyPtr(), Stride, num, m_Length);
		gCHandle.Free();
	}

	internal T[] ToArray()
	{
		T[] array = new T[Length];
		CopyTo(array);
		return array;
	}
}
