namespace UnityEngine.XR.OpenXR.NativeTypes;

public static class XrResultExtensions
{
	public static bool IsSuccess(this XrResult xrResult)
	{
		return xrResult >= XrResult.Success;
	}

	public static bool IsUnqualifiedSuccess(this XrResult xrResult)
	{
		return xrResult == XrResult.Success;
	}

	public static bool IsError(this XrResult xrResult)
	{
		return xrResult < XrResult.Success;
	}
}
