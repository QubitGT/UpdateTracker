using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct IconObject
{
	internal readonly string Filename;

	internal readonly string Original;

	internal readonly string Thumb64X64;

	internal readonly string Thumb128X128;

	internal readonly string Thumb256X256;

	[JsonConstructor]
	public IconObject(string filename, string original, string thumb_64x64, string thumb_128x128, string thumb_256x256)
	{
		Filename = filename;
		Original = original;
		Thumb64X64 = thumb_64x64;
		Thumb128X128 = thumb_128x128;
		Thumb256X256 = thumb_256x256;
	}
}
