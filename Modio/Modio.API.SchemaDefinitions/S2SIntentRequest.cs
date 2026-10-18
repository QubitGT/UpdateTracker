using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct S2SIntentRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Sku;

	internal readonly string Portal;

	internal readonly string GatewayUuid;

	[JsonConstructor]
	public S2SIntentRequest(string sku, string portal, string gateway_uuid)
	{
		Sku = sku;
		Portal = portal;
		GatewayUuid = gateway_uuid;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("sku", Sku);
		_bodyParameters.Add("portal", Portal);
		_bodyParameters.Add("gateway_uuid", GatewayUuid);
		return _bodyParameters;
	}
}
