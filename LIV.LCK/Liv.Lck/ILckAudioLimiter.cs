namespace Liv.Lck;

internal interface ILckAudioLimiter
{
	float ApplyLimiter(float audioIn, int sampleRate);
}
