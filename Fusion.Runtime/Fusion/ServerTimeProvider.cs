using Fusion.Statistics;

namespace Fusion;

internal class ServerTimeProvider : ITimeProvider
{
	private ServerTimeProviderSettings _settings;

	private double _time;

	internal ServerTimeProvider()
	{
		_settings = ServerTimeProviderSettings.Default();
	}

	internal ServerTimeProvider(ServerTimeProviderSettings settings)
	{
		_settings = settings;
	}

	private void Reset(Tick snapshot)
	{
		_time = (double)(int)snapshot * _settings.SimDeltaTime;
	}

	private void Update(double unscaledDeltaTime)
	{
		_time += unscaledDeltaTime;
	}

	bool ITimeProvider.IsRunning()
	{
		return true;
	}

	void ITimeProvider.Configure(SimulationRuntimeConfig src)
	{
		_settings.SimDeltaTime = src.TickRate.ClientTickDelta;
	}

	void ITimeProvider.Configure(TimeSyncConfiguration tsc)
	{
	}

	void ITimeProvider.Reset(double roundTripTime, Tick snapshot)
	{
		Reset(snapshot);
	}

	void ITimeProvider.Snap()
	{
	}

	void ITimeProvider.Update(double unscaledDeltaTime)
	{
		Update(unscaledDeltaTime);
	}

	void ITimeProvider.OnSnapshotReceived(double roundTripTime, Tick snapshot)
	{
	}

	void ITimeProvider.OnFeedbackReceived(Simulation.TimeFeedback feedback)
	{
	}

	void ITimeProvider.ResetFeedback()
	{
	}

	Instant ITimeProvider.Now()
	{
		Instant result = default(Instant);
		result.Input = _time;
		result.Local = _time;
		result.Remote = _time;
		return result;
	}

	void ITimeProvider.Log(FusionStatisticsManager stats)
	{
	}

	void ITimeProvider.SetPlayerIndex(int index)
	{
	}

	void ITimeProvider.StartTrace()
	{
	}

	void ITimeProvider.StopTrace()
	{
	}
}
