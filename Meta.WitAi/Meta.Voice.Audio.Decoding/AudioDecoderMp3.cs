using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
public class AudioDecoderMp3 : IAudioDecoder
{
	private readonly AudioDecoderMp3Frame _frame = new AudioDecoderMp3Frame();

	public bool WillDecodeInBackground => true;

	public void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		while (bufferLength > 0)
		{
			int num = _frame.Decode(buffer, bufferOffset, bufferLength, onSamplesDecoded);
			bufferOffset += num;
			bufferLength -= num;
		}
	}
}
