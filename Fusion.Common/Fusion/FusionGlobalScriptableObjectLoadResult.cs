namespace Fusion;

public readonly struct FusionGlobalScriptableObjectLoadResult
{
	public readonly FusionGlobalScriptableObject Object;

	public readonly FusionGlobalScriptableObjectUnloadDelegate Unloader;

	public FusionGlobalScriptableObjectLoadResult(FusionGlobalScriptableObject obj, FusionGlobalScriptableObjectUnloadDelegate unloader = null)
	{
		Object = obj;
		Unloader = unloader;
	}

	public static implicit operator FusionGlobalScriptableObjectLoadResult(FusionGlobalScriptableObject result)
	{
		return new FusionGlobalScriptableObjectLoadResult(result);
	}
}
