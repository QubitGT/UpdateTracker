namespace UnityEngine.Rendering.Universal;

internal sealed class StpHistory : CameraHistoryItem
{
	private STP.HistoryContext[] m_historyContexts = new STP.HistoryContext[2];

	public override void OnCreate(BufferedRTHandleSystem owner, uint typeId)
	{
		base.OnCreate(owner, typeId);
		for (int i = 0; i < 2; i++)
		{
			m_historyContexts[i] = new STP.HistoryContext();
		}
	}

	public override void Reset()
	{
		for (int i = 0; i < 2; i++)
		{
			m_historyContexts[i].Dispose();
		}
	}

	internal STP.HistoryContext GetHistoryContext(int eyeIndex)
	{
		return m_historyContexts[eyeIndex];
	}

	internal bool Update(UniversalCameraData cameraData)
	{
		STP.HistoryUpdateInfo info = default(STP.HistoryUpdateInfo);
		info.preUpscaleSize = new Vector2Int(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);
		info.postUpscaleSize = new Vector2Int(cameraData.pixelWidth, cameraData.pixelHeight);
		info.useHwDrs = false;
		info.useTexArray = cameraData.xr.enabled && cameraData.xr.singlePassEnabled;
		int eyeIndex = ((cameraData.xr.enabled && !cameraData.xr.singlePassEnabled) ? cameraData.xr.multipassId : 0);
		return !GetHistoryContext(eyeIndex).Update(ref info);
	}
}
