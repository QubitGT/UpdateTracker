namespace Fusion;

internal struct ServerTimeProviderSettings
{
	public double SimDeltaTime;

	public static ServerTimeProviderSettings Default()
	{
		TickRate.Resolved resolved = TickRate.Resolve(TickRate.Default);
		ServerTimeProviderSettings result = default(ServerTimeProviderSettings);
		result.SimDeltaTime = resolved.ClientTickDelta;
		return result;
	}
}
