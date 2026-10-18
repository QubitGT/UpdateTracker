using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct EmailAuthenticationSecurityCodeRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string SecurityCode;

	[JsonConstructor]
	public EmailAuthenticationSecurityCodeRequest(string security_code)
	{
		SecurityCode = security_code;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("security_code", SecurityCode);
		return _bodyParameters;
	}
}
