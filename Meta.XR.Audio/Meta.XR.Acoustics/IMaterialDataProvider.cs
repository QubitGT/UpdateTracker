namespace Meta.XR.Acoustics;

internal interface IMaterialDataProvider
{
	MaterialData Data { get; }

	string name { get; }
}
