using System.Diagnostics;

namespace Meta.XR.MRUtilityKit;

internal static class MRUKAssert
{
	[Conditional("OVR_INTERNAL_CODE")]
	internal static void AreEqual<T>(T expected, T actual, string message = null)
	{
	}
}
