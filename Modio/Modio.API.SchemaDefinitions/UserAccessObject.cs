using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UserAccessObject
{
	internal readonly string ResourceType;

	internal readonly long ResourceId;

	internal readonly long ResourceName;

	internal readonly string ResourceNameId;

	internal readonly string ResourceUrl;

	[JsonConstructor]
	public UserAccessObject(string resource_type, long resource_id, long resource_name, string resource_name_id, string resource_url)
	{
		ResourceType = resource_type;
		ResourceId = resource_id;
		ResourceName = resource_name;
		ResourceNameId = resource_name_id;
		ResourceUrl = resource_url;
	}
}
