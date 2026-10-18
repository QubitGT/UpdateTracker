using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering;

internal ref struct HashFNV1A32
{
	private const uint k_Prime = 16777619u;

	private const uint k_OffsetBasis = 2166136261u;

	private uint m_Hash;

	public int value => (int)m_Hash;

	public static HashFNV1A32 Create()
	{
		HashFNV1A32 result = default(HashFNV1A32);
		result.m_Hash = 2166136261u;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in int input)
	{
		m_Hash = (m_Hash ^ (uint)input) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in uint input)
	{
		m_Hash = (m_Hash ^ input) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in bool input)
	{
		m_Hash = (m_Hash ^ (uint)(input ? 1 : 0)) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in float input)
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in double input)
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in Vector2 input)
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in Vector3 input)
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append(in Vector4 input)
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Append<T>(T input) where T : struct
	{
		m_Hash = (m_Hash ^ (uint)input.GetHashCode()) * 16777619;
	}

	public override int GetHashCode()
	{
		return value;
	}
}
