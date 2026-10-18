using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UpdateModMediaResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public UpdateModMediaResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
