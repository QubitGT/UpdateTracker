using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UserObject
{
	internal readonly long Id;

	internal readonly string NameId;

	internal readonly string Username;

	internal readonly string DisplayNamePortal;

	internal readonly long DateOnline;

	internal readonly long DateJoined;

	internal readonly AvatarObject Avatar;

	internal readonly string Timezone;

	internal readonly string Language;

	internal readonly string ProfileUrl;

	[JsonConstructor]
	public UserObject(long id, string name_id, string username, string display_name_portal, long date_online, long date_joined, AvatarObject avatar, string timezone, string language, string profile_url)
	{
		Id = id;
		NameId = name_id;
		Username = username;
		DisplayNamePortal = display_name_portal;
		DateOnline = date_online;
		DateJoined = date_joined;
		Avatar = avatar;
		Timezone = timezone;
		Language = language;
		ProfileUrl = profile_url;
	}
}
