using System;
using System.Runtime.ExceptionServices;
using UnityEngine;

namespace Fusion;

[Serializable]
public class NetworkAssetSourceResource<T> where T : UnityEngine.Object
{
	[UnityResourcePath(typeof(UnityEngine.Object))]
	public string ResourcePath;

	public string SubObjectName;

	[NonSerialized]
	private object _state;

	[NonSerialized]
	private int _acquireCount;

	public bool IsCompleted
	{
		get
		{
			if (_state == null)
			{
				return false;
			}
			if (_state is ResourceRequest { isDone: false })
			{
				return false;
			}
			return true;
		}
	}

	public string Description => "Resource: " + ResourcePath + ((!string.IsNullOrEmpty(SubObjectName)) ? ("[" + SubObjectName + "]") : "");

	public void Acquire(bool synchronous)
	{
		if (_acquireCount == 0)
		{
			LoadInternal(synchronous);
		}
		_acquireCount++;
	}

	public void Release()
	{
		if (_acquireCount <= 0)
		{
			throw new Exception("Asset is not loaded");
		}
		if (--_acquireCount == 0)
		{
			UnloadInternal();
		}
	}

	public T WaitForResult()
	{
		if (_state is ResourceRequest resourceRequest)
		{
			if (resourceRequest.isDone)
			{
				FinishAsyncOp(resourceRequest);
			}
			else
			{
				_state = null;
				LoadInternal(synchronous: true);
			}
		}
		if (_state == null)
		{
			throw new InvalidOperationException($"Failed to load asset {typeof(T)}: {ResourcePath}[{SubObjectName}]. Asset is null.");
		}
		if (_state is T result)
		{
			return result;
		}
		if (_state is ExceptionDispatchInfo exceptionDispatchInfo)
		{
			exceptionDispatchInfo.Throw();
			throw new NotSupportedException();
		}
		throw new InvalidOperationException($"Failed to load asset {typeof(T)}: {ResourcePath}, SubObjectName: {SubObjectName}");
	}

	private void FinishAsyncOp(ResourceRequest asyncOp)
	{
		try
		{
			UnityEngine.Object @object = (string.IsNullOrEmpty(SubObjectName) ? asyncOp.asset : LoadNamedResource(ResourcePath, SubObjectName));
			if ((bool)@object)
			{
				_state = @object;
				return;
			}
			throw new InvalidOperationException("Missing Resource: " + ResourcePath + ", SubObjectName: " + SubObjectName);
		}
		catch (Exception source)
		{
			_state = ExceptionDispatchInfo.Capture(source);
		}
	}

	private static T LoadNamedResource(string resoucePath, string subObjectName)
	{
		T[] array = Resources.LoadAll<T>(resoucePath);
		foreach (T val in array)
		{
			if (string.Equals(val.name, subObjectName, StringComparison.Ordinal))
			{
				return val;
			}
		}
		return null;
	}

	private void LoadInternal(bool synchronous)
	{
		try
		{
			if (synchronous)
			{
				_state = (string.IsNullOrEmpty(SubObjectName) ? Resources.Load<T>(ResourcePath) : LoadNamedResource(ResourcePath, SubObjectName));
			}
			else
			{
				_state = Resources.LoadAsync<T>(ResourcePath);
			}
			if (_state == null)
			{
				_state = new InvalidOperationException("Missing Resource: " + ResourcePath + ", SubObjectName: " + SubObjectName);
			}
		}
		catch (Exception source)
		{
			_state = ExceptionDispatchInfo.Capture(source);
		}
	}

	private void UnloadInternal()
	{
		if (_state is ResourceRequest resourceRequest)
		{
			resourceRequest.completed += delegate
			{
			};
		}
		else
		{
			_ = _state is UnityEngine.Object;
		}
		_state = null;
	}
}
