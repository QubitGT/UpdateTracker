using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct MetaQuestAuthenticationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Device;

	internal readonly string Nonce;

	internal readonly string UserId;

	internal readonly string AccessToken;

	internal readonly bool TermsAgreed;

	internal readonly string? Email;

	internal readonly long DateExpires;

	[JsonConstructor]
	public MetaQuestAuthenticationRequest(string device, string nonce, string userId, string accessToken, bool termsAgreed, string? email, long dateExpires)
	{
		Device = device;
		Nonce = nonce;
		UserId = userId;
		AccessToken = accessToken;
		TermsAgreed = termsAgreed;
		Email = email;
		DateExpires = dateExpires;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("device", Device);
		_bodyParameters.Add("nonce", Nonce);
		_bodyParameters.Add("user_id", UserId);
		_bodyParameters.Add("access_token", AccessToken);
		_bodyParameters.Add("terms_agreed", TermsAgreed);
		if (!string.IsNullOrEmpty(Email))
		{
			_bodyParameters.Add("email", Email);
		}
		_bodyParameters.Add("date_expires", DateExpires);
		return _bodyParameters;
	}
}
