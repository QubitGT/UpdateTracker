using System.Collections.Generic;
using Meta.Voice.Net.Encoding.Wit;
using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
public class AudioDecoderJson : IAudioDecoder
{
	private readonly WitChunkConverter _chunkDecoder = new WitChunkConverter();

	private readonly List<WitResponseNode> _decodedJson = new List<WitResponseNode>();

	private readonly AudioJsonDecodeDelegate _onJsonDecoded;

	private readonly IAudioDecoder _audioDecoder;

	private AudioSampleDecodeDelegate _onSamplesDecoded;

	public bool WillDecodeInBackground => true;

	public AudioDecoderJson(IAudioDecoder audioDecoder, AudioJsonDecodeDelegate onJsonDecoded)
	{
		_audioDecoder = audioDecoder;
		_onJsonDecoded = onJsonDecoded;
	}

	public void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		_onSamplesDecoded = onSamplesDecoded;
		_chunkDecoder.Decode(buffer, bufferOffset, bufferLength, DecodeJson, DecodeAudio);
		_onSamplesDecoded = null;
		if (_decodedJson.Count != 0)
		{
			_onJsonDecoded?.Invoke(_decodedJson);
			_decodedJson.Clear();
		}
	}

	private void DecodeJson(WitChunk chunk)
	{
		if (chunk.jsonData is WitResponseArray witResponseArray)
		{
			_decodedJson.AddRange(witResponseArray.Childs);
		}
		else if (chunk.jsonData != null)
		{
			_decodedJson.Add(chunk.jsonData);
		}
	}

	private void DecodeAudio(byte[] buffer, int bufferOffset, int bufferLength)
	{
		_audioDecoder.Decode(buffer, bufferOffset, bufferLength, _onSamplesDecoded);
	}
}
