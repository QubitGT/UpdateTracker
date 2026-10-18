using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct PsnAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	public readonly string AuthCode;

	public readonly bool TermsAgreed;

	public readonly string? Email;

	public readonly int Environment;

	internal readonly long DateExpires;

	[JsonConstructor]
	public PsnAuthenticationRequest(string auth_code, bool terms_agreed, string? email, int environment, long date_expires)
	{
		AuthCode = auth_code;
		TermsAgreed = terms_agreed;
		Email = email;
		Environment = environment;
		DateExpires = date_expires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("auth_code", AuthCode);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		if (!string.IsNullOrEmpty(Email))
		{
			_bodyParameters.Add("email", Email);
		}
		_bodyParameters.Add("env", Environment);
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
