using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fusion;

[Serializable]
public class NetworkAssetSourceStaticLazy<T> where T : UnityEngine.Object
{
	[FormerlySerializedAs("Prefab")]
	public LazyLoadReference<T> Object;

	[Obsolete("Use Object instead")]
	public LazyLoadReference<T> Prefab
	{
		get
		{
			return Object;
		}
		set
		{
			Object = value;
		}
	}

	public bool IsCompleted => true;

	public string Description
	{
		get
		{
			if (Object.isBroken)
			{
				return "Static: (broken)";
			}
			if (Object.isSet)
			{
				return "Static: " + Object.asset;
			}
			return "Static: (null)";
		}
	}

	public void Acquire(bool synchronous)
	{
	}

	public void Release()
	{
	}

	public T WaitForResult()
	{
		if (Object.asset == null)
		{
			throw new InvalidOperationException("Missing static reference");
		}
		return Object.asset;
	}
}
