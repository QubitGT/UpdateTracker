using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct SyncPlayStationNetworkEntitlementsRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	public readonly string AuthCode;

	public readonly long Env;

	public readonly long ServiceLabel;

	[JsonConstructor]
	public SyncPlayStationNetworkEntitlementsRequest(string auth_code, long env, long service_label)
	{
		AuthCode = auth_code;
		Env = env;
		ServiceLabel = service_label;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("auth_code", AuthCode);
		_bodyParameters.Add("env", Env);
		_bodyParameters.Add("service_label", ServiceLabel);
		return _bodyParameters;
	}
}
