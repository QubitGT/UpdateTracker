using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct TestOpenIdConfigRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string JwkUrl;

	internal readonly string IdToken;

	[JsonConstructor]
	public TestOpenIdConfigRequest(string jwk_url, string id_token)
	{
		JwkUrl = jwk_url;
		IdToken = id_token;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("jwk_url", JwkUrl);
		_bodyParameters.Add("id_token", IdToken);
		return _bodyParameters;
	}
}
