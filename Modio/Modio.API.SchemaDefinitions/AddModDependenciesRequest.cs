using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddModDependenciesRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long[] Dependencies;

	internal readonly bool Sync;

	[JsonConstructor]
	public AddModDependenciesRequest(long[] dependencies, bool sync)
	{
		Dependencies = dependencies;
		Sync = sync;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("dependencies", Dependencies);
		_bodyParameters.Add("sync", Sync);
		return _bodyParameters;
	}
}
