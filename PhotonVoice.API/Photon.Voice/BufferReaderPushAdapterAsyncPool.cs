namespace Photon.Voice;

public class BufferReaderPushAdapterAsyncPool<T> : BufferReaderPushAdapterBase<T>
{
	public BufferReaderPushAdapterAsyncPool(LocalVoice localVoice, IDataReader<T> reader)
		: base(reader)
	{
	}

	public override void Service(LocalVoice localVoice)
	{
		LocalVoiceFramed<T> localVoiceFramed = (LocalVoiceFramed<T>)localVoice;
		T[] array = localVoiceFramed.BufferFactory.New();
		while (reader.Read(array))
		{
			localVoiceFramed.PushDataAsync(array);
			array = localVoiceFramed.BufferFactory.New();
		}
		localVoiceFramed.BufferFactory.Free(array, array.Length);
	}
}
