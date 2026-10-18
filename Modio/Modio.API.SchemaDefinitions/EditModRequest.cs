using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
internal readonly struct EditModRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string? Name;

	internal readonly string? NameId;

	internal readonly string? Summary;

	internal readonly string? Description;

	internal readonly ModioAPIFileParameter? Logo;

	internal readonly long? Visible;

	internal readonly long? MaturityOption;

	internal readonly long? CommunityOptions;

	internal readonly string? MetadataBlob;

	internal readonly string[]? Tags;

	internal readonly long? MonetizationOptions;

	internal readonly long? Price;

	internal readonly long? Stock;

	[JsonConstructor]
	public EditModRequest(string? name, string? nameId, string? summary, string? description, ModioAPIFileParameter? logo, long? visible, long? maturity_option, long? community_options, string? metadataBlob, string[]? tags, long? monetizationOptions, long? price, long? stock)
	{
		Name = name;
		NameId = nameId;
		Summary = summary;
		Description = description;
		Logo = logo;
		Visible = visible;
		MaturityOption = maturity_option;
		CommunityOptions = community_options;
		MetadataBlob = metadataBlob;
		Tags = tags;
		MonetizationOptions = monetizationOptions;
		Price = price;
		Stock = stock;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		if (!string.IsNullOrEmpty(Name))
		{
			_bodyParameters.Add("name", Name);
		}
		if (!string.IsNullOrEmpty(NameId))
		{
			_bodyParameters.Add("name_id", NameId);
		}
		if (!string.IsNullOrEmpty(Summary))
		{
			_bodyParameters.Add("summary", Summary);
		}
		if (!string.IsNullOrEmpty(Description))
		{
			_bodyParameters.Add("description", Description);
		}
		if (Logo.HasValue)
		{
			_bodyParameters.Add("logo", Logo);
		}
		if (Visible.HasValue)
		{
			_bodyParameters.Add("visible", Visible);
		}
		if (MaturityOption.HasValue)
		{
			_bodyParameters.Add("maturity_option", MaturityOption);
		}
		if (CommunityOptions.HasValue)
		{
			_bodyParameters.Add("community_options", CommunityOptions);
		}
		if (!string.IsNullOrEmpty(MetadataBlob))
		{
			_bodyParameters.Add("metadata_blob", MetadataBlob);
		}
		if (Tags != null)
		{
			_bodyParameters.Add("tags", Tags);
		}
		if (MonetizationOptions.HasValue)
		{
			_bodyParameters.Add("monetization_options", MonetizationOptions);
		}
		if (Price.HasValue)
		{
			_bodyParameters.Add("price", Price);
		}
		if (Stock.HasValue)
		{
			_bodyParameters.Add("stock", Stock);
		}
		return _bodyParameters;
	}
}
