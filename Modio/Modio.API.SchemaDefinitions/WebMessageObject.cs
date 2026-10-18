using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct WebMessageObject
{
	internal readonly long Code;

	internal readonly bool Success;

	internal readonly string Message;

	[JsonConstructor]
	public WebMessageObject(long code, bool success, string message)
	{
		Code = code;
		Success = success;
		Message = message;
	}
}
