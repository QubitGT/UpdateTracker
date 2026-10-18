using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Burst;

public readonly struct FunctionPointer<T> : IFunctionPointer
{
	[NativeDisableUnsafePtrRestriction]
	private readonly IntPtr _ptr;

	public IntPtr Value => _ptr;

	public T Invoke => Marshal.GetDelegateForFunctionPointer<T>(_ptr);

	public bool IsCreated => _ptr != IntPtr.Zero;

	public FunctionPointer(IntPtr ptr)
	{
		_ptr = ptr;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckIsCreated()
	{
		if (!IsCreated)
		{
			throw new NullReferenceException("Object reference not set to an instance of an object");
		}
	}

	IFunctionPointer IFunctionPointer.FromIntPtr(IntPtr ptr)
	{
		return new FunctionPointer<T>(ptr);
	}
}
