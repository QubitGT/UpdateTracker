using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AdminEditModPriceRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long Price;

	[JsonConstructor]
	public AdminEditModPriceRequest(long price)
	{
		Price = price;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("price", Price);
		return _bodyParameters;
	}
}
