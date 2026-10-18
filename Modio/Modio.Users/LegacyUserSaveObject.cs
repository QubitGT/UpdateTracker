using System;

namespace Modio.Users;

[Serializable]
public class LegacyUserSaveObject
{
	public string oAuthToken;

	public long oAuthExpiryDate;

	public bool oAuthTokenWasRejected;

	public LegacyUserObject userObject;
}
