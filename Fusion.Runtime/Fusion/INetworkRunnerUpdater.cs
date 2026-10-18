namespace Fusion;

public interface INetworkRunnerUpdater
{
	void Initialize(NetworkRunner runner);

	void Shutdown(NetworkRunner runner);
}
