using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct SyncAppleEntitlementsRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Receipt;

	[JsonConstructor]
	public SyncAppleEntitlementsRequest(string receipt)
	{
		Receipt = receipt;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("receipt", Receipt);
		return _bodyParameters;
	}
}
