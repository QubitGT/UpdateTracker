using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Fusion.Sockets;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[InlineHelp]
[NetworkStructWeaved(2)]
public struct NetworkObjectTypeId : INetworkStruct, IEquatable<NetworkObjectTypeId>
{
	public sealed class EqualityComparer : IEqualityComparer<NetworkObjectTypeId>
	{
		public bool Equals(NetworkObjectTypeId x, NetworkObjectTypeId y)
		{
			return x.Equals(y);
		}

		public int GetHashCode(NetworkObjectTypeId obj)
		{
			return obj.GetHashCode();
		}
	}

	public const int SIZE = 8;

	public const int ALIGNMENT = 4;

	private const int KIND_MASK = 3;

	private const int KIND_BITS = 2;

	private const int SCENE_OBJECT_INDEX_SHIFT = 2;

	private const int SCENE_OBJECT_INDEX_BITS = 22;

	private const int SCENE_OBJECT_INDEX_MASK = 4194303;

	private const int SCENE_OBJECT_LOAD_ID_SHIFT = 24;

	private const int SCENE_OBJECT_LOAD_ID_BITS = 8;

	public const int MAX_SCENE_OBJECT_INDEX = 4194303;

	private const ushort STRUCT_TYPE_PLAYERDATA = 1;

	[FieldOffset(0)]
	private uint _value0;

	[FieldOffset(4)]
	private uint _value1;

	public static EqualityComparer Comparer { get; } = new EqualityComparer();


	public static NetworkObjectTypeId PlayerData => FromStruct(1);

	public NetworkTypeIdKind Kind
	{
		get
		{
			if (_value0 == 0 && _value1 == 0)
			{
				return NetworkTypeIdKind.Invalid;
			}
			return (NetworkTypeIdKind)((int)_value1 & 3);
		}
	}

	public NetworkSceneObjectId AsSceneObjectId
	{
		get
		{
			if (!IsSceneObject)
			{
				throw new InvalidOperationException($"Invalid kind, got {Kind}, expected {NetworkTypeIdKind.SceneObject}");
			}
			SceneRef scene = SceneRef.FromRaw(_value0);
			int objectId = (int)((_value1 >> 2) & 0x3FFFFF);
			byte b = (byte)(_value1 >> 24);
			NetworkSceneObjectId result = default(NetworkSceneObjectId);
			result.ObjectId = objectId;
			result.Scene = scene;
			result.LoadId = b;
			return result;
		}
	}

	public NetworkPrefabId AsPrefabId
	{
		get
		{
			if (!IsPrefab)
			{
				throw new InvalidOperationException($"Invalid kind, got {Kind}, expected {NetworkTypeIdKind.Prefab}");
			}
			return NetworkPrefabId.FromRaw(_value0);
		}
	}

	public uint AsCustom
	{
		get
		{
			if (!IsCustom)
			{
				throw new InvalidOperationException($"Invalid kind, got {Kind}, expected {NetworkTypeIdKind.Custom}");
			}
			return _value0;
		}
	}

	public ushort AsInternalStructId
	{
		get
		{
			if (!IsStruct)
			{
				throw new InvalidOperationException($"Invalid kind, got {Kind}, expected {NetworkTypeIdKind.InternalStruct}");
			}
			return (ushort)_value0;
		}
	}

	public bool IsNone => Kind == NetworkTypeIdKind.Invalid;

	public bool IsValid => Kind != NetworkTypeIdKind.Invalid;

	public bool IsSceneObject => Kind == NetworkTypeIdKind.SceneObject;

	public bool IsPrefab => Kind == NetworkTypeIdKind.Prefab;

	public bool IsStruct => Kind == NetworkTypeIdKind.InternalStruct;

	public bool IsCustom => Kind == NetworkTypeIdKind.Custom;

	public static NetworkObjectTypeId FromSceneRefAndObjectIndex(SceneRef sceneRef, int objIndex, NetworkSceneLoadId loadId = default(NetworkSceneLoadId))
	{
		NetworkSceneObjectId sceneObjectId = default(NetworkSceneObjectId);
		sceneObjectId.Scene = sceneRef;
		sceneObjectId.ObjectId = objIndex;
		sceneObjectId.LoadId = loadId;
		return FromSceneObjectId(sceneObjectId);
	}

	public static NetworkObjectTypeId FromSceneObjectId(NetworkSceneObjectId sceneObjectId)
	{
		if (!sceneObjectId.Scene.IsValid)
		{
			throw new ArgumentException("SceneRef is not valid", "sceneObjectId");
		}
		if (sceneObjectId.ObjectId < 0 || sceneObjectId.ObjectId > 4194303)
		{
			throw new ArgumentException("ObjectId is out of range", "sceneObjectId");
		}
		NetworkObjectTypeId result = default(NetworkObjectTypeId);
		result._value0 = sceneObjectId.Scene.RawValue;
		result._value1 = 3u | (uint)(sceneObjectId.ObjectId << 2) | (uint)(sceneObjectId.LoadId.Value << 24);
		return result;
	}

	public static NetworkObjectTypeId FromPrefabId(NetworkPrefabId prefabId)
	{
		if (!prefabId.IsValid)
		{
			throw new ArgumentException("PrefabId is not valid", "prefabId");
		}
		NetworkObjectTypeId result = default(NetworkObjectTypeId);
		result._value0 = prefabId.RawValue;
		result._value1 = 0u;
		return result;
	}

	public static NetworkObjectTypeId FromCustom(uint raw)
	{
		NetworkObjectTypeId result = default(NetworkObjectTypeId);
		result._value0 = raw;
		result._value1 = 1u;
		return result;
	}

	public static NetworkObjectTypeId FromStruct(ushort structId)
	{
		NetworkObjectTypeId result = default(NetworkObjectTypeId);
		result._value0 = structId;
		result._value1 = 2u;
		return result;
	}

	public bool Equals(NetworkObjectTypeId other)
	{
		return _value0 == other._value0 && _value1 == other._value1;
	}

	public override int GetHashCode()
	{
		int value = (int)_value0;
		return (value * 397) ^ (int)_value1;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkObjectTypeId other && Equals(other);
	}

	public override string ToString()
	{
		if (!IsValid)
		{
			return "[None]";
		}
		return Kind switch
		{
			NetworkTypeIdKind.InternalStruct => $"[Struct 0x{AsInternalStructId:X4}]", 
			NetworkTypeIdKind.Custom => $"[Custom 0x{AsCustom:X8}]", 
			NetworkTypeIdKind.Prefab => $"[Prefab {AsPrefabId.AsIndex}]", 
			NetworkTypeIdKind.SceneObject => AsSceneObjectId.ToString(), 
			_ => "[Invalid]", 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(NetworkObjectTypeId a, NetworkObjectTypeId b)
	{
		return a._value0 == b._value0 && a._value1 == b._value1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(NetworkObjectTypeId a, NetworkObjectTypeId b)
	{
		return a._value0 != b._value0 || a._value1 != b._value1;
	}

	public static implicit operator NetworkObjectTypeId(NetworkPrefabId prefabId)
	{
		return FromPrefabId(prefabId);
	}

	internal unsafe static void WriteInternal(NetworkObjectTypeId typeId, NetBitBuffer* buffer, int blockSize)
	{
		buffer->WriteUInt32VarLength(typeId._value0, blockSize);
		buffer->WriteUInt32VarLength(typeId._value1, blockSize);
	}

	internal unsafe static NetworkObjectTypeId ReadInternal(NetBitBuffer* buffer, int blockSize)
	{
		NetworkObjectTypeId result = default(NetworkObjectTypeId);
		result._value0 = buffer->ReadUInt32VarLength(blockSize);
		result._value1 = buffer->ReadUInt32VarLength(blockSize);
		return result;
	}
}
