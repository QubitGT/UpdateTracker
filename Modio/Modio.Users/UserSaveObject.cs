using System;
using System.Collections.Generic;

namespace Modio.Users;

[Serializable]
public class UserSaveObject
{
	public string LocalUserId;

	public string Username;

	public long UserId;

	public string AuthToken;

	public long AuthExpiration;

	public List<long> SubscribedMods;

	public List<long> DisabledMods;

	public List<long> PurchasedMods;
}
