using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct MonetizationTeamAccountsObject
{
	internal readonly long Id;

	internal readonly string NameId;

	internal readonly string Username;

	internal readonly long MonetizationStatus;

	internal readonly long MonetizationOptions;

	internal readonly long Split;

	[JsonConstructor]
	public MonetizationTeamAccountsObject(long id, string name_id, string username, long monetization_status, long monetization_options, long split)
	{
		Id = id;
		NameId = name_id;
		Username = username;
		MonetizationStatus = monetization_status;
		MonetizationOptions = monetization_options;
		Split = split;
	}
}
