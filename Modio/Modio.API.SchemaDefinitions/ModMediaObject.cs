using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModMediaObject
{
	internal readonly string[] Youtube;

	internal readonly string[] Sketchfab;

	internal readonly ImageObject[] Images;

	[JsonConstructor]
	public ModMediaObject(string[] youtube, string[] sketchfab, ImageObject[] images)
	{
		Youtube = youtube;
		Sketchfab = sketchfab;
		Images = images;
	}
}
