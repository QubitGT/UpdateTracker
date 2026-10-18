using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct DeleteModMetadataRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string[] Metadata;

	[JsonConstructor]
	public DeleteModMetadataRequest(string[] metadata)
	{
		Metadata = metadata;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("metadata", Metadata);
		return _bodyParameters;
	}
}
