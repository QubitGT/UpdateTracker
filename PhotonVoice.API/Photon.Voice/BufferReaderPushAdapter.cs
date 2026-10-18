namespace Photon.Voice;

public class BufferReaderPushAdapter<T> : BufferReaderPushAdapterBase<T>
{
	protected T[] buffer;

	public BufferReaderPushAdapter(LocalVoice localVoice, IDataReader<T> reader)
		: base(reader)
	{
		buffer = new T[((LocalVoiceFramed<T>)localVoice).FrameSize];
	}

	public override void Service(LocalVoice localVoice)
	{
		while (reader.Read(buffer))
		{
			((LocalVoiceFramed<T>)localVoice).PushData(buffer);
		}
	}
}
