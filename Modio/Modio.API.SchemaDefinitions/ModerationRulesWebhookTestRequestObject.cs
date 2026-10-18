using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ModerationRulesWebhookTestRequestObject
{
	internal readonly string Url;

	[JsonConstructor]
	public ModerationRulesWebhookTestRequestObject(string url)
	{
		Url = url;
	}
}
