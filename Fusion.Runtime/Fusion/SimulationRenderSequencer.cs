namespace Fusion;

internal struct SimulationRenderSequencer
{
	private ulong _sequence;

	public bool ConsumeRenderUpdate(NetworkRunner runner)
	{
		return ConsumeRenderUpdate(runner._simulation);
	}

	public bool ConsumeRenderUpdate(Simulation simulation)
	{
		bool flag = simulation.InterpolateSequence != _sequence;
		if (flag)
		{
			_sequence = simulation.InterpolateSequence;
		}
		return flag;
	}
}
