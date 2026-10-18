using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct TeamMemberObject
{
	internal readonly long Id;

	internal readonly UserObject User;

	internal readonly long Level;

	internal readonly long DateAdded;

	internal readonly string Position;

	internal readonly long InvitePending;

	[JsonConstructor]
	public TeamMemberObject(long id, UserObject user, long level, long date_added, string position, long invite_pending)
	{
		Id = id;
		User = user;
		Level = level;
		DateAdded = date_added;
		Position = position;
		InvitePending = invite_pending;
	}
}
