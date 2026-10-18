using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct SteamAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Appdata;

	internal readonly bool TermsAgreed;

	internal readonly string? Email;

	internal readonly long DateExpires;

	[JsonConstructor]
	public SteamAuthenticationRequest(string appdata, bool termsAgreed, string? email, long dateExpires)
	{
		Appdata = appdata;
		TermsAgreed = termsAgreed;
		Email = email;
		DateExpires = dateExpires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("appdata", Appdata);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		if (!string.IsNullOrEmpty(Email))
		{
			_bodyParameters.Add("email", Email);
		}
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
