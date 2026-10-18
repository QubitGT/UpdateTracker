using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UpdateTeamMemberRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Email;

	internal readonly string Position;

	internal readonly long Level;

	[JsonConstructor]
	public UpdateTeamMemberRequest(string email, string position, long level)
	{
		Email = email;
		Position = position;
		Level = level;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("email", Email);
		_bodyParameters.Add("position", Position);
		_bodyParameters.Add("level", Level);
		return _bodyParameters;
	}
}
