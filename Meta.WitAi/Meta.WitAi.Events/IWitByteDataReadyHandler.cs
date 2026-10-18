namespace Meta.WitAi.Events;

public interface IWitByteDataReadyHandler
{
	void OnWitDataReady(byte[] data, int offset, int length);
}
