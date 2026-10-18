using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AdminEditModStatusRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long Status;

	internal readonly string Reason;

	[JsonConstructor]
	public AdminEditModStatusRequest(long status, string reason)
	{
		Status = status;
		Reason = reason;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("status", Status);
		_bodyParameters.Add("reason", Reason);
		return _bodyParameters;
	}
}
