using System.Runtime.CompilerServices;
using UnityEngine;

namespace Fusion;

public static class ReadWriteUtils
{
	public const float ACCURACY = 1024f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteFloat(int* data, float f)
	{
		*(float*)data = f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static float ReadFloat(int* data)
	{
		return *(float*)data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteVector2(int* data, Vector2 value)
	{
		*(Vector2*)data = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Vector2 ReadVector2(int* data)
	{
		return *(Vector2*)data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteVector3(int* data, Vector3 value)
	{
		*(Vector3*)data = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Vector3 ReadVector3(int* data)
	{
		return *(Vector3*)data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteVector4(int* data, Vector4 value)
	{
		*(Vector4*)data = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Vector4 ReadVector4(int* data)
	{
		return *(Vector4*)data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteQuaternion(int* data, Quaternion value)
	{
		*(Quaternion*)data = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Quaternion ReadQuaternion(int* data)
	{
		return *(Quaternion*)data;
	}
}
