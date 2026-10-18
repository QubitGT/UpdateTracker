using System;

namespace Unity.Cinemachine;

[Flags]
public enum OutputChannels
{
	Default = 1,
	Channel01 = 2,
	Channel02 = 4,
	Channel03 = 8,
	Channel04 = 0x10,
	Channel05 = 0x20,
	Channel06 = 0x40,
	Channel07 = 0x80,
	Channel08 = 0x100,
	Channel09 = 0x200,
	Channel10 = 0x400,
	Channel11 = 0x800,
	Channel12 = 0x1000,
	Channel13 = 0x2000,
	Channel14 = 0x4000,
	Channel15 = 0x8000
}
