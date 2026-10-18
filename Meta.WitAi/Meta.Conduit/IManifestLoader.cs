using System.Threading.Tasks;

namespace Meta.Conduit;

internal interface IManifestLoader
{
	Manifest LoadManifest(string filePath);

	Manifest LoadManifestFromJson(string manifestText);

	Task<Manifest> LoadManifestAsync(string filePath);

	Task<Manifest> LoadManifestFromJsonAsync(string manifestText);
}
