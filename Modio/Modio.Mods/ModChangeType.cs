using System;

namespace Modio.Mods;

[Flags]
public enum ModChangeType
{
	Modfile = 1,
	IsEnabled = 2,
	IsSubscribed = 4,
	ModObject = 8,
	DownloadProgress = 0x10,
	FileState = 0x20,
	Rating = 0x40,
	IsPurchased = 0x80,
	Generic = 0x100,
	Dependencies = 0x200,
	Everything = -1
}
