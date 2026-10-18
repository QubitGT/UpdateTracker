using System;

namespace Liv.Lck.ErrorHandling;

internal interface ILckCaptureErrorDispatcher : IDisposable
{
	void PushError(LckCaptureError error);
}
