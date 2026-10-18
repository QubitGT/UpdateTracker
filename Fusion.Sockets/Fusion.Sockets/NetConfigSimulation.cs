namespace Fusion.Sockets;

public struct NetConfigSimulation
{
	public unsafe short* LossNotifySequences;

	public int LossNotifySequencesLength;

	public NetConfigSimulationOscillator DelayOscillator;

	public NetConfigSimulationOscillator LossOscillator;

	public double DuplicateChance;

	public unsafe static NetConfigSimulation Defaults
	{
		get
		{
			NetConfigSimulation result = default(NetConfigSimulation);
			result.LossNotifySequences = default(short*);
			result.LossNotifySequencesLength = 0;
			result.DuplicateChance = 0.0;
			result.DelayOscillator = default(NetConfigSimulationOscillator);
			result.LossOscillator = default(NetConfigSimulationOscillator);
			return result;
		}
	}

	public unsafe static NetConfigSimulation WithLossNotifySequences(params short[] sequences)
	{
		NetConfigSimulation defaults = Defaults;
		if (sequences.Length != 0)
		{
			defaults.LossNotifySequencesLength = sequences.Length;
			defaults.LossNotifySequences = Native.MallocAndClearArray<short>(sequences.Length);
			for (int i = 0; i < sequences.Length; i++)
			{
				defaults.LossNotifySequences[i] = sequences[i];
			}
		}
		return defaults;
	}
}
