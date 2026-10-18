namespace ICSharpCode.SharpZipLib.Zip;

public static class ZipEntryExtensions
{
	public static bool HasFlag(this ZipEntry entry, GeneralBitFlags flag)
	{
		return ((uint)entry.Flags & (uint)flag) != 0;
	}

	public static void SetFlag(this ZipEntry entry, GeneralBitFlags flag, bool enabled = true)
	{
		entry.Flags = (enabled ? (entry.Flags | (int)flag) : (entry.Flags & (int)(~flag)));
	}
}
