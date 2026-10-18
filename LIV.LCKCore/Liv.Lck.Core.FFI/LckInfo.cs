using System;

namespace Liv.Lck.Core.FFI;

internal struct LckInfo
{
	public IntPtr Version;

	public int BuildNumber;

	public static LckInfo AllocateFromLckInfo(Liv.Lck.Core.LckInfo lckInfo)
	{
		return new LckInfo(lckInfo);
	}

	public void Free()
	{
		InteropUtilities.Free(Version);
	}

	private LckInfo(Liv.Lck.Core.LckInfo lckInfo)
	{
		Version = InteropUtilities.StringToUTF8Pointer(lckInfo.Version);
		BuildNumber = lckInfo.BuildNumber;
	}
}
