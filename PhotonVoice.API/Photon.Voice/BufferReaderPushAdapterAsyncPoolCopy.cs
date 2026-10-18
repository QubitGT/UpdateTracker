using System;

namespace Photon.Voice;

public class BufferReaderPushAdapterAsyncPoolCopy<T> : BufferReaderPushAdapterBase<T>
{
	protected T[] buffer;

	public BufferReaderPushAdapterAsyncPoolCopy(LocalVoice localVoice, IDataReader<T> reader)
		: base(reader)
	{
		buffer = new T[((LocalVoiceFramedBase)localVoice).FrameSize];
	}

	public override void Service(LocalVoice localVoice)
	{
		while (reader.Read(buffer))
		{
			LocalVoiceFramed<T> obj = (LocalVoiceFramed<T>)localVoice;
			T[] array = obj.BufferFactory.New();
			Array.Copy(buffer, array, buffer.Length);
			obj.PushDataAsync(array);
		}
	}
}
