using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct PaginationObject
{
	internal readonly long PerPage;

	internal readonly string CurrentPage;

	internal readonly long NextPageUrl;

	internal readonly long PrevPageUrl;

	[JsonConstructor]
	public PaginationObject(long per_page, string current_page, long next_page_url, long prev_page_url)
	{
		PerPage = per_page;
		CurrentPage = current_page;
		NextPageUrl = next_page_url;
		PrevPageUrl = prev_page_url;
	}
}
