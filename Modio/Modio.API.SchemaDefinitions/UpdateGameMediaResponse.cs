using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UpdateGameMediaResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public UpdateGameMediaResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
