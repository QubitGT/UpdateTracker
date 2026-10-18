using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GuideTagObject
{
	internal readonly string Name;

	internal readonly long DateAdded;

	internal readonly long Count;

	[JsonConstructor]
	public GuideTagObject(string name, long date_added, long count)
	{
		Name = name;
		DateAdded = date_added;
		Count = count;
	}
}
