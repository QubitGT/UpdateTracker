using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace UnityEngine.AddressableAssets;

[Serializable]
public class AssetReference : IKeyEvaluator
{
	[FormerlySerializedAs("m_assetGUID")]
	[SerializeField]
	protected internal string m_AssetGUID = "";

	[SerializeField]
	private string m_SubObjectName;

	[SerializeField]
	private string m_SubObjectType;

	private AsyncOperationHandle m_Operation;

	public AsyncOperationHandle OperationHandle
	{
		get
		{
			return m_Operation;
		}
		internal set
		{
			m_Operation = value;
		}
	}

	public virtual object RuntimeKey
	{
		get
		{
			if (m_AssetGUID == null)
			{
				m_AssetGUID = string.Empty;
			}
			if (!string.IsNullOrEmpty(m_SubObjectName))
			{
				return $"{m_AssetGUID}[{m_SubObjectName}]";
			}
			return m_AssetGUID;
		}
	}

	public virtual string AssetGUID => m_AssetGUID;

	public virtual string SubObjectName
	{
		get
		{
			return m_SubObjectName;
		}
		set
		{
			m_SubObjectName = value;
		}
	}

	internal virtual Type SubObjectType
	{
		get
		{
			if (!string.IsNullOrEmpty(m_SubObjectName) && m_SubObjectType != null)
			{
				return Type.GetType(m_SubObjectType);
			}
			return null;
		}
	}

	public bool IsDone => m_Operation.IsDone;

	public virtual Object Asset
	{
		get
		{
			if (!m_Operation.IsValid())
			{
				return null;
			}
			return m_Operation.Result as Object;
		}
	}

	public bool IsValid()
	{
		return m_Operation.IsValid();
	}

	public AssetReference()
	{
	}

	public AssetReference(string guid)
	{
		m_AssetGUID = guid;
	}

	public override string ToString()
	{
		return "[" + m_AssetGUID + "]";
	}

	private static AsyncOperationHandle<T> CreateFailedOperation<T>()
	{
		Addressables.InitializeAsync();
		return Addressables.ResourceManager.CreateCompletedOperation(default(T), new Exception("Attempting to load an asset reference that has no asset assigned to it.").Message);
	}

	public virtual AsyncOperationHandle<TObject> LoadAssetAsync<TObject>()
	{
		AsyncOperationHandle<TObject> asyncOperationHandle = default(AsyncOperationHandle<TObject>);
		if (m_Operation.IsValid())
		{
			Debug.LogError("Attempting to load AssetReference that has already been loaded. Handle is exposed through getter OperationHandle");
		}
		else
		{
			asyncOperationHandle = Addressables.LoadAssetAsync<TObject>(RuntimeKey);
			OperationHandle = asyncOperationHandle;
		}
		return asyncOperationHandle;
	}

	public virtual AsyncOperationHandle<SceneInstance> LoadSceneAsync(LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
	{
		AsyncOperationHandle<SceneInstance> asyncOperationHandle = default(AsyncOperationHandle<SceneInstance>);
		if (m_Operation.IsValid())
		{
			Debug.LogError("Attempting to load AssetReference Scene that has already been loaded. Handle is exposed through getter OperationHandle");
		}
		else
		{
			asyncOperationHandle = Addressables.LoadSceneAsync(RuntimeKey, loadMode, activateOnLoad, priority);
			OperationHandle = asyncOperationHandle;
		}
		return asyncOperationHandle;
	}

	public virtual AsyncOperationHandle<SceneInstance> UnLoadScene()
	{
		return Addressables.UnloadSceneAsync(m_Operation);
	}

	public virtual AsyncOperationHandle<GameObject> InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
	{
		return Addressables.InstantiateAsync(RuntimeKey, position, rotation, parent);
	}

	public virtual AsyncOperationHandle<GameObject> InstantiateAsync(Transform parent = null, bool instantiateInWorldSpace = false)
	{
		return Addressables.InstantiateAsync(RuntimeKey, parent, instantiateInWorldSpace);
	}

	public virtual bool RuntimeKeyIsValid()
	{
		string text = RuntimeKey.ToString();
		int num = text.IndexOf('[');
		if (num != -1)
		{
			text = text.Substring(0, num);
		}
		Guid result;
		return Guid.TryParse(text, out result);
	}

	public virtual void ReleaseAsset()
	{
		if (!m_Operation.IsValid())
		{
			Debug.LogWarning("Cannot release a null or unloaded asset.");
			return;
		}
		m_Operation.Release();
		m_Operation = default(AsyncOperationHandle);
	}

	public virtual void ReleaseInstance(GameObject obj)
	{
		Addressables.ReleaseInstance(obj);
	}

	public virtual bool ValidateAsset(Object obj)
	{
		return true;
	}

	public virtual bool ValidateAsset(string path)
	{
		return true;
	}
}
