using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddReportResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public AddReportResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
