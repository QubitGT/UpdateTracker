using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddTeamMemberRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Email;

	internal readonly long ToUserId;

	internal readonly string Position;

	internal readonly long Level;

	[JsonConstructor]
	public AddTeamMemberRequest(string email, long to_user_id, string position, long level)
	{
		Email = email;
		ToUserId = to_user_id;
		Position = position;
		Level = level;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("email", Email);
		_bodyParameters.Add("to_user_id", ToUserId);
		_bodyParameters.Add("position", Position);
		_bodyParameters.Add("level", Level);
		return _bodyParameters;
	}
}
