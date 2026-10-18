using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterVector3 : IElementReaderWriter<Vector3>
{
	private static IElementReaderWriter<Vector3> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe Vector3 Read(byte* data, int index)
	{
		return *(Vector3*)(data + index * 12);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref Vector3 ReadRef(byte* data, int index)
	{
		return ref *(Vector3*)(data + index * 12);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, Vector3 val)
	{
		*(Vector3*)(data + index * 12) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(Vector3 val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<Vector3> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterVector3);
		}
		return _instance;
	}
}
