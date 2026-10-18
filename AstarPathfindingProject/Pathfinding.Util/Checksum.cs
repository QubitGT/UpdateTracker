namespace Pathfinding.Util;

public class Checksum
{
	public static uint GetChecksum(byte[] arr, uint hash)
	{
		hash ^= 0x811C9DC5u;
		for (int i = 0; i < arr.Length; i++)
		{
			hash = (hash ^ arr[i]) * 16777619;
		}
		return hash;
	}
}
