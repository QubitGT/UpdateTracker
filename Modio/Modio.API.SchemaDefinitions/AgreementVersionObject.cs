using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AgreementVersionObject
{
	internal readonly long Id;

	internal readonly bool IsActive;

	internal readonly bool IsLatest;

	internal readonly long Type;

	internal readonly UserObject User;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly long DateLive;

	internal readonly string Name;

	internal readonly string Changelog;

	internal readonly string Description;

	internal readonly JObject AdjacentVersions;

	[JsonConstructor]
	public AgreementVersionObject(long id, bool is_active, bool is_latest, long type, UserObject user, long date_added, long date_updated, long date_live, string name, string changelog, string description, JObject adjacent_versions)
	{
		Id = id;
		IsActive = is_active;
		IsLatest = is_latest;
		Type = type;
		User = user;
		DateAdded = date_added;
		DateUpdated = date_updated;
		DateLive = date_live;
		Name = name;
		Changelog = changelog;
		Description = description;
		AdjacentVersions = adjacent_versions;
	}
}
