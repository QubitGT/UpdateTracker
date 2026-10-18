using System.Collections.Generic;
using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct AddModMediaRequest : IApiRequest
{
	private static readonly Dictionary<string, object> _bodyParameters = new Dictionary<string, object>();

	internal readonly ModioAPIFileParameter _media;

	public bool GallerySync { get; }

	[JsonConstructor]
	public AddModMediaRequest(ModioAPIFileParameter media, bool gallerySync)
	{
		_media = media;
		GallerySync = gallerySync;
	}

	public IReadOnlyDictionary<string, object> GetBodyParameters()
	{
		_bodyParameters.Clear();
		_bodyParameters.Add(_media.MediaType, _media);
		return _bodyParameters;
	}
}
