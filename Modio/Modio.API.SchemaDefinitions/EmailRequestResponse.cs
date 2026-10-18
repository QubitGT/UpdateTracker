using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct EmailRequestResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public EmailRequestResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
