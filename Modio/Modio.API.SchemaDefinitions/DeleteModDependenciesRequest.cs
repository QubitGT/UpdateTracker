using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct DeleteModDependenciesRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long[] Dependencies;

	[JsonConstructor]
	public DeleteModDependenciesRequest(long[] dependencies)
	{
		Dependencies = dependencies;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("dependencies", Dependencies);
		return _bodyParameters;
	}
}
