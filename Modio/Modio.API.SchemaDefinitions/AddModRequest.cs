using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct AddModRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Name;

	internal readonly string? NameId;

	internal readonly string Summary;

	internal readonly string? Description;

	internal readonly ModioAPIFileParameter Logo;

	internal readonly long? Visible;

	internal readonly long? MaturityOption;

	internal readonly long? CommunityOptions;

	internal readonly string? MetadataBlob;

	internal readonly string[]? Tags;

	[JsonConstructor]
	public AddModRequest(string name, string? name_id, string summary, string? description, ModioAPIFileParameter logo, long? visible, long? maturity_option, long? community_options, string? metadata_blob, string[]? tags)
	{
		Name = name;
		NameId = name_id;
		Summary = summary;
		Description = description;
		Logo = logo;
		Visible = visible;
		MaturityOption = maturity_option;
		CommunityOptions = community_options;
		MetadataBlob = metadata_blob;
		Tags = tags;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("name", Name);
		_bodyParameters.Add("summary", Summary);
		_bodyParameters.Add("logo", Logo);
		if (!string.IsNullOrEmpty(NameId))
		{
			_bodyParameters.Add("name_id", NameId);
		}
		if (!string.IsNullOrEmpty(Description))
		{
			_bodyParameters.Add("description", Description);
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
		if (MetadataBlob != null)
		{
			_bodyParameters.Add("metadata_blob", MetadataBlob);
		}
		if (Tags != null)
		{
			for (int i = 0; i < Tags.Length; i++)
			{
				_bodyParameters.Add($"tags[{i}]", Tags[i]);
			}
		}
		return _bodyParameters;
	}
}
