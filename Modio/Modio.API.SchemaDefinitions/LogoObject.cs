using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct LogoObject
{
	internal readonly string Filename;

	internal readonly string Original;

	internal readonly string Thumb320X180;

	internal readonly string Thumb640X360;

	internal readonly string Thumb1280X720;

	[JsonConstructor]
	public LogoObject(string filename, string original, string thumb_320x180, string thumb_640x360, string thumb_1280x720)
	{
		Filename = filename;
		Original = original;
		Thumb320X180 = thumb_320x180;
		Thumb640X360 = thumb_640x360;
		Thumb1280X720 = thumb_1280x720;
	}
}
