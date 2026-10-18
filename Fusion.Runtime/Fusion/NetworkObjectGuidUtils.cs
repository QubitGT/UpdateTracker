namespace Fusion;

internal class NetworkObjectGuidUtils
{
	public unsafe static void MangleGuidBytes(byte* bytes)
	{
		byte b = *bytes;
		*bytes = bytes[3];
		bytes[3] = b;
		b = bytes[1];
		bytes[1] = bytes[2];
		bytes[2] = b;
		b = bytes[4];
		bytes[4] = bytes[5];
		bytes[5] = b;
		b = bytes[6];
		bytes[6] = bytes[7];
		bytes[7] = b;
	}

	public unsafe static void CopyAndMangleGuid(byte* src, byte* dst)
	{
		*dst = src[3];
		dst[1] = src[2];
		dst[2] = src[1];
		dst[3] = *src;
		dst[4] = src[5];
		dst[5] = src[4];
		dst[6] = src[7];
		dst[7] = src[6];
		dst[8] = src[8];
		dst[9] = src[9];
		dst[10] = src[10];
		dst[11] = src[11];
		dst[12] = src[12];
		dst[13] = src[13];
		dst[14] = src[14];
		dst[15] = src[15];
	}
}
