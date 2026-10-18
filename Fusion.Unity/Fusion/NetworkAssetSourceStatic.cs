using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fusion;

[Serializable]
public class NetworkAssetSourceStatic<T> where T : UnityEngine.Object
{
	[FormerlySerializedAs("Prefab")]
	public T Object;

	[Obsolete("Use Asset instead")]
	public T Prefab
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
			if ((bool)Object)
			{
				return "Static: " + Object;
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
		if (Object == null)
		{
			throw new InvalidOperationException("Missing static reference");
		}
		return Object;
	}
}
