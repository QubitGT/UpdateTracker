using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddRatingResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public AddRatingResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
