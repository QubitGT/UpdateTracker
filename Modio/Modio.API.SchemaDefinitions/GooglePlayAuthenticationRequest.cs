using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GooglePlayAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string AuthCode;

	internal readonly bool TermsAgreed;

	internal readonly string Email;

	internal readonly long DateExpires;

	[JsonConstructor]
	public GooglePlayAuthenticationRequest(string auth_code, bool terms_agreed, string email, long date_expires)
	{
		AuthCode = auth_code;
		TermsAgreed = terms_agreed;
		Email = email;
		DateExpires = date_expires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("auth_code", AuthCode);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		_bodyParameters.Add("email", Email);
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
