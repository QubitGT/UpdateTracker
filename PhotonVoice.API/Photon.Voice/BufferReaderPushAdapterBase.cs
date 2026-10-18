namespace Photon.Voice;

public abstract class BufferReaderPushAdapterBase<T> : IServiceable
{
	protected IDataReader<T> reader;

	public abstract void Service(LocalVoice localVoice);

	public BufferReaderPushAdapterBase(IDataReader<T> reader)
	{
		this.reader = reader;
	}

	public void Dispose()
	{
		reader.Dispose();
	}
}
