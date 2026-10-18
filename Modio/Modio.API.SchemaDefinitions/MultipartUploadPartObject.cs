using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct MultipartUploadPartObject
{
	internal readonly string UploadId;

	internal readonly long PartNumber;

	internal readonly long PartSize;

	internal readonly long DateAdded;

	[JsonConstructor]
	public MultipartUploadPartObject(string upload_id, long part_number, long part_size, long date_added)
	{
		UploadId = upload_id;
		PartNumber = part_number;
		PartSize = part_size;
		DateAdded = date_added;
	}
}
