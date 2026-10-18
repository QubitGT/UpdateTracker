using Meta.WitAi.TTS.Data;

namespace Meta.WitAi.TTS.LipSync;

public interface IVisemeAnimatorProvider
{
	Viseme LastViseme { get; }

	VisemeChangedEvent OnVisemeStarted { get; }

	VisemeChangedEvent OnVisemeFinished { get; }

	VisemeLerpEvent OnVisemeLerp { get; }
}
