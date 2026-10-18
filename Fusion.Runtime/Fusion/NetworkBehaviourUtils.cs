#define TRACE
#define DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

public static class NetworkBehaviourUtils
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct MetaData
	{
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ArrayInitializer<T>
	{
		public static implicit operator NetworkArray<T>(ArrayInitializer<T> arr)
		{
			throw new NotImplementedException("This is a special method that is meant to be used only for [Networked] properties inline initialization.");
		}

		public static implicit operator NetworkLinkedList<T>(ArrayInitializer<T> arr)
		{
			throw new NotImplementedException("This is a special method that is meant to be used only for [Networked] properties inline initialization.");
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct DictionaryInitializer<K, V>
	{
		public static implicit operator NetworkDictionary<K, V>(DictionaryInitializer<K, V> arr)
		{
			throw new NotImplementedException("This is a special method that is meant to be used only for [Networked] properties inline initialization.");
		}
	}

	private static Dictionary<Type, int> _wordCounts = new Dictionary<Type, int>();

	private static Dictionary<Type, RpcInvokeData[]> _invokerDelegates = new Dictionary<Type, RpcInvokeData[]>();

	private static SortedList<string, RpcStaticInvokeDelegate> _staticInvokers = new SortedList<string, RpcStaticInvokeDelegate>();

	private static Dictionary<Type, MetaData> _metaData = new Dictionary<Type, MetaData>();

	public static bool InvokeRpc = false;

	internal static void ResetStatics()
	{
		InvokeRpc = false;
		_metaData.Clear();
		_wordCounts.Clear();
		_invokerDelegates.Clear();
		_staticInvokers.Clear();
	}

	public static MetaData GetMetaData(Type type)
	{
		MetaData value;
		return _metaData.TryGetValue(type, out value) ? value : default(MetaData);
	}

	public static void RegisterMetaData(Type type)
	{
		if (!_metaData.ContainsKey(type))
		{
			MetaData value = default(MetaData);
			_metaData.Add(type, value);
		}
	}

	public static int GetWordCount(NetworkBehaviour behaviour)
	{
		int? dynamicWordCount = behaviour.DynamicWordCount;
		if (dynamicWordCount.HasValue)
		{
			Assert.Check(dynamicWordCount.Value >= 0, "DynamicWordCount returned a negative value {0} {1}", dynamicWordCount.Value, LogUtils.GetDump(behaviour));
			return dynamicWordCount.Value;
		}
		int staticWordCount = GetStaticWordCount(behaviour.GetType());
		Assert.Check(staticWordCount >= 0, "GetStaticWordCount returned a negative value {0} {1}", staticWordCount, LogUtils.GetDump(behaviour));
		return staticWordCount;
	}

	public static bool HasStaticWordCount(Type type)
	{
		Assert.Check(typeof(NetworkBehaviour).IsAssignableFrom(type));
		return ReflectionUtils.GetWeavedAttributeOrThrow(type).WordCount >= 0;
	}

	public static int GetStaticWordCount(Type type)
	{
		Assert.Check(typeof(NetworkBehaviour).IsAssignableFrom(type));
		if (!_wordCounts.TryGetValue(type, out var value))
		{
			NetworkBehaviourWeavedAttribute weavedAttributeOrThrow = ReflectionUtils.GetWeavedAttributeOrThrow(type);
			Assert.Check(weavedAttributeOrThrow.WordCount >= 0);
			_wordCounts.Add(type, value = weavedAttributeOrThrow.WordCount);
		}
		return value;
	}

	public static bool ShouldRegisterRpcInvokeDelegates(Type type)
	{
		return !_invokerDelegates.ContainsKey(type);
	}

	public static void RegisterRpcInvokeDelegates(Type type)
	{
		if (!ShouldRegisterRpcInvokeDelegates(type))
		{
			return;
		}
		List<RpcInvokeData> list = new List<RpcInvokeData>();
		list.Add(default(RpcInvokeData));
		MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			object[] customAttributes = methodInfo.GetCustomAttributes(typeof(NetworkRpcWeavedInvokerAttribute), inherit: false);
			if (customAttributes.Length != 0 && customAttributes[0] is NetworkRpcWeavedInvokerAttribute networkRpcWeavedInvokerAttribute)
			{
				list.Add(new RpcInvokeData
				{
					Key = networkRpcWeavedInvokerAttribute.Key,
					Sources = networkRpcWeavedInvokerAttribute.Sources,
					Targets = networkRpcWeavedInvokerAttribute.Targets,
					Delegate = (RpcInvokeDelegate)Delegate.CreateDelegate(typeof(RpcInvokeDelegate), methodInfo)
				});
			}
			if (methodInfo.DeclaringType == type)
			{
				object[] customAttributes2 = methodInfo.GetCustomAttributes(typeof(NetworkRpcStaticWeavedInvokerAttribute), inherit: false);
				if (customAttributes2.Length != 0 && customAttributes2[0] is NetworkRpcStaticWeavedInvokerAttribute networkRpcStaticWeavedInvokerAttribute)
				{
					_staticInvokers.Add(networkRpcStaticWeavedInvokerAttribute.Key, (RpcStaticInvokeDelegate)Delegate.CreateDelegate(typeof(RpcStaticInvokeDelegate), methodInfo));
				}
			}
		}
		list.Sort((RpcInvokeData a, RpcInvokeData b) => a.Key.CompareTo(b.Key));
		_invokerDelegates.Add(type, list.ToArray());
	}

	public static bool TryGetRpcInvokeDelegateArray(Type type, out RpcInvokeData[] delegates)
	{
		return _invokerDelegates.TryGetValue(type, out delegates);
	}

	public static int GetRpcStaticIndexOrThrow(string key)
	{
		int num = _staticInvokers.IndexOfKey(key);
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("Static RPC not found: " + key);
		}
		return num;
	}

	public static bool TryGetRpcStaticInvokeDelegate(int index, out RpcStaticInvokeDelegate del)
	{
		if (index >= 0 && index < _staticInvokers.Count)
		{
			del = _staticInvokers.Values[index];
			return true;
		}
		del = null;
		return false;
	}

	public static void NotifyRpcPayloadSizeExceeded(string rpc, int size)
	{
		InternalLogStreams.LogError?.Log($"{rpc}: payload is too large ({size} bytes). Max allowed: {512} bytes)");
	}

	public static void NotifyRpcTargetUnreachable(PlayerRef player, string rpc)
	{
		InternalLogStreams.LogError?.Log($"{rpc}: target {player} not reachable.");
	}

	public static void NotifyLocalSimulationNotAllowedToSendRpc(string rpc, NetworkObject obj, int sources)
	{
		InternalLogStreams.LogError?.Log(rpc + ": Local simulation is not allowed to send this RPC on " + obj.Name + ".");
	}

	public static void NotifyLocalTargetedRpcCulled(PlayerRef player, string methodName)
	{
		InternalLogStreams.LogWarn?.Log($"{methodName} culled for target {player}: player is local and InvokeLocal is set to false");
	}

	public static void ThrowIfBehaviourNotInitialized(NetworkBehaviour behaviour)
	{
		if (BehaviourUtils.IsNotAlive(behaviour.Object))
		{
			throw new InvalidOperationException("Behaviour not initialized: Object not set.");
		}
		if (BehaviourUtils.IsNotAlive(behaviour.Runner))
		{
			throw new InvalidOperationException("Behaviour not initialized: Runner not set.");
		}
	}

	public static void NotifyNetworkWrapFailed<T>(T value)
	{
		InternalLogStreams.LogWarn?.Log($"Failed to wrap {value}");
	}

	public static void NotifyNetworkWrapFailed<T>(T value, Type wrapperType)
	{
		InternalLogStreams.LogWarn?.Log($"Failed to wrap {value} as {wrapperType}");
	}

	public static void NotifyNetworkUnwrapFailed<T>(T wrapper, Type valueType)
	{
		InternalLogStreams.LogWarn?.Log($"Failed to unwrap {wrapper} to {valueType}");
	}

	public static void InitializeNetworkArray<T>(NetworkArray<T> networkArray, T[] sourceArray, string name) where T : unmanaged
	{
		int num = ((sourceArray != null) ? sourceArray.Length : 0);
		if (num != 0)
		{
			if (networkArray.Length < num)
			{
				InternalLogStreams.LogError?.Log($"Source array is too long for {name} with capacity of {networkArray.Length}: {num}. Ignoring extra elements.");
				num = networkArray.Length;
			}
			networkArray.CopyFrom(sourceArray, 0, num);
		}
	}

	public static void CopyFromNetworkArray<T>(NetworkArray<T> networkArray, ref T[] dstArray) where T : unmanaged
	{
		if (dstArray?.Length != networkArray.Length)
		{
			dstArray = new T[networkArray.Length];
		}
		networkArray.CopyTo(dstArray);
	}

	public static T[] CloneArray<T>(T[] array)
	{
		if (array == null)
		{
			return Array.Empty<T>();
		}
		T[] array2 = new T[array.Length];
		Array.Copy(array, array2, array.Length);
		return array2;
	}

	public static void InitializeNetworkList<T>(NetworkLinkedList<T> networkList, T[] sourceArray, string name) where T : unmanaged
	{
		int num = ((sourceArray != null) ? sourceArray.Length : 0);
		if (num != 0)
		{
			if (networkList.Capacity < num)
			{
				InternalLogStreams.LogError?.Log($"Source array is too long for {name} with capacity of {networkList.Capacity}: {num}. Ignoring extra elements.");
				num = networkList.Capacity;
			}
			networkList.Clear();
			for (int i = 0; i < num; i++)
			{
				networkList.Add(sourceArray[i]);
			}
		}
	}

	public static void CopyFromNetworkList<T>(NetworkLinkedList<T> networkList, ref T[] dstArray) where T : unmanaged
	{
		if (dstArray?.Length != networkList.Count)
		{
			dstArray = new T[networkList.Count];
		}
		int num = 0;
		foreach (T item in networkList)
		{
			dstArray[num++] = item;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void InternalOnDestroy(SimulationBehaviour obj)
	{
		InternalLogStreams.LogTraceObject?.Log(obj, "OnDestroy");
		obj.Flags |= SimulationBehaviourRuntimeFlags.IsUnityDestroyed;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void InternalOnEnable(SimulationBehaviour obj)
	{
		InternalLogStreams.LogTraceObject?.Log(obj, "OnEnable");
		obj.Flags &= ~SimulationBehaviourRuntimeFlags.IsUnityDisabled;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void InternalOnDisable(SimulationBehaviour obj)
	{
		InternalLogStreams.LogTraceObject?.Log(obj, "OnDisable");
		obj.Flags |= SimulationBehaviourRuntimeFlags.IsUnityDisabled;
	}

	public static void InitializeNetworkDictionary<D, K, V>(NetworkDictionary<K, V> networkDictionary, D dictionary, string name) where D : IDictionary<K, V> where K : unmanaged where V : unmanaged
	{
		int num = dictionary?.Count ?? 0;
		if (num == 0)
		{
			return;
		}
		if (num > networkDictionary.Capacity)
		{
			InternalLogStreams.LogError?.Log($"Source dictionary is too long for {name} with capacity of {networkDictionary.Capacity}: {num}. Ignoring extra elements.");
			num = networkDictionary.Capacity;
		}
		networkDictionary.Clear();
		foreach (KeyValuePair<K, V> item in dictionary)
		{
			if (--num < 0)
			{
				break;
			}
			networkDictionary.Add(item.Key, item.Value);
		}
	}

	public static void CopyFromNetworkDictionary<D, K, V>(NetworkDictionary<K, V> networkDictionary, ref D dictionary) where D : IDictionary<K, V>, new() where K : unmanaged where V : unmanaged
	{
		if (dictionary == null)
		{
			dictionary = new D();
		}
		else
		{
			dictionary.Clear();
		}
		foreach (KeyValuePair<K, V> item in networkDictionary)
		{
			ref D reference = ref dictionary;
			D val = default(D);
			if (val == null)
			{
				val = reference;
				reference = ref val;
			}
			reference.Add(item.Key, item.Value);
		}
	}

	public static SerializableDictionary<K, V> MakeSerializableDictionary<K, V>(Dictionary<K, V> dictionary) where K : unmanaged where V : unmanaged
	{
		return SerializableDictionary<K, V>.Wrap(dictionary);
	}
}
