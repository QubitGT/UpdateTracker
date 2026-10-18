namespace Fusion;

internal class NetworkObjectConnectionData
{
	public NetworkObjectConnectionData Prev;

	public NetworkObjectConnectionData Next;

	public NetworkId Id;

	public NetworkObjectMeta MetaCache;

	public int PriorityLevel;

	public NetworkObjectConnectionDataStatus Status;

	public bool MainTRSP;

	public Tick TickSent;

	public Tick TickAcknowledged;

	public Tick TickMin;

	public ulong Filter = ulong.MaxValue;

	public NetworkObjectHeader.PlayerUniqueData UniqueData;

	public NetworkObjectHeader.PlayerUniqueDataChanges UniqueDataChanges;

	public (NetworkObjectHeader.PlayerUniqueData values, NetworkObjectHeader.PlayerUniqueDataChanges changes) GetPlayerData()
	{
		return (values: UniqueData, changes: UniqueDataChanges);
	}

	public unsafe void SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags flags, Simulation simulation)
	{
		UniqueData.Flags |= flags;
		ref int fixedElementField = ref UniqueDataChanges.Changes[0];
		fixedElementField = simulation.Tick.Raw;
		TickMin = simulation.Tick;
	}

	public unsafe void ClearPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags flags, Simulation simulation)
	{
		UniqueData.Flags &= ~flags;
		ref int fixedElementField = ref UniqueDataChanges.Changes[0];
		fixedElementField = simulation.Tick.Raw;
		TickMin = simulation.Tick;
	}

	public bool HasPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags flags)
	{
		return (UniqueData.Flags & flags) == flags;
	}

	public bool HasAnyPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags flags)
	{
		return (UniqueData.Flags & flags) != 0;
	}
}
