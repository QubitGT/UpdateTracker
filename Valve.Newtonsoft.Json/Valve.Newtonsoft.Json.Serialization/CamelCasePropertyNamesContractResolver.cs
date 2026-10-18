namespace Valve.Newtonsoft.Json.Serialization;

public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
{
	public CamelCasePropertyNamesContractResolver()
		: base(shareCache: true)
	{
		base.NamingStrategy = new CamelCaseNamingStrategy
		{
			ProcessDictionaryKeys = true,
			OverrideSpecifiedNames = true
		};
	}
}
