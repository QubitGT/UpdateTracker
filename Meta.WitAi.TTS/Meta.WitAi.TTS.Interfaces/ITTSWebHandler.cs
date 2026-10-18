using System;
using System.Threading.Tasks;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.Interfaces;

public interface ITTSWebHandler
{
	string GetWebErrors(TTSClipData clipData);

	TTSClipData CreateClipData(string clipId, string textToSpeak, TTSVoiceSettings voiceSettings, TTSDiskCacheSettings diskCacheSettings);

	bool DecodeTtsFromJson(WitResponseNode responseNode, out string textToSpeak, out TTSVoiceSettings voiceSettings);

	Task<string> RequestStreamFromWeb(TTSClipData clipData, Action<TTSClipData> onReady);

	Task<string> IsDownloadedToDisk(string diskPath);

	Task<string> RequestStreamFromDisk(TTSClipData clipData, string diskPath, Action<TTSClipData> onReady);

	Task<string> RequestDownloadFromWeb(TTSClipData clipData, string diskPath);

	bool CancelRequests(TTSClipData clipData);
}
