using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ToggleModRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string MarketplaceEffect;

	internal readonly string LimitedEffect;

	internal readonly string Code;

	[JsonConstructor]
	public ToggleModRequest(string marketplace_effect, string limited_effect, string code)
	{
		MarketplaceEffect = marketplace_effect;
		LimitedEffect = limited_effect;
		Code = code;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("marketplace_effect", MarketplaceEffect);
		_bodyParameters.Add("limited_effect", LimitedEffect);
		_bodyParameters.Add("code", Code);
		return _bodyParameters;
	}
}
