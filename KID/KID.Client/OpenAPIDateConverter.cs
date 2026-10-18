using Newtonsoft.Json.Converters;

namespace KID.Client;

public class OpenAPIDateConverter : IsoDateTimeConverter
{
	public OpenAPIDateConverter()
	{
		base.DateTimeFormat = "yyyy-MM-dd";
	}
}
