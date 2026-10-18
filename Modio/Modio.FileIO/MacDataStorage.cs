using System.Runtime.InteropServices;

namespace Modio.FileIO;

public class MacDataStorage : BaseDataStorage
{
	private struct UnixStatsFs
	{
		public ulong f_bsize;

		public ulong f_frsize;

		public ulong f_blocks;

		public ulong f_bfree;

		public ulong f_bavail;

		public ulong f_files;

		public ulong f_ffre;

		public ulong f_favail;

		public ulong f_fsid;

		public ulong f_flag;

		public ulong f_namemax;
	}

	protected override long GetAvailableFreeSpace()
	{
		return 0L;
	}

	[DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
	private static extern short statvfs(string directory, out UnixStatsFs statsFs);
}
