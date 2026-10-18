#define DEBUG
using System.Runtime.CompilerServices;

namespace Fusion;

internal class NetworkObjectPriorityList
{
	public PlayerRef Player;

	public NetworkObjectConnectionDataList Idle;

	private NetworkObjectConnectionDataList[] Levels = new NetworkObjectConnectionDataList[5];

	public NetworkObjectConnectionDataList GetLevelList(int level)
	{
		return Levels[level];
	}

	public void IncreasePriorities()
	{
		for (int i = 1; i < Levels.Length - 1; i++)
		{
			if (Levels[i + 1].Head != null)
			{
				for (NetworkObjectConnectionData networkObjectConnectionData = Levels[i + 1].Head; networkObjectConnectionData != null; networkObjectConnectionData = networkObjectConnectionData.Next)
				{
					networkObjectConnectionData.PriorityLevel = NetworkObjectMeta.EncodePriorityLevel(i);
				}
				if (Levels[i].Head == null)
				{
					Levels[i] = Levels[i + 1];
				}
				else
				{
					Levels[i + 1].Head.Prev = Levels[i].Tail;
					Levels[i].Tail.Next = Levels[i + 1].Head;
					Levels[i].Tail = Levels[i + 1].Tail;
					Levels[i].Count += Levels[i + 1].Count;
				}
				Levels[i + 1] = default(NetworkObjectConnectionDataList);
			}
		}
	}

	public void SetIdle(NetworkObjectConnectionData item)
	{
		if (!NetworkObjectMeta.IsIdle(item.PriorityLevel))
		{
			if (NetworkObjectMeta.IsActive(item.PriorityLevel))
			{
				Levels[NetworkObjectMeta.DecodePriorityLevel(item.PriorityLevel)].Remove(item);
			}
			item.PriorityLevel = -32768;
			Idle.AddLast(item);
			Assert.Check(NetworkObjectMeta.IsIdle(item.PriorityLevel));
		}
	}

	public void SetActive(NetworkObjectConnectionData item, NetworkObjectMeta meta)
	{
		if (!NetworkObjectMeta.IsActive(item.PriorityLevel))
		{
			if (NetworkObjectMeta.IsIdle(item.PriorityLevel))
			{
				Idle.Remove(item);
			}
			item.PriorityLevel = meta.GetPriority(Player);
			Assert.Check(NetworkObjectMeta.IsActive(item.PriorityLevel));
			Levels[NetworkObjectMeta.DecodePriorityLevel(item.PriorityLevel)].AddLast(item);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(NetworkObjectConnectionData item)
	{
		Assert.Check(NetworkObjectMeta.IsActive(item.PriorityLevel));
		Levels[NetworkObjectMeta.DecodePriorityLevel(item.PriorityLevel)].AddLast(item);
	}

	public void RemoveSent(NetworkObjectConnectionData item)
	{
		Assert.Check(item.PriorityLevel != -32768);
		Assert.Check(NetworkObjectMeta.IsActive(item.PriorityLevel));
		Levels[NetworkObjectMeta.DecodePriorityLevel(item.PriorityLevel)].Remove(item);
	}

	public void Remove(NetworkObjectConnectionData item)
	{
		if (item.PriorityLevel == -32768)
		{
			Idle.Remove(item);
		}
		else if (NetworkObjectMeta.IsActive(item.PriorityLevel))
		{
			Levels[NetworkObjectMeta.DecodePriorityLevel(item.PriorityLevel)].Remove(item);
		}
	}
}
