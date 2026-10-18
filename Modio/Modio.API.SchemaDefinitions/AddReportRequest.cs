using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddReportRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly string Resource;

	internal readonly long Id;

	internal readonly long Type;

	internal readonly long Reason;

	internal readonly string Platforms;

	internal readonly string Name;

	internal readonly string Contact;

	internal readonly string Summary;

	[JsonConstructor]
	public AddReportRequest(string resource, long id, long type, long reason, string platforms, string name, string contact, string summary)
	{
		Resource = resource;
		Id = id;
		Type = type;
		Reason = reason;
		Platforms = platforms;
		Name = name;
		Contact = contact;
		Summary = summary;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add("resource", Resource);
		_bodyParameters.Add("id", Id);
		_bodyParameters.Add("type", Type);
		_bodyParameters.Add("reason", Reason);
		_bodyParameters.Add("platforms", Platforms);
		_bodyParameters.Add("name", Name);
		_bodyParameters.Add("contact", Contact);
		_bodyParameters.Add("summary", Summary);
		return _bodyParameters;
	}
}
