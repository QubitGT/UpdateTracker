using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddModSubscriptionRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly bool IncludeDependencies;

	[JsonConstructor]
	public AddModSubscriptionRequest(bool include_dependencies)
	{
		IncludeDependencies = include_dependencies;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("include_dependencies", IncludeDependencies);
		return _bodyParameters;
	}
}
