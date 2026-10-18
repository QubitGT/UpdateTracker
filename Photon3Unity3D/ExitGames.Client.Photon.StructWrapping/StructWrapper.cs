using System;
using UnityEngine;

namespace ExitGames.Client.Photon.StructWrapping;

public abstract class StructWrapper : IDisposable
{
	public readonly WrappedType wrappedType;

	public readonly Type ttype;

	public StructWrapper(Type ttype, WrappedType wrappedType)
	{
		this.ttype = ttype;
		this.wrappedType = wrappedType;
	}

	public abstract object Box();

	public abstract void DisconnectFromPool();

	public abstract void Dispose();

	public abstract string ToString(bool writeType);

	public static implicit operator StructWrapper(bool value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(byte value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(float value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(double value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(short value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(int value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(long value)
	{
		return value.Wrap();
	}

	public static implicit operator bool(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<bool>).Unwrap();
	}

	public static implicit operator byte(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<byte>).Unwrap();
	}

	public static implicit operator float(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<float>).Unwrap();
	}

	public static implicit operator double(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<double>).Unwrap();
	}

	public static implicit operator short(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<short>).Unwrap();
	}

	public static implicit operator int(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<int>).Unwrap();
	}

	public static implicit operator long(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<long>).Unwrap();
	}

	public static implicit operator StructWrapper(Vector2 value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(Vector3 value)
	{
		return value.Wrap();
	}

	public static implicit operator StructWrapper(Quaternion value)
	{
		return value.Wrap();
	}

	public static implicit operator Vector2(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<Vector2>).Unwrap();
	}

	public static implicit operator Vector3(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<Vector3>).Unwrap();
	}

	public static implicit operator Quaternion(StructWrapper wrapper)
	{
		return (wrapper as StructWrapper<Quaternion>).Unwrap();
	}
}
public class StructWrapper<T> : StructWrapper
{
	internal Pooling pooling;

	internal T value;

	internal static StructWrapperPool<T> staticPool = new StructWrapperPool<T>(isStaticPool: true);

	public StructWrapperPool<T> ReturnPool { get; internal set; }

	public StructWrapper(Pooling releasing)
		: base(typeof(T), StructWrapperPool.GetWrappedType(typeof(T)))
	{
		pooling = releasing;
	}

	public StructWrapper(Pooling releasing, Type tType, WrappedType wType)
		: base(tType, wType)
	{
		pooling = releasing;
	}

	public StructWrapper<T> Poke(byte value)
	{
		if (pooling == Pooling.Readonly)
		{
			throw new InvalidOperationException("Trying to Poke the value of a readonly StructWrapper<byte>. Value cannot be modified.");
		}
		return this;
	}

	public StructWrapper<T> Poke(bool value)
	{
		if (pooling == Pooling.Readonly)
		{
			throw new InvalidOperationException("Trying to Poke the value of a readonly StructWrapper<bool>. Value cannot be modified.");
		}
		return this;
	}

	public StructWrapper<T> Poke(T value)
	{
		this.value = value;
		return this;
	}

	public T Unwrap()
	{
		T result = value;
		if (pooling != Pooling.Readonly)
		{
			ReturnPool.Release(this);
		}
		return result;
	}

	public T Peek()
	{
		return value;
	}

	public override object Box()
	{
		T val = value;
		if (ReturnPool != null)
		{
			ReturnPool.Release(this);
		}
		return val;
	}

	public override void Dispose()
	{
		if ((pooling & Pooling.CheckedOut) == Pooling.CheckedOut && ReturnPool != null)
		{
			ReturnPool.Release(this);
		}
	}

	public override void DisconnectFromPool()
	{
		if (pooling != Pooling.Readonly)
		{
			pooling = Pooling.Disconnected;
			ReturnPool = null;
		}
	}

	public override string ToString()
	{
		return Unwrap().ToString();
	}

	public override string ToString(bool writeTypeInfo)
	{
		if (writeTypeInfo)
		{
			return $"(StructWrapper<{wrappedType}>){Unwrap().ToString()}";
		}
		return Unwrap().ToString();
	}

	public static implicit operator StructWrapper<T>(T value)
	{
		return staticPool.Acquire(value);
	}
}
