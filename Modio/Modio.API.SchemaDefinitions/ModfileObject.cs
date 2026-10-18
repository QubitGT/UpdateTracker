using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModfileObject
{
	internal readonly long Id;

	internal readonly long ModId;

	internal readonly long DateAdded;

	internal readonly long DateUpdated;

	internal readonly long DateScanned;

	internal readonly long VirusStatus;

	internal readonly long VirusPositive;

	internal readonly string VirustotalHash;

	internal readonly long Filesize;

	internal readonly long FilesizeUncompressed;

	internal readonly FilehashObject Filehash;

	internal readonly string Filename;

	internal readonly string Version;

	internal readonly string Changelog;

	internal readonly string MetadataBlob;

	internal readonly DownloadObject Download;

	internal readonly ModfilePlatformObject[] Platforms;

	[JsonConstructor]
	public ModfileObject(long id, long mod_id, long date_added, long date_updated, long date_scanned, long virus_status, long virus_positive, string virustotal_hash, long filesize, long filesize_uncompressed, FilehashObject filehash, string filename, string version, string changelog, string metadata_blob, DownloadObject download, ModfilePlatformObject[] platforms)
	{
		Id = id;
		ModId = mod_id;
		DateAdded = date_added;
		DateUpdated = date_updated;
		DateScanned = date_scanned;
		VirusStatus = virus_status;
		VirusPositive = virus_positive;
		VirustotalHash = virustotal_hash;
		Filesize = filesize;
		FilesizeUncompressed = filesize_uncompressed;
		Filehash = filehash;
		Filename = filename;
		Version = version;
		Changelog = changelog;
		MetadataBlob = metadata_blob;
		Download = download;
		Platforms = platforms;
	}
}
