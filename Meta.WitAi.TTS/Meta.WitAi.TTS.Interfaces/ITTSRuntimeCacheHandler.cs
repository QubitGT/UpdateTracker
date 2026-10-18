using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.Interfaces;

public interface ITTSRuntimeCacheHandler
{
	event TTSClipCallback OnClipAdded;

	event TTSClipCallback OnClipRemoved;

	TTSClipData[] GetClips();

	TTSClipData GetClip(string clipID);

	bool AddClip(TTSClipData clipData);

	void RemoveClip(string clipID);
}
