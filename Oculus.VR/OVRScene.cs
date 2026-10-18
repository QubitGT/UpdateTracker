using Meta.XR.Util;

[Feature(Feature.Scene)]
public static class OVRScene
{
	public static OVRTask<bool> RequestSpaceSetup()
	{
		ulong requestId;
		return OVRTask.Build(OVRPlugin.RequestSceneCapture(out requestId), requestId).ToTask(failureValue: false);
	}
}
