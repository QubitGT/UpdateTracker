using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddGameMediaRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string[] RedirectUris;

	[JsonConstructor]
	public AddGameMediaRequest(string[] redirect_uris)
	{
		RedirectUris = redirect_uris;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("redirect_uris", RedirectUris);
		return _bodyParameters;
	}
}
