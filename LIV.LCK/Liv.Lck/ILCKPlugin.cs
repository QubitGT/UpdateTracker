namespace Liv.Lck;

public interface ILCKPlugin
{
	string PluginName { get; }

	string PluginVersion { get; }

	void Initialize(LckService lckService);

	void Shutdown();
}
