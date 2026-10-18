using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct MessageObject
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public MessageObject(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
