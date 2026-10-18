namespace Modio;

public enum ModioServicePriority
{
	Fallback = 0,
	Default = 10,
	EngineImplementation = 20,
	PlatformProvided = 30,
	DeveloperOverride = 40,
	UnitTestOverride = 100
}
