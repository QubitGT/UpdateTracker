namespace Modio.Mods;

public enum ModFileState
{
	None,
	Queued,
	Downloading,
	Downloaded,
	Installing,
	Installed,
	Updating,
	Uninstalling,
	FileOperationFailed
}
