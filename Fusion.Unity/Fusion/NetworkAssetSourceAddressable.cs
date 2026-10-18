using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fusion;

[Serializable]
public class NetworkAssetSourceAddressable<T> where T : UnityEngine.Object
{
	[UnityAddressablesRuntimeKey]
	public string RuntimeKey;

	[NonSerialized]
	private int _acquireCount;

	[NonSerialized]
	private AsyncOperationHandle _op;

	[Obsolete("Use RuntimeKey instead")]
	public AssetReference Address
	{
		get
		{
			if (string.IsNullOrEmpty(RuntimeKey))
			{
				return null;
			}
			return FusionAddressablesUtils.CreateAssetReference(RuntimeKey);
		}
		set
		{
			if (value.IsValid())
			{
				RuntimeKey = (string)value.RuntimeKey;
			}
			else
			{
				RuntimeKey = string.Empty;
			}
		}
	}

	public bool IsCompleted => _op.IsDone;

	public string Description => "RuntimeKey: " + RuntimeKey;

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
		if (!_op.IsDone)
		{
			try
			{
				_op.WaitForCompletion();
			}
			catch (Exception ex) when (!Application.isPlaying && typeof(Exception) == ex.GetType())
			{
				InternalLogStreams.LogError?.Log("An exception was thrown when loading asset: " + RuntimeKey + "; since this method was called from the editor, it may be due to the fact that Addressables don't have edit-time load support. Please use EditorInstance instead.");
				throw;
			}
		}
		if (_op.OperationException != null)
		{
			throw new InvalidOperationException("Failed to load asset: " + RuntimeKey, _op.OperationException);
		}
		return ValidateResult(_op.Result);
	}

	private void LoadInternal(bool synchronous)
	{
		_op = Addressables.LoadAssetAsync<UnityEngine.Object>(RuntimeKey);
		if (!_op.IsValid())
		{
			throw new Exception("Failed to load asset: " + RuntimeKey);
		}
		if (_op.Status == AsyncOperationStatus.Failed)
		{
			throw new Exception("Failed to load asset: " + RuntimeKey, _op.OperationException);
		}
		if (synchronous)
		{
			_op.WaitForCompletion();
		}
	}

	private void UnloadInternal()
	{
		if (_op.IsValid())
		{
			AsyncOperationHandle op = _op;
			_op = default(AsyncOperationHandle);
			Addressables.Release(op);
		}
	}

	private T ValidateResult(object result)
	{
		if (result == null)
		{
			throw new InvalidOperationException("Failed to load asset: " + RuntimeKey + "; asset is null");
		}
		if (typeof(T).IsSubclassOf(typeof(Component)))
		{
			if (!(result is GameObject))
			{
				throw new InvalidOperationException($"Failed to load asset: {RuntimeKey}; asset is not a GameObject, but a {result.GetType()}");
			}
			T component = ((GameObject)result).GetComponent<T>();
			if (!component)
			{
				throw new InvalidOperationException($"Failed to load asset: {RuntimeKey}; asset does not contain component {typeof(T)}");
			}
			return component;
		}
		if (result is T result2)
		{
			return result2;
		}
		throw new InvalidOperationException($"Failed to load asset: {RuntimeKey}; asset is not of type {typeof(T)}, but {result.GetType()}");
	}
}
