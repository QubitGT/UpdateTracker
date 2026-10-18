using System;

namespace Oculus.Interaction;

public interface IDeltaTimeConsumer
{
	void SetDeltaTimeProvider(Func<float> deltaTimeProvider);
}
