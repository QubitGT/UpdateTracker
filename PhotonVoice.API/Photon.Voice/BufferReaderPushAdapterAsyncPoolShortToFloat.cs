namespace Photon.Voice;

public class BufferReaderPushAdapterAsyncPoolShortToFloat : BufferReaderPushAdapterBase<short>
{
	private short[] buffer;

	public BufferReaderPushAdapterAsyncPoolShortToFloat(LocalVoice localVoice, IDataReader<short> reader)
		: base(reader)
	{
		buffer = new short[((LocalVoiceFramed<float>)localVoice).FrameSize];
	}

	public override void Service(LocalVoice localVoice)
	{
		LocalVoiceFramed<float> localVoiceFramed = (LocalVoiceFramed<float>)localVoice;
		float[] array = localVoiceFramed.BufferFactory.New();
		while (reader.Read(buffer))
		{
			AudioUtil.Convert(buffer, array, array.Length);
			localVoiceFramed.PushDataAsync(array);
			array = localVoiceFramed.BufferFactory.New();
		}
		localVoiceFramed.BufferFactory.Free(array, array.Length);
	}
}
