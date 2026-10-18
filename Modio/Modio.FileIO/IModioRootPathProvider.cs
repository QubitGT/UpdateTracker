namespace Modio.FileIO;

public interface IModioRootPathProvider
{
	string Path { get; }

	string UserPath { get; }
}
