using System.Text;

namespace ICSharpCode.SharpZipLib.GZip;

public sealed class GZipConstants
{
	public const byte ID1 = 31;

	public const byte ID2 = 139;

	public const byte CompressionMethodDeflate = 8;

	public static Encoding Encoding
	{
		get
		{
			try
			{
				return Encoding.GetEncoding(1252);
			}
			catch
			{
				return Encoding.ASCII;
			}
		}
	}
}
