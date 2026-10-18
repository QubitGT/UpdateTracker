#define DEBUG
namespace Fusion;

internal struct SimulationPacketEnvelope
{
	private const int MIN_CAPACITY = 64;

	public Tick Tick;

	public SimulationMessageList Messages;

	public unsafe NetworkObjectPacketData* ObjectData;

	public int ObjectDataCount;

	public int ObjectDataCapacity;

	public unsafe void AddObjectPacketData(Simulation sim, NetworkId id, Tick tick, NetworkObjectPacketFlags flags)
	{
		Assert.Check(ObjectDataCapacity >= 64);
		if (ObjectDataCount == ObjectDataCapacity)
		{
			ObjectData = sim.TempDoubleArray(ref ObjectData, ObjectDataCapacity);
			ObjectDataCapacity *= 2;
		}
		ObjectData[ObjectDataCount].Id = id;
		ObjectData[ObjectDataCount].ResetTick = tick;
		ObjectData[ObjectDataCount].Flags = flags;
		ObjectDataCount++;
		Assert.Check(ObjectDataCount <= ObjectDataCapacity);
	}

	internal unsafe static void Free(Simulation sim, ref SimulationPacketEnvelope* envelope)
	{
		if (envelope != null)
		{
			Assert.Check(envelope->Messages.Count == 0);
			Assert.Check(envelope->Messages.Head == null);
			Assert.Check(envelope->Messages.Tail == null);
			if (envelope->ObjectData != null)
			{
				sim.TempFree(ref envelope->ObjectData);
			}
			sim.TempFree(ref envelope);
		}
	}

	internal unsafe static SimulationPacketEnvelope* Alloc(Simulation sim)
	{
		SimulationPacketEnvelope* ptr = sim.TempAlloc<SimulationPacketEnvelope>();
		ptr->ObjectData = sim.TempAllocArray<NetworkObjectPacketData>(64);
		ptr->ObjectDataCount = 0;
		ptr->ObjectDataCapacity = 64;
		return ptr;
	}
}
