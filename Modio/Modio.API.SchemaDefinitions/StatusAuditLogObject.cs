using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct StatusAuditLogObject
{
	internal readonly long StatusNew;

	internal readonly long StatusOld;

	internal readonly UserObject User;

	internal readonly long DateAdded;

	internal readonly string Reason;

	[JsonConstructor]
	public StatusAuditLogObject(long status_new, long status_old, UserObject user, long date_added, string reason)
	{
		StatusNew = status_new;
		StatusOld = status_old;
		User = user;
		DateAdded = date_added;
		Reason = reason;
	}
}
