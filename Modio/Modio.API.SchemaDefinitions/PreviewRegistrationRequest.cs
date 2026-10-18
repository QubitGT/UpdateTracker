using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct PreviewRegistrationRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Hash;

	[JsonConstructor]
	public PreviewRegistrationRequest(string hash)
	{
		Hash = hash;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("hash", Hash);
		return _bodyParameters;
	}
}
