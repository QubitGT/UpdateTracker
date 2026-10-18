#define DEBUG
using System;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct NetworkButtons : INetworkStruct, IEquatable<NetworkButtons>
{
	[FieldOffset(0)]
	private int _bits;

	public int Bits => _bits;

	public NetworkButtons(int buttons)
	{
		_bits = buttons;
	}

	public bool IsSet(int button)
	{
		Assert.Check((uint)button < 32u);
		return (_bits & (1 << button)) != 0;
	}

	public void SetDown(int button)
	{
		Assert.Check((uint)button < 32u);
		_bits |= 1 << button;
	}

	public void SetUp(int button)
	{
		Assert.Check((uint)button < 32u);
		_bits &= ~(1 << button);
	}

	public void Set(int button, bool state)
	{
		Assert.Check((uint)button < 32u);
		if (state)
		{
			SetDown(button);
		}
		else
		{
			SetUp(button);
		}
	}

	public void SetAllUp()
	{
		_bits = 0;
	}

	public void SetAllDown()
	{
		_bits = -1;
	}

	public bool IsSet<T>(T button) where T : unmanaged, Enum
	{
		Assert.Check(typeof(T).GetEnumUnderlyingType() == typeof(int));
		return IsSet(UnsafeUtility.EnumToInt(button));
	}

	public void SetDown<T>(T button) where T : unmanaged, Enum
	{
		Assert.Check(typeof(T).GetEnumUnderlyingType() == typeof(int));
		SetDown(UnsafeUtility.EnumToInt(button));
	}

	public void SetUp<T>(T button) where T : unmanaged, Enum
	{
		Assert.Check(typeof(T).GetEnumUnderlyingType() == typeof(int));
		SetUp(UnsafeUtility.EnumToInt(button));
	}

	public void Set<T>(T button, bool state) where T : unmanaged, Enum
	{
		Assert.Check(typeof(T).GetEnumUnderlyingType() == typeof(int));
		Set(UnsafeUtility.EnumToInt(button), state);
	}

	public (NetworkButtons, NetworkButtons) GetPressedOrReleased(NetworkButtons previous)
	{
		return (GetPressed(previous), GetReleased(previous));
	}

	public NetworkButtons GetPressed(NetworkButtons previous)
	{
		previous._bits = (previous._bits ^ _bits) & _bits;
		return previous;
	}

	public NetworkButtons GetReleased(NetworkButtons previous)
	{
		previous._bits = (previous._bits ^ _bits) & previous._bits;
		return previous;
	}

	public bool WasPressed(NetworkButtons previous, int button)
	{
		return GetPressed(previous).IsSet(button);
	}

	public bool WasReleased(NetworkButtons previous, int button)
	{
		return GetReleased(previous).IsSet(button);
	}

	public bool WasPressed<T>(NetworkButtons previous, T button) where T : unmanaged, Enum
	{
		return GetPressed(previous).IsSet(button);
	}

	public bool WasReleased<T>(NetworkButtons previous, T button) where T : unmanaged, Enum
	{
		return GetReleased(previous).IsSet(button);
	}

	public bool Equals(NetworkButtons other)
	{
		return _bits == other._bits;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkButtons other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _bits;
	}
}
