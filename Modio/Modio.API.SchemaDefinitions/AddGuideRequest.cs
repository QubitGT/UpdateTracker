using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddGuideRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Name;

	internal readonly string Summary;

	internal readonly string Description;

	internal readonly string Logo;

	internal readonly long DateLive;

	internal readonly long Status;

	internal readonly long CommunityOptions;

	internal readonly string[] Tags;

	internal readonly string NameId;

	[JsonConstructor]
	public AddGuideRequest(string name, string summary, string description, string logo, long date_live, long status, long community_options, string[] tags, string name_id)
	{
		Name = name;
		Summary = summary;
		Description = description;
		Logo = logo;
		DateLive = date_live;
		Status = status;
		CommunityOptions = community_options;
		Tags = tags;
		NameId = name_id;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("name", Name);
		_bodyParameters.Add("summary", Summary);
		_bodyParameters.Add("description", Description);
		_bodyParameters.Add("logo", Logo);
		_bodyParameters.Add("date_live", DateLive);
		_bodyParameters.Add("status", Status);
		_bodyParameters.Add("community_options", CommunityOptions);
		_bodyParameters.Add("tags", Tags);
		_bodyParameters.Add("name_id", NameId);
		return _bodyParameters;
	}
}
