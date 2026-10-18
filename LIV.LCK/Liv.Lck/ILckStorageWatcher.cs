using System;

namespace Liv.Lck;

internal interface ILckStorageWatcher : IDisposable
{
	bool HasEnoughFreeStorage();

	void SetRecordingContext(CameraTrackDescriptor descriptor, Func<float> getDurationSeconds);

	void ClearRecordingContext();
}
