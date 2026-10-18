using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ErrorObject
{
	[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
	internal readonly struct EmbeddedError
	{
		internal readonly long Code;

		internal readonly long ErrorRef;

		internal readonly string Message;

		internal readonly JObject Errors;

		[JsonConstructor]
		public EmbeddedError(long code, long errorRef, string message, JObject errors)
		{
			Code = code;
			ErrorRef = errorRef;
			Message = message;
			Errors = errors;
		}
	}

	internal readonly EmbeddedError Error;

	[JsonConstructor]
	public ErrorObject(EmbeddedError error)
	{
		Error = error;
	}
}
