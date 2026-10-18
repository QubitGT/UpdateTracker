namespace Meta.Voice.Audio.Decoding;

public interface IAudioDecoder
{
	bool WillDecodeInBackground { get; }

	void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded);
}
