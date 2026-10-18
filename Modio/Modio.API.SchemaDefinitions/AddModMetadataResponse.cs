using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddModMetadataResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public AddModMetadataResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
