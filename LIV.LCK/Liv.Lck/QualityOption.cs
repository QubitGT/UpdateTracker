using System;
using UnityEngine.Serialization;

namespace Liv.Lck;

[Serializable]
public struct QualityOption
{
	public string Name;

	public bool IsDefault;

	[FormerlySerializedAs("CameraTrackDescriptor")]
	public CameraTrackDescriptor RecordingCameraTrackDescriptor;

	public CameraTrackDescriptor StreamingCameraTrackDescriptor;

	[Obsolete("Provides the RecordingCameraTrackDescriptor and only exists for backwards compability - Use RecordingCameraTrackDescriptor or StreamingCameraTrackDescriptor instead")]
	public CameraTrackDescriptor CameraTrackDescriptor => RecordingCameraTrackDescriptor;

	public QualityOption(string name, bool isDefault, CameraTrackDescriptor recordingCameraTrackDescriptor, CameraTrackDescriptor streamingCameraTrackDescriptor)
	{
		Name = name;
		IsDefault = isDefault;
		RecordingCameraTrackDescriptor = recordingCameraTrackDescriptor;
		StreamingCameraTrackDescriptor = streamingCameraTrackDescriptor;
	}
}
