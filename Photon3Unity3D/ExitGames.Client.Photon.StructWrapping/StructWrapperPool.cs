using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon.StructWrapping;

public class StructWrapperPool
{
	public static WrappedType GetWrappedType(Type type)
	{
		if (type == typeof(bool))
		{
			return WrappedType.Bool;
		}
		if (type == typeof(byte))
		{
			return WrappedType.Byte;
		}
		if (type == typeof(short))
		{
			return WrappedType.Int16;
		}
		if (type == typeof(int))
		{
			return WrappedType.Int32;
		}
		if (type == typeof(long))
		{
			return WrappedType.Int64;
		}
		if (type == typeof(float))
		{
			return WrappedType.Single;
		}
		if (type == typeof(double))
		{
			return WrappedType.Double;
		}
		return WrappedType.Unknown;
	}
}
public class StructWrapperPool<T> : StructWrapperPool
{
	public const int GROWBY = 4;

	public readonly Type tType = typeof(T);

	public readonly WrappedType wType = StructWrapperPool.GetWrappedType(typeof(T));

	public Stack<StructWrapper<T>> pool;

	public readonly bool isStaticPool;

	public int Count => pool.Count;

	public StructWrapperPool(bool isStaticPool)
	{
		pool = new Stack<StructWrapper<T>>();
		this.isStaticPool = isStaticPool;
	}

	public StructWrapper<T> Acquire()
	{
		StructWrapper<T> structWrapper;
		if (pool.Count == 0)
		{
			int num = 1;
			while (true)
			{
				Pooling releasing = ((!isStaticPool) ? Pooling.Connected : ((Pooling)3));
				structWrapper = new StructWrapper<T>(releasing, tType, wType);
				structWrapper.ReturnPool = this;
				if (num == 4)
				{
					break;
				}
				pool.Push(structWrapper);
				num++;
				bool flag = true;
			}
		}
		else
		{
			structWrapper = pool.Pop();
		}
		structWrapper.pooling |= Pooling.CheckedOut;
		return structWrapper;
	}

	public StructWrapper<T> Acquire(T value)
	{
		StructWrapper<T> structWrapper = Acquire();
		structWrapper.value = value;
		return structWrapper;
	}

	internal void Release(StructWrapper<T> obj)
	{
		obj.pooling &= (Pooling)(-9);
		pool.Push(obj);
	}
}
