using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.Interfaces;

public interface ITTSDiskCacheHandler
{
	TTSDiskCacheSettings DiskCacheDefaultSettings { get; }

	string GetDiskCachePath(TTSClipData clipData);

	bool ShouldCacheToDisk(TTSClipData clipData);
}
