using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meta.Voice.Audio.Decoding;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Meta.WitAi.Requests;

internal class WitTTSVRequest : WitVRequest
{
	private IAudioDecoder _decoder;

	public string TextToSpeak { get; set; }

	public Dictionary<string, string> TtsParameters { get; set; }

	public TTSWitAudioType FileType { get; set; }

	public bool Stream { get; set; }

	public bool UseEvents { get; set; }

	public WitTTSVRequest(IWitRequestConfiguration configuration, string requestId, string operationId)
		: base(configuration, requestId, operationId)
	{
	}

	protected override Dictionary<string, string> GetHeaders()
	{
		Dictionary<string, string> headers = base.GetHeaders();
		headers["Accept"] = WitRequestSettings.GetAudioMimeType(FileType);
		return headers;
	}

	public async Task<VRequestResponse<bool>> RequestStreamFromDisk(string diskPath, AudioSampleDecodeDelegate onSamplesDecoded, AudioJsonDecodeDelegate onJsonDecoded)
	{
		base.Url = "file://" + diskPath;
		base.Method = VRequestMethod.HttpGet;
		_decoder = WitRequestSettings.GetTtsAudioDecoder(FileType, UseEvents ? onJsonDecoded : null);
		await ThreadUtility.CallOnMainThread(delegate
		{
			base.Downloader = new AudioStreamHandler(_decoder, onSamplesDecoded);
		});
		return await Request(base.DecodeSuccess);
	}

	public async Task<VRequestResponse<bool>> RequestStream(AudioSampleDecodeDelegate onSamplesDecoded, AudioJsonDecodeDelegate onJsonDecoded)
	{
		string text = await SetupTts(download: false, onSamplesDecoded, onJsonDecoded);
		if (!string.IsNullOrEmpty(text))
		{
			return new VRequestResponse<bool>(-1, text);
		}
		return await Request(base.DecodeSuccess);
	}

	public async Task<VRequestResponse<bool>> RequestDownload(string downloadPath)
	{
		string text = await SetupTts(download: true, null, null);
		if (!string.IsNullOrEmpty(text))
		{
			return new VRequestResponse<bool>(-1, text);
		}
		return await RequestFileDownload(base.Url, downloadPath);
	}

	private async Task<string> SetupTts(bool download, AudioSampleDecodeDelegate onSamplesDecoded, AudioJsonDecodeDelegate onJsonDecoded)
	{
		string webErrors = GetWebErrors(download);
		if (!string.IsNullOrEmpty(webErrors))
		{
			return webErrors;
		}
		byte[] postData = EncodePostData();
		if (postData == null)
		{
			return "Data failed to encode";
		}
		base.Url = base.Configuration.GetEndpointInfo().Synthesize;
		base.Method = VRequestMethod.HttpPost;
		base.ContentType = "application/json";
		if (!download)
		{
			_decoder = WitRequestSettings.GetTtsAudioDecoder(FileType, UseEvents ? onJsonDecoded : null);
		}
		await ThreadUtility.CallOnMainThread(delegate
		{
			base.Uploader = new UploadHandlerRaw(postData);
			if (!download)
			{
				base.Downloader = new AudioStreamHandler(_decoder, onSamplesDecoded);
			}
		});
		return string.Empty;
	}

	private string GetWebErrors(bool downloadOnly = false)
	{
		string ttsErrors = WitRequestSettings.GetTtsErrors(TextToSpeak, base.Configuration);
		if (!downloadOnly && Stream)
		{
			if (Application.platform == RuntimePlatform.WebGLPlayer)
			{
				base.Logger.Warning("Wit cannot currently stream TTS in WebGL");
				Stream = false;
			}
			else if (!WitRequestSettings.CanStreamAudio(FileType))
			{
				base.Logger.Warning("Wit cannot stream {0} files please use {1} instead.", FileType, TTSWitAudioType.MPEG);
				Stream = false;
			}
		}
		return ttsErrors;
	}

	private byte[] EncodePostData()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary["q"] = TextToSpeak;
		dictionary["viseme"] = UseEvents.ToString().ToLower();
		if (TtsParameters != null)
		{
			foreach (KeyValuePair<string, string> ttsParameter in TtsParameters)
			{
				dictionary[ttsParameter.Key] = ttsParameter.Value;
			}
		}
		string s = JsonConvert.SerializeObject(dictionary);
		return Encoding.UTF8.GetBytes(s);
	}
}
