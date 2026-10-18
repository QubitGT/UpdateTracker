using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct EpicGamesAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string AccessToken;

	internal readonly bool TermsAgreed;

	internal readonly string Email;

	internal readonly long DateExpires;

	[JsonConstructor]
	public EpicGamesAuthenticationRequest(string access_token, bool terms_agreed, string email, long date_expires)
	{
		AccessToken = access_token;
		TermsAgreed = terms_agreed;
		Email = email;
		DateExpires = date_expires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("access_token", AccessToken);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		_bodyParameters.Add("email", Email);
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
