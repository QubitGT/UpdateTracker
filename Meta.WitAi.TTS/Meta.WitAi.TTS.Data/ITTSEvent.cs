namespace Meta.WitAi.TTS.Data;

public interface ITTSEvent
{
	string EventType { get; }

	int SampleOffset { get; }
}
