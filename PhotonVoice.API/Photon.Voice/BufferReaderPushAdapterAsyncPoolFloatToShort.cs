namespace Photon.Voice;

public class BufferReaderPushAdapterAsyncPoolFloatToShort : BufferReaderPushAdapterBase<float>
{
	private float[] buffer;

	public BufferReaderPushAdapterAsyncPoolFloatToShort(LocalVoice localVoice, IDataReader<float> reader)
		: base(reader)
	{
		buffer = new float[((LocalVoiceFramed<short>)localVoice).FrameSize];
	}

	public override void Service(LocalVoice localVoice)
	{
		LocalVoiceFramed<short> localVoiceFramed = (LocalVoiceFramed<short>)localVoice;
		short[] array = localVoiceFramed.BufferFactory.New();
		while (reader.Read(buffer))
		{
			AudioUtil.Convert(buffer, array, array.Length);
			localVoiceFramed.PushDataAsync(array);
			array = localVoiceFramed.BufferFactory.New();
		}
		localVoiceFramed.BufferFactory.Free(array, array.Length);
	}
}
