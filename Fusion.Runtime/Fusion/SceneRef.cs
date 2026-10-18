using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct SceneRef : INetworkStruct, IEquatable<SceneRef>
{
	public const int SIZE = 4;

	public const uint FLAG_ADDRESSABLE = 2147483648u;

	[FieldOffset(0)]
	public uint RawValue;

	public static SceneRef None => default(SceneRef);

	public bool IsValid => RawValue != 0;

	public bool IsIndex => (RawValue & 0x80000000u) == 0;

	public int AsIndex
	{
		get
		{
			if (!IsIndex)
			{
				throw new InvalidOperationException($"SceneRef {RawValue:X8} is not an index");
			}
			return (int)(RawValue - 1);
		}
	}

	public uint AsPathHash
	{
		get
		{
			if (IsIndex)
			{
				throw new InvalidOperationException($"SceneRef {RawValue:X8} is not a path hash");
			}
			return RawValue & 0x7FFFFFFFu;
		}
	}

	public bool IsPath(string path)
	{
		if (IsIndex)
		{
			return false;
		}
		return this == FromPath(path);
	}

	public static SceneRef FromIndex(int index)
	{
		if (index < 0 || index == int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		SceneRef result = default(SceneRef);
		result.RawValue = (uint)(index + 1);
		return result;
	}

	public static SceneRef FromPath(string path)
	{
		uint hashCodeDeterministic = (uint)HashCodeUtilities.GetHashCodeDeterministic(path ?? throw new ArgumentNullException("path"));
		hashCodeDeterministic &= 0x7FFFFFFFu;
		SceneRef result = default(SceneRef);
		result.RawValue = 0x80000000u | hashCodeDeterministic;
		return result;
	}

	public static SceneRef FromRaw(uint rawValue)
	{
		SceneRef result = default(SceneRef);
		result.RawValue = rawValue;
		return result;
	}

	public override bool Equals(object obj)
	{
		return obj is SceneRef other && Equals(other);
	}

	public bool Equals(SceneRef other)
	{
		return RawValue == other.RawValue;
	}

	public override int GetHashCode()
	{
		return RawValue.GetHashCode();
	}

	public override string ToString()
	{
		return ToString(brackets: true, prefix: true);
	}

	public string ToString(bool brackets, bool prefix)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (brackets)
		{
			stringBuilder.Append('[');
		}
		if (prefix)
		{
			stringBuilder.Append("Scene:");
		}
		if (IsValid)
		{
			if (IsIndex)
			{
				stringBuilder.Append("#").Append(AsIndex);
			}
			else
			{
				stringBuilder.AppendFormat("0x{0:X8}", AsPathHash);
			}
		}
		else
		{
			stringBuilder.Append("None");
		}
		if (brackets)
		{
			stringBuilder.Append(']');
		}
		return stringBuilder.ToString();
	}

	public static SceneRef Parse(string str)
	{
		ReadOnlySpan<char> span = str.AsSpan();
		if (span.StartsWith("["))
		{
			if (!span.EndsWith("]"))
			{
				throw new FormatException("Invalid SceneRef format: " + str);
			}
			span = span.Slice(1, span.Length - 2);
		}
		if (span.StartsWith("Scene:"))
		{
			span = span.Slice(6);
		}
		if (span.StartsWith("#"))
		{
			return FromIndex(int.Parse(span.Slice(1)));
		}
		if (span.StartsWith("0x"))
		{
			return FromRaw(uint.Parse(span.Slice(2), NumberStyles.HexNumber) | 0x80000000u);
		}
		if (span.SequenceEqual("None".AsSpan()))
		{
			return None;
		}
		throw new FormatException("Invalid SceneRef format: " + str);
	}

	public static bool operator ==(SceneRef a, SceneRef b)
	{
		return a.RawValue == b.RawValue;
	}

	public static bool operator !=(SceneRef a, SceneRef b)
	{
		return a.RawValue != b.RawValue;
	}
}
