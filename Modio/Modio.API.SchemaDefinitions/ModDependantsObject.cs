using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModDependantsObject
{
	internal readonly long ModId;

	internal readonly string Name;

	internal readonly string NameId;

	internal readonly long Status;

	internal readonly long Visible;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly LogoObject Logo;

	[JsonConstructor]
	public ModDependantsObject(long mod_id, string name, string name_id, long status, long visible, long date_added, long date_updated, LogoObject logo)
	{
		ModId = mod_id;
		Name = name;
		NameId = name_id;
		Status = status;
		Visible = visible;
		DateAdded = date_added;
		DateUpdated = date_updated;
		Logo = logo;
	}
}
