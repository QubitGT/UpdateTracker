namespace Meta.WitAi.Interfaces;

public interface IDataUploadHandler
{
	void Write(byte[] buffer, int offset, int length);
}
