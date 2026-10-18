namespace Meta.WitAi.Events;

public interface IWitByteDataSentHandler
{
	void OnWitDataSent(byte[] data, int offset, int length);
}
