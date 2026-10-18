namespace Fusion;

internal struct ClientTimeProviderSettings
{
	public double TimeScaleOffsetMax;

	public double SampleWindowSeconds;

	public double OutgoingQuantile;

	public double IncomingQuantile;

	public double OutgoingRedundancy;

	public double IncomingRedundancy;

	public int OutgoingSendRate;

	public int IncomingSendRate;

	public double OutgoingSendDelta;

	public double IncomingSendDelta;

	public double PredictionMax;

	public double InputDelayMin;

	public double InputDelayMax;

	public int ClientTickRate;

	public double ClientSimDeltaTime;

	public int ServerTickRate;

	public double ServerSimDeltaTime;

	public static ClientTimeProviderSettings Default()
	{
		TickRate.Resolved resolved = TickRate.Resolve(TickRate.Default);
		ClientTimeProviderSettings result = default(ClientTimeProviderSettings);
		result.TimeScaleOffsetMax = 0.05;
		result.SampleWindowSeconds = 1.0;
		result.OutgoingQuantile = 0.99;
		result.IncomingQuantile = 0.99;
		result.OutgoingRedundancy = 1.0;
		result.IncomingRedundancy = 1.0;
		result.OutgoingSendRate = resolved.ClientSend;
		result.IncomingSendRate = resolved.ServerSend;
		result.OutgoingSendDelta = resolved.ClientSendDelta;
		result.IncomingSendDelta = resolved.ServerSendDelta;
		result.PredictionMax = double.MaxValue;
		result.InputDelayMin = 0.0;
		result.InputDelayMax = 0.0;
		result.ClientTickRate = resolved.Client;
		result.ClientSimDeltaTime = resolved.ClientTickDelta;
		result.ServerTickRate = resolved.Server;
		result.ServerSimDeltaTime = resolved.ServerTickDelta;
		return result;
	}
}
