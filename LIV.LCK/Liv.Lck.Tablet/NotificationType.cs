using System;

namespace Liv.Lck.Tablet;

[Serializable]
public enum NotificationType
{
	VideoSaved,
	PhotoSaved,
	EnterStreamCode,
	ConfigureStream,
	InternalError,
	MissingTrackingId,
	InvalidTrackingId,
	InvalidArgument,
	UnknownStreamingError,
	ServiceUnavailable,
	RateLimiterBackoff,
	EchoInfo,
	EchoLowStorage,
	EchoError,
	HeadsetView
}
