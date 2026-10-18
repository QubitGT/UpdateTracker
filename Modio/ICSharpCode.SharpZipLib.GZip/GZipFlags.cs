using System;

namespace ICSharpCode.SharpZipLib.GZip;

[Flags]
public enum GZipFlags : byte
{
	FTEXT = 1,
	FHCRC = 2,
	FEXTRA = 4,
	FNAME = 8,
	FCOMMENT = 0x10
}
