using System;
using System.Threading.Tasks;

namespace Liv.Lck.Echo;

internal interface ILckEcho : IDisposable
{
	bool IsEnabled { get; }

	bool IsSaving { get; }

	Task<LckResult> SetEnabledAsync(bool enabled);

	LckResult TriggerSave();

	TimeSpan GetBufferDuration();

	TimeSpan GetMaxBufferDuration();
}
