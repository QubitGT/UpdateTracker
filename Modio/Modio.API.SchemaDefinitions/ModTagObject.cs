using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModTagObject
{
	internal readonly string Name;

	internal readonly string NameLocalized;

	internal readonly long DateAdded;

	[JsonConstructor]
	public ModTagObject(string name, string name_localized, long date_added)
	{
		Name = name;
		NameLocalized = name_localized;
		DateAdded = date_added;
	}
}
