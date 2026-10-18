using System;

namespace Oculus.Interaction;

public interface ITimeConsumer
{
	void SetTimeProvider(Func<float> timeProvider);
}
