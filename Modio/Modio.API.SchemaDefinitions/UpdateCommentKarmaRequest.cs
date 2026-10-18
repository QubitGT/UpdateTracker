using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UpdateCommentKarmaRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long Karma;

	[JsonConstructor]
	public UpdateCommentKarmaRequest(long karma)
	{
		Karma = karma;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("karma", Karma);
		return _bodyParameters;
	}
}
