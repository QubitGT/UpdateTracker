namespace UnityEngine.Rendering.Universal;

internal static class TileSizeExtensions
{
	public static bool IsValid(this TileSize tileSize)
	{
		if (tileSize != TileSize._8 && tileSize != TileSize._16 && tileSize != TileSize._32)
		{
			return tileSize == TileSize._64;
		}
		return true;
	}
}
