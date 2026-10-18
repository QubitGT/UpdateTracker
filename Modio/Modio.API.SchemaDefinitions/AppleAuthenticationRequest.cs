using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AppleAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string IdToken;

	internal readonly bool TermsAgreed;

	internal readonly long DateExpires;

	[JsonConstructor]
	public AppleAuthenticationRequest(string id_token, bool terms_agreed, long date_expires)
	{
		IdToken = id_token;
		TermsAgreed = terms_agreed;
		DateExpires = date_expires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("id_token", IdToken);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
