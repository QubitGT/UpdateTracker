using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct ElementReaderWriterVector2 : IElementReaderWriter<Vector2>
{
	private static IElementReaderWriter<Vector2> _instance;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe Vector2 Read(byte* data, int index)
	{
		return *(Vector2*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref Vector2 ReadRef(byte* data, int index)
	{
		return ref *(Vector2*)(data + index * 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Write(byte* data, int index, Vector2 val)
	{
		*(Vector2*)(data + index * 8) = val;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementWordCount()
	{
		return 2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetElementHashCode(Vector2 val)
	{
		return val.GetHashCode();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static IElementReaderWriter<Vector2> GetInstance()
	{
		if (_instance == null)
		{
			_instance = default(ElementReaderWriterVector2);
		}
		return _instance;
	}
}
