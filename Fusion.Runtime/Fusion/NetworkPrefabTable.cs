#define TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

public class NetworkPrefabTable
{
	private struct PrefabAcquireData
	{
		public uint RawValue;

		public int InstanceCount
		{
			get
			{
				return (int)(RawValue & 0x7FFFFFFF);
			}
			set
			{
				RawValue = (RawValue & 0x80000000u) | ((uint)value & 0x7FFFFFFFu);
			}
		}

		public bool IsSynchronous
		{
			get
			{
				return (RawValue & 0x80000000u) != 0;
			}
			set
			{
				RawValue = (RawValue & 0x7FFFFFFFu) | (value ? 2147483648u : 0u);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CGetEntries_003Ed__12 : IEnumerable<(NetworkPrefabId, INetworkPrefabSource)>, IEnumerable, IEnumerator<(NetworkPrefabId, INetworkPrefabSource)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (NetworkPrefabId, INetworkPrefabSource) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public NetworkPrefabTable _003C_003E4__this;

		private int _003Ci_003E5__1;

		(NetworkPrefabId, INetworkPrefabSource) IEnumerator<(NetworkPrefabId, INetworkPrefabSource)>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetEntries_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__1 = 0;
				break;
			case 1:
			{
				_003C_003E1__state = -1;
				int num = _003Ci_003E5__1 + 1;
				_003Ci_003E5__1 = num;
				break;
			}
			}
			if (_003Ci_003E5__1 < _003C_003E4__this._sources.Count)
			{
				_003C_003E2__current = (NetworkPrefabId.FromIndex(_003Ci_003E5__1), _003C_003E4__this._sources[_003Ci_003E5__1]);
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<(NetworkPrefabId, INetworkPrefabSource)> IEnumerable<(NetworkPrefabId, INetworkPrefabSource)>.GetEnumerator()
		{
			_003CGetEntries_003Ed__12 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CGetEntries_003Ed__12(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(NetworkPrefabId, INetworkPrefabSource)>)this).GetEnumerator();
		}
	}

	public NetworkPrefabTableOptions Options = NetworkPrefabTableOptions.Default;

	private List<INetworkPrefabSource> _sources = new List<INetworkPrefabSource>();

	private BitSet64[] _acquireMask = Array.Empty<BitSet64>();

	private const int BitsPerMask = 64;

	private PrefabAcquireData[] _acquireData = Array.Empty<PrefabAcquireData>();

	private Dictionary<NetworkObjectGuid, int> _guidToIndex = new Dictionary<NetworkObjectGuid, int>();

	private int _version;

	public IReadOnlyList<INetworkPrefabSource> Prefabs => _sources;

	public int Version => _version;

	[IteratorStateMachine(typeof(_003CGetEntries_003Ed__12))]
	public IEnumerable<(NetworkPrefabId, INetworkPrefabSource)> GetEntries()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEntries_003Ed__12(-2)
		{
			_003C_003E4__this = this
		};
	}

	public NetworkPrefabId AddSource(INetworkPrefabSource source)
	{
		if (!TryAddSource(source, out var id))
		{
			throw new ArgumentException($"Source with guid {source.AssetGuid} already exists: {id}", "source");
		}
		return id;
	}

	public bool TryAddSource(INetworkPrefabSource source, out NetworkPrefabId id)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		NetworkObjectGuid assetGuid = source.AssetGuid;
		if (assetGuid.IsValid)
		{
			if (_guidToIndex.TryGetValue(assetGuid, out var value))
			{
				id = NetworkPrefabId.FromIndex(value);
				return false;
			}
			_guidToIndex.Add(assetGuid, _sources.Count);
		}
		_sources.Add(source);
		if (_acquireMask.Length < GetBitSetCapacity(_sources.Capacity))
		{
			Array.Resize(ref _acquireMask, GetBitSetCapacity(_sources.Capacity));
		}
		if (_acquireData.Length < _sources.Capacity)
		{
			Array.Resize(ref _acquireData, _sources.Capacity);
		}
		id = NetworkPrefabId.FromIndex(_sources.Count - 1);
		InternalLogStreams.LogTracePrefab?.Log($"Added prefab source {id}: {source.Description}");
		_version++;
		return true;
	}

	public INetworkPrefabSource GetSource(NetworkObjectGuid guid)
	{
		if (_guidToIndex.TryGetValue(guid, out var value))
		{
			return _sources[value];
		}
		return null;
	}

	public INetworkPrefabSource GetSource(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index))
		{
			return null;
		}
		return _sources[index];
	}

	public NetworkPrefabId GetId(NetworkObjectGuid guid)
	{
		if (_guidToIndex.TryGetValue(guid, out var value))
		{
			return NetworkPrefabId.FromIndex(value);
		}
		return default(NetworkPrefabId);
	}

	public NetworkObjectGuid GetGuid(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index))
		{
			return default(NetworkObjectGuid);
		}
		return _sources[index].AssetGuid;
	}

	public int GetInstancesCount(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index) || !IsAcquired(index))
		{
			return 0;
		}
		return _acquireData[index].InstanceCount;
	}

	public int AddInstance(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index) || !IsAcquired(index))
		{
			return 0;
		}
		InternalLogStreams.LogTracePrefab?.Log($"Increasing {prefabId} instance count (to {_acquireData[index].InstanceCount + 1})");
		_version++;
		return ++_acquireData[index].InstanceCount;
	}

	public int RemoveInstance(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index) || !IsAcquired(index))
		{
			return 0;
		}
		if (_acquireData[index].InstanceCount == 0)
		{
			InternalLogStreams.LogTracePrefab?.Warn($"Can't decrease {prefabId} instance count below zero");
			return 0;
		}
		InternalLogStreams.LogTracePrefab?.Log($"Decreasing {prefabId} instance count (to {_acquireData[index].InstanceCount - 1})");
		int num = --_acquireData[index].InstanceCount;
		if (num == 0 && Options.UnloadPrefabOnReleasingLastInstance)
		{
			UnloadInternal(index);
		}
		_version++;
		return num;
	}

	public bool Contains(NetworkPrefabId prefabId)
	{
		int index;
		return TryDecodePrefabId(prefabId, out index);
	}

	public bool IsAcquired(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index))
		{
			return false;
		}
		return IsAcquired(index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IsAcquired(int index)
	{
		int num = index / 64;
		return _acquireMask[num][index & 0x3F];
	}

	private void SetAcquired(int index, bool value)
	{
		int num = index / 64;
		_acquireMask[num][index & 0x3F] = value;
		_version++;
	}

	public NetworkObject Load(NetworkPrefabId prefabId, bool isSynchronous)
	{
		int num = DecodePrefabId(prefabId);
		INetworkPrefabSource networkPrefabSource = _sources[num];
		if (!IsAcquired(num))
		{
			InternalLogStreams.LogTracePrefab?.Log($"Acquiring {prefabId} ({networkPrefabSource.Description})");
			networkPrefabSource.Acquire(isSynchronous);
			SetAcquired(num, value: true);
			_acquireData[num] = new PrefabAcquireData
			{
				IsSynchronous = isSynchronous
			};
		}
		if (!networkPrefabSource.IsCompleted)
		{
			if (!isSynchronous || _acquireData[num].IsSynchronous)
			{
				return null;
			}
			networkPrefabSource.Release();
			networkPrefabSource.Acquire(synchronous: true);
			_acquireData[num].IsSynchronous = true;
			if (!networkPrefabSource.IsCompleted)
			{
				return null;
			}
		}
		NetworkObject networkObject = networkPrefabSource.WaitForResult();
		Assert.Always(networkObject, "Source for {0} returned null", prefabId);
		return networkObject;
	}

	public bool Unload(NetworkPrefabId prefabId)
	{
		if (!TryDecodePrefabId(prefabId, out var index))
		{
			return false;
		}
		if (!IsAcquired(index))
		{
			return false;
		}
		UnloadInternal(index);
		return true;
	}

	public int UnloadUnreferenced(bool includeIncompleteLoads = false)
	{
		int result = 0;
		for (int i = 0; i < _sources.Count; i += 64)
		{
			BitSet64 bitSet = _acquireMask[i / 64];
			if (!bitSet.Any())
			{
				continue;
			}
			foreach (int item in bitSet)
			{
				if (i + item >= _sources.Count)
				{
					break;
				}
				int index = i + item;
				if (!includeIncompleteLoads && _sources[index].IsCompleted)
				{
					InternalLogStreams.LogTracePrefab?.Log($"Not unloading {NetworkPrefabId.FromIndex(index)}: incomplete load");
				}
				else
				{
					UnloadInternal(i + item);
				}
			}
		}
		return result;
	}

	public void UnloadAll()
	{
		for (int i = 0; i < _sources.Count; i++)
		{
			Unload(NetworkPrefabId.FromIndex(i));
		}
	}

	public void Clear()
	{
		UnloadAll();
		Array.Clear(_acquireData, 0, _sources.Count);
		_acquireMask = Array.Empty<BitSet64>();
		_sources.Clear();
		_guidToIndex.Clear();
		_version = 0;
	}

	private void UnloadInternal(int index)
	{
		InternalLogStreams.LogTracePrefab?.Log($"Unloading {NetworkPrefabId.FromIndex(index)}");
		INetworkPrefabSource networkPrefabSource = _sources[index];
		networkPrefabSource.Release();
		SetAcquired(index, value: false);
		_version++;
	}

	private int DecodePrefabId(NetworkPrefabId prefabId)
	{
		if (!prefabId.IsValid)
		{
			throw new ArgumentException("Invalid prefab id", "prefabId");
		}
		if (prefabId.AsIndex >= _sources.Count)
		{
			throw new ArgumentException($"Unknown prefab id: {prefabId}", "prefabId");
		}
		return prefabId.AsIndex;
	}

	private bool TryDecodePrefabId(NetworkPrefabId prefabId, out int index)
	{
		if (!prefabId.IsValid)
		{
			index = 0;
			return false;
		}
		if (prefabId.AsIndex >= _sources.Count)
		{
			index = 0;
			return false;
		}
		index = prefabId.AsIndex;
		return true;
	}

	private int GetBitSetCapacity(int length)
	{
		return (_sources.Capacity + 63) / 64;
	}
}
