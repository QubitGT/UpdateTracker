using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.LipSync;

public interface ILipsyncAnimator
{
	void OnVisemeStarted(Viseme viseme);

	void OnVisemeFinished(Viseme viseme);

	void OnVisemeLerp(Viseme oldVieseme, Viseme newViseme, float percentage);
}
