using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[InlineHelp]
[NetworkStructWeaved(20)]
public struct NetworkObjectHeader : INetworkStruct, IEquatable<NetworkObjectHeader>
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct PlayerUniqueData
	{
		public const int SIZE = 4;

		public const int WORDS = 1;

		public const int FLAGS_WORD_INDEX = 0;

		[FieldOffset(0)]
		public NetworkObjectHeaderPlayerDataFlags Flags;

		public bool HasFlag(NetworkObjectHeaderPlayerDataFlags flag)
		{
			return (Flags & flag) == flag;
		}

		public void SetFlag(NetworkObjectHeaderPlayerDataFlags flag)
		{
			Flags |= flag;
		}

		public void ClearFlag(NetworkObjectHeaderPlayerDataFlags flag)
		{
			Flags &= ~flag;
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct PlayerUniqueDataChanges
	{
		[FieldOffset(0)]
		public unsafe fixed int Changes[1];

		public unsafe int MaxTick
		{
			get
			{
				int num = Changes[0];
				for (int i = 1; i < 1; i++)
				{
					if (Changes[i] > num)
					{
						num = Changes[i];
					}
				}
				return num;
			}
		}
	}

	public const int SIZE = 80;

	public const int WORDS = 20;

	public const int PLAYER_DATA_WORD = 9;

	[FieldOffset(0)]
	public readonly NetworkId Id;

	[FieldOffset(4)]
	public readonly short WordCount;

	[FieldOffset(6)]
	public readonly short BehaviourCount;

	[FieldOffset(8)]
	public readonly NetworkObjectTypeId Type;

	[FieldOffset(16)]
	public readonly NetworkId NestingRoot;

	[FieldOffset(20)]
	public readonly NetworkObjectNestingKey NestingKey;

	[FieldOffset(24)]
	public readonly NetworkObjectHeaderFlags Flags;

	internal const int READ_ONLY_WORD_COUNT = 7;

	[FieldOffset(28)]
	public PlayerRef InputAuthority;

	[FieldOffset(32)]
	public PlayerRef StateAuthority;

	[FieldOffset(36)]
	internal PlayerUniqueData PlayerData;

	[FieldOffset(40)]
	private unsafe fixed int _reserved[10];

	public int ByteCount => WordCount * 4;

	public NetworkObjectHeader(NetworkId id, short wordCount, short behaviourCount, NetworkObjectTypeId type, NetworkId nestingRoot, NetworkObjectNestingKey nestingKey, NetworkObjectHeaderFlags flags)
	{
		InputAuthority = default(PlayerRef);
		StateAuthority = default(PlayerRef);
		PlayerData = default(PlayerUniqueData);
		Id = id;
		WordCount = wordCount;
		BehaviourCount = behaviourCount;
		Type = type;
		NestingRoot = nestingRoot;
		NestingKey = nestingKey;
		Flags = flags;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use NetworkObjectMeta instead")]
	public unsafe static int* GetDataPointer(NetworkObjectHeader* header)
	{
		return (int*)((byte*)header + (nint)20 * (nint)4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use NetworkObjectMeta instead")]
	public unsafe static int GetDataWordCount(NetworkObjectHeader* header)
	{
		return header->WordCount - (20 + header->BehaviourCount);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use NetworkObjectMeta instead")]
	public unsafe static int* GetBehaviourChangedTickArray(NetworkObjectHeader* header)
	{
		return (int*)((byte*)header + (nint)(header->WordCount - header->BehaviourCount) * (nint)4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use NetworkObjectMeta instead")]
	public unsafe static bool HasMainNetworkTRSP(NetworkObjectHeader* header)
	{
		return (header->Flags & NetworkObjectHeaderFlags.HasMainNetworkTRSP) == NetworkObjectHeaderFlags.HasMainNetworkTRSP;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Obsolete("Use NetworkObjectMeta instead")]
	public unsafe static NetworkTRSPData* GetMainNetworkTRSPData(NetworkObjectHeader* header)
	{
		if (HasMainNetworkTRSP(header))
		{
			return (NetworkTRSPData*)((byte*)header + (nint)20 * (nint)4);
		}
		return null;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append("Id").Append(": ").Append(Id.ToString());
		stringBuilder.Append(", ").Append("WordCount").Append(": ")
			.Append(WordCount);
		stringBuilder.Append(", ").Append("BehaviourCount").Append(": ")
			.Append(BehaviourCount);
		if (Type.IsValid)
		{
			stringBuilder.Append(", ").Append("Type").Append(": ")
				.Append(Type.ToString());
		}
		if (NestingRoot.IsValid)
		{
			stringBuilder.Append(", ").Append("NestingRoot").Append(": ")
				.Append(NestingRoot.ToString());
		}
		if (NestingKey.IsValid)
		{
			stringBuilder.Append(", ").Append("NestingKey").Append(": ")
				.Append(NestingKey.ToString());
		}
		if (WordCount != 0)
		{
			stringBuilder.Append(", ").Append("WordCount").Append(": ")
				.Append(WordCount);
		}
		if (Flags != 0)
		{
			stringBuilder.Append(", ").Append("Flags").Append(": ")
				.Append(Flags.ToString());
		}
		if (InputAuthority != default(PlayerRef))
		{
			stringBuilder.Append(", ").Append("InputAuthority").Append(": ")
				.Append(InputAuthority.ToString());
		}
		if (StateAuthority != default(PlayerRef))
		{
			stringBuilder.Append(", ").Append("StateAuthority").Append(": ")
				.Append(StateAuthority.ToString());
		}
		if (PlayerData.Flags != 0)
		{
			stringBuilder.Append(", ").Append("PlayerData").Append(": ")
				.Append(PlayerData.Flags.ToString());
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public bool Equals(NetworkObjectHeader other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		return obj is NetworkObjectHeader other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hashCode = Id.GetHashCode();
		hashCode = (hashCode * 397) ^ WordCount;
		hashCode = (hashCode * 397) ^ BehaviourCount;
		hashCode = (hashCode * 397) ^ Type.GetHashCode();
		hashCode = (hashCode * 397) ^ NestingRoot.GetHashCode();
		hashCode = (hashCode * 397) ^ NestingKey.GetHashCode();
		hashCode = (hashCode * 397) ^ (int)Flags;
		hashCode = (hashCode * 397) ^ InputAuthority.GetHashCode();
		hashCode = (hashCode * 397) ^ StateAuthority.GetHashCode();
		return (hashCode * 397) ^ PlayerData.GetHashCode();
	}

	public unsafe static bool operator ==(NetworkObjectHeader left, NetworkObjectHeader right)
	{
		return Native.MemCmp(&left, &right, 80) == 0;
	}

	public unsafe static bool operator !=(NetworkObjectHeader left, NetworkObjectHeader right)
	{
		return Native.MemCmp(&left, &right, 80) != 0;
	}
}
