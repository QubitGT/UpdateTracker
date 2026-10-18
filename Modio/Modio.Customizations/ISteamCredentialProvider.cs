using System;

namespace Modio.Customizations;

public interface ISteamCredentialProvider
{
	void RequestEncryptedAppTicket(Action<bool, string> callback);
}
