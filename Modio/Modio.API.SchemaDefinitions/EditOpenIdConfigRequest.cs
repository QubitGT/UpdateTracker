using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct EditOpenIdConfigRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string JwkUrl;

	internal readonly string Aud;

	internal readonly string DisplayNameClaim;

	internal readonly string AvatarUrlClaim;

	[JsonConstructor]
	public EditOpenIdConfigRequest(string jwk_url, string aud, string display_name_claim, string avatar_url_claim)
	{
		JwkUrl = jwk_url;
		Aud = aud;
		DisplayNameClaim = display_name_claim;
		AvatarUrlClaim = avatar_url_claim;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("jwk_url", JwkUrl);
		_bodyParameters.Add("aud", Aud);
		_bodyParameters.Add("display_name_claim", DisplayNameClaim);
		_bodyParameters.Add("avatar_url_claim", AvatarUrlClaim);
		return _bodyParameters;
	}
}
