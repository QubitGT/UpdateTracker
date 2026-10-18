using System;

namespace CSCore;

[Flags]
public enum ChannelMask
{
	SpeakerFrontLeft = 1,
	SpeakerFrontRight = 2,
	SpeakerFrontCenter = 4,
	SpeakerLowFrequency = 8,
	SpeakerBackLeft = 0x10,
	SpeakerBackRight = 0x20,
	SpeakerFrontLeftOfCenter = 0x40,
	SpeakerFrontRightOfCenter = 0x80,
	SpeakerBackCenter = 0x100,
	SpeakerSideLeft = 0x200,
	SpeakerSideRight = 0x400,
	SpeakerTopCenter = 0x800,
	SpeakerTopFrontLeft = 0x1000,
	SpeakerTopFrontCenter = 0x2000,
	SpeakerTopFrontRight = 0x4000,
	SpeakerTopBackLeft = 0x8000,
	SpeakerTopBackCenter = 0x10000,
	SpeakerTopBackRight = 0x20000
}
