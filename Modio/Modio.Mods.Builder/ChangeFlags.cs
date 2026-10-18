using System;

namespace Modio.Mods.Builder;

[Flags]
public enum ChangeFlags
{
	None = 0,
	Name = 1,
	Summary = 2,
	Description = 4,
	Logo = 8,
	Gallery = 0x10,
	Tags = 0x20,
	MetadataBlob = 0x40,
	MetadataKvps = 0x80,
	Visibility = 0x100,
	MaturityOptions = 0x200,
	CommunityOptions = 0x400,
	Modfile = 0x800,
	MonetizationConfig = 0x1000,
	Dependencies = 0x2000,
	AddFlags = 0x76F,
	EditFlags = 0x176F
}
