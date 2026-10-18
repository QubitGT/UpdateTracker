using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddModDependenciesResponse
{
	internal readonly long Code;

	internal readonly string Message;

	[JsonConstructor]
	public AddModDependenciesResponse(long code, string message)
	{
		Code = code;
		Message = message;
	}
}
