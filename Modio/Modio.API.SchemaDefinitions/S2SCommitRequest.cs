using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct S2SCommitRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long TransactionId;

	internal readonly string ClawbackUuid;

	[JsonConstructor]
	public S2SCommitRequest(long transaction_id, string clawback_uuid)
	{
		TransactionId = transaction_id;
		ClawbackUuid = clawback_uuid;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("transaction_id", TransactionId);
		_bodyParameters.Add("clawback_uuid", ClawbackUuid);
		return _bodyParameters;
	}
}
