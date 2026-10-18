using System;
using System.Threading.Tasks;

namespace Modio.Customizations;

public struct ExternalAuthenticationToken
{
	public string url;

	public string autoUrl;

	public string code;

	public Task<(Error, WssLoginSuccess)> task;

	public DateTime expiryTime;

	internal Action cancel { get; set; }

	public void Cancel()
	{
		cancel?.Invoke();
		cancel = null;
	}
}
