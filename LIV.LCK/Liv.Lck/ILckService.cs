using System;
using System.Threading.Tasks;
using Liv.Lck.Recorder;
using UnityEngine;

namespace Liv.Lck;

public interface ILckService : IDisposable
{
	event Action<LckResult> OnRecordingStarted;

	event Action<LckResult> OnRecordingPaused;

	event Action<LckResult> OnRecordingResumed;

	event Action<LckResult> OnRecordingStopped;

	event Action<LckResult> OnStreamingStarted;

	event Action<LckResult> OnStreamingStopped;

	event Action<LckResult> OnLowStorageSpace;

	event Action<LckResult<RecordingData>> OnRecordingSaved;

	event Action<LckResult> OnPhotoSaved;

	event Action<LckResult<ILckCamera>> OnActiveCameraSet;

	event Action<LckResult<RecordingData>> OnEchoSaved;

	event Action<LckResult> OnEchoEnabled;

	event Action<LckResult, EchoDisableReason> OnEchoDisabled;

	LckResult StartRecording();

	LckResult PauseRecording();

	LckResult<bool> IsPaused();

	LckResult ResumeRecording();

	LckResult StopRecording();

	LckResult StartStreaming();

	LckResult StopStreaming();

	LckResult<TimeSpan> GetRecordingDuration();

	LckResult<TimeSpan> GetStreamDuration();

	LckResult SetTrackFramerate(uint framerate);

	LckResult SetTrackDescriptor(CameraTrackDescriptor cameraTrackDescriptor);

	LckResult SetTrackResolution(CameraResolutionDescriptor cameraResolutionDescriptor);

	LckResult SetTrackBitrate(uint bitrate);

	LckResult SetTrackAudioBitrate(uint audioBitrate);

	LckResult SetCameraOrientation(LckCameraOrientation orientation);

	LckResult SetTrackDescriptor(LckCaptureType captureType, CameraTrackDescriptor cameraTrackDescriptor);

	LckResult<LckCaptureType> GetActiveCaptureType();

	LckResult SetActiveCaptureType(LckCaptureType captureType);

	LckResult SetPreviewActive(bool isActive);

	LckResult<bool> IsRecording();

	LckResult<bool> IsStreaming();

	LckResult<bool> IsCapturing();

	LckResult SetGameAudioCaptureActive(bool isActive);

	LckResult SetMicrophoneCaptureActive(bool isActive);

	LckResult<float> GetMicrophoneOutputLevel();

	LckResult SetMicrophoneGain(float gain);

	LckResult SetGameAudioGain(float gain);

	LckResult<float> GetGameOutputLevel();

	LckResult<bool> IsGameAudioMute();

	LckResult SetActiveCamera(string cameraId, string monitorId = null);

	LckResult<ILckCamera> GetActiveCamera();

	LckResult PreloadDiscreetAudio(AudioClip audioClip, float volume, bool forceReload = false);

	LckResult PlayDiscreetAudioClip(AudioClip audioClip);

	LckResult StopAllDiscreetAudio();

	LckResult SetEchoEnabled(bool enabled);

	Task<LckResult> SetEchoEnabledAsync(bool enabled);

	LckResult<bool> IsEchoEnabled();

	LckResult TriggerEchoSave();

	LckResult<TimeSpan> GetEchoBufferDuration();

	LckResult<TimeSpan> GetEchoMaxBufferDuration();

	LckResult<LckDescriptor> GetDescriptor();

	LckResult CapturePhoto();
}
