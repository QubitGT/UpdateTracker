using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct SyncXboxEntitlementsRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	public readonly string XboxToken;

	[JsonConstructor]
	public SyncXboxEntitlementsRequest(string xbox_token)
	{
		XboxToken = xbox_token;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("xbox_token", XboxToken);
		return _bodyParameters;
	}
}
