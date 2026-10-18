using System;

namespace Modio.Users;

[Serializable]
public class LegacyUserObject
{
	public long id;

	public string name_id;

	public string username;

	public string display_name_portal;

	public long date_online;

	public string timezone;

	public string language;

	public string profile_url;
}
