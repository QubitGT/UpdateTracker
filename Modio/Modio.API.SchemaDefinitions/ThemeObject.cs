using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
internal readonly struct ThemeObject
{
	internal readonly string Primary;

	internal readonly string Dark;

	internal readonly string Light;

	internal readonly string Success;

	internal readonly string Warning;

	internal readonly string Danger;

	[JsonConstructor]
	public ThemeObject(string primary, string dark, string light, string success, string warning, string danger)
	{
		Primary = primary;
		Dark = dark;
		Light = light;
		Success = success;
		Warning = warning;
		Danger = danger;
	}
}
