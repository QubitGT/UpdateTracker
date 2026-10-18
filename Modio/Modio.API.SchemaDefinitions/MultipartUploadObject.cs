using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct MultipartUploadObject
{
	internal readonly string UploadId;

	internal readonly long Status;

	[JsonConstructor]
	public MultipartUploadObject(string upload_id, long status)
	{
		UploadId = upload_id;
		Status = status;
	}
}
