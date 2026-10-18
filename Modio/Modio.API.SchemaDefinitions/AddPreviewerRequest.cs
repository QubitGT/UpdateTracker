using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddPreviewerRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long Member;

	[JsonConstructor]
	public AddPreviewerRequest(long member)
	{
		Member = member;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("member", Member);
		return _bodyParameters;
	}
}
