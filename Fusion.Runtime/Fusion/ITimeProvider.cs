using Fusion.Statistics;

namespace Fusion;

internal interface ITimeProvider
{
	bool IsRunning();

	void Configure(SimulationRuntimeConfig src);

	void Configure(TimeSyncConfiguration tsc);

	void Reset(double roundTripTime, Tick snapshot);

	void Snap();

	void Update(double unscaledDeltaTime);

	void OnSnapshotReceived(double roundTripTime, Tick snapshot);

	void OnFeedbackReceived(Simulation.TimeFeedback feedback);

	void ResetFeedback();

	Instant Now();

	void Log(FusionStatisticsManager stats);

	void SetPlayerIndex(int index);

	void StartTrace();

	void StopTrace();
}
