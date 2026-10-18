using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ResolveReportsRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly long[] ReportIds;

	[JsonConstructor]
	public ResolveReportsRequest(long[] report_ids)
	{
		ReportIds = report_ids;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("report_ids", ReportIds);
		return _bodyParameters;
	}
}
