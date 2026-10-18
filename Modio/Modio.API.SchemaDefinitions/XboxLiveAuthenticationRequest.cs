using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct XboxLiveAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	public readonly string XboxToken;

	public readonly bool TermsAgreed;

	public readonly string Email;

	public readonly long DateExpires;

	[JsonConstructor]
	public XboxLiveAuthenticationRequest(string xbox_token, bool terms_agreed, string email, long date_expires)
	{
		XboxToken = xbox_token;
		TermsAgreed = terms_agreed;
		Email = email;
		DateExpires = date_expires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("xbox_token", XboxToken);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		_bodyParameters.Add("email", Email);
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
