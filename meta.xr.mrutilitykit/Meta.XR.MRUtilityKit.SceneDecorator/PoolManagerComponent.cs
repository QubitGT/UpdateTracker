using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class PoolManagerComponent : MonoBehaviour
{
	[Serializable]
	public abstract class CallbackProvider : MonoBehaviour
	{
		public abstract Pool<GameObject>.Callbacks GetPoolCallbacks();
	}

	[Serializable]
	internal class PoolableData : MonoBehaviour
	{
		internal Pool<GameObject> Pool;

		internal Vector3 Scale;

		internal MRUKAnchor Anchor;
	}

	[Serializable]
	internal struct PoolDesc
	{
		public enum PoolType
		{
			CIRCULAR,
			FIXED
		}

		public PoolType poolType;

		public GameObject primitive;

		public int size;

		public CallbackProvider callbackProviderOverride;
	}

	private static class DefaultCallbacks
	{
		public static GameObject Create(GameObject primitive)
		{
			bool activeSelf = primitive.activeSelf;
			primitive.SetActive(value: false);
			GameObject result = UnityEngine.Object.Instantiate(primitive, Vector3.zero, Quaternion.identity);
			primitive.SetActive(activeSelf);
			return result;
		}

		public static void OnGet(GameObject go)
		{
			go.SetActive(value: true);
		}

		public static void OnRelease(GameObject go)
		{
			go.SetActive(value: false);
		}
	}

	public static readonly Pool<GameObject>.Callbacks DEFAULT_CALLBACKS = new Pool<GameObject>.Callbacks
	{
		Create = DefaultCallbacks.Create,
		OnGet = DefaultCallbacks.OnGet,
		OnRelease = DefaultCallbacks.OnRelease
	};

	[SerializeField]
	internal PoolDesc[] defaultPools;

	[NonSerialized]
	public PoolManager<GameObject, Pool<GameObject>> poolManager = new PoolManager<GameObject, Pool<GameObject>>();

	protected internal virtual void InitDefaultPools(Pool<GameObject>.Callbacks? defaultCallbacks = null)
	{
		if (!defaultCallbacks.HasValue)
		{
			defaultCallbacks = DEFAULT_CALLBACKS;
		}
		PoolDesc[] array = defaultPools;
		for (int i = 0; i < array.Length; i++)
		{
			PoolDesc poolDesc = array[i];
			Pool<GameObject>.Callbacks callbacks = defaultCallbacks.Value;
			CallbackProvider callbackProvider = ((poolDesc.callbackProviderOverride == null) ? poolDesc.primitive.GetComponent<CallbackProvider>() : poolDesc.callbackProviderOverride);
			if (callbackProvider != null)
			{
				callbacks = callbackProvider.GetPoolCallbacks();
			}
			PoolDesc.PoolType poolType = poolDesc.poolType;
			Pool<GameObject> pool = ((poolType == PoolDesc.PoolType.CIRCULAR || poolType != PoolDesc.PoolType.FIXED) ? ((Pool<GameObject>)new CircularPool<GameObject>(poolDesc.primitive, poolDesc.size, callbacks)) : ((Pool<GameObject>)new FixedPool<GameObject>(poolDesc.primitive, poolDesc.size, callbacks)));
			poolManager.AddPool(poolDesc.primitive, pool);
		}
	}

	public GameObject Create(GameObject primitive, Vector3 position, Quaternion rotation, MRUKAnchor anchor, Transform parent = null)
	{
		Pool<GameObject> pool = poolManager.GetPool(primitive);
		if (pool == null)
		{
			return UnityEngine.Object.Instantiate(primitive, position, rotation, parent);
		}
		Action<GameObject> onGet = pool.callbacks.OnGet;
		pool.callbacks.OnGet = null;
		GameObject gameObject = pool.Get();
		if (gameObject == null)
		{
			pool.callbacks.OnGet = onGet;
			return null;
		}
		if (!gameObject.TryGetComponent<PoolableData>(out var component))
		{
			component = gameObject.AddComponent<PoolableData>();
		}
		component.Scale = gameObject.transform.localScale;
		component.Anchor = anchor;
		component.Pool = pool;
		gameObject.transform.SetParent(parent);
		gameObject.transform.SetPositionAndRotation(position, rotation);
		onGet(gameObject);
		pool.callbacks.OnGet = onGet;
		return gameObject;
	}

	public GameObject Create(GameObject primitive, MRUKAnchor anchor, Transform parent = null, bool instantiateInWorldSpace = false)
	{
		Pool<GameObject> pool = poolManager.GetPool(primitive);
		if (pool == null)
		{
			return UnityEngine.Object.Instantiate(primitive, parent, instantiateInWorldSpace);
		}
		Action<GameObject> onGet = pool.callbacks.OnGet;
		pool.callbacks.OnGet = null;
		GameObject gameObject = pool.Get();
		if (gameObject == null)
		{
			pool.callbacks.OnGet = onGet;
			return null;
		}
		if (!gameObject.TryGetComponent<PoolableData>(out var component))
		{
			component = gameObject.AddComponent<PoolableData>();
		}
		component.Scale = gameObject.transform.localScale;
		component.Anchor = anchor;
		component.Pool = pool;
		gameObject.transform.SetParent(parent);
		if ((bool)parent)
		{
			if (instantiateInWorldSpace)
			{
				gameObject.transform.SetPositionAndRotation(parent.position, parent.rotation);
			}
			else
			{
				gameObject.transform.localRotation = parent.localRotation;
				gameObject.transform.localPosition = parent.localPosition;
			}
		}
		onGet(gameObject);
		pool.callbacks.OnGet = onGet;
		return gameObject;
	}

	public T Create<T>(T primitive, Vector3 position, Quaternion rotation, MRUKAnchor anchor, Transform parent = null) where T : Component
	{
		GameObject gameObject = Create(primitive.gameObject, position, rotation, anchor, parent);
		if (!(gameObject == null))
		{
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public T Create<T>(T primitive, MRUKAnchor anchor, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
	{
		GameObject gameObject = Create(primitive.gameObject, anchor, parent, instantiateInWorldSpace);
		if (!(gameObject == null))
		{
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public void Release(GameObject go)
	{
		if (go.TryGetComponent<PoolableData>(out var component) && component.Pool != null)
		{
			go.transform.localScale = component.Scale;
			component.Anchor = null;
			component.Pool.Release(go);
		}
		else
		{
			UnityEngine.Object.Destroy(go);
		}
	}
}
