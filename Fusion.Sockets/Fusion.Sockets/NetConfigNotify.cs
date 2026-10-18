namespace Fusion.Sockets;

public struct NetConfigNotify
{
	public int AckMaskBytes;

	public int AckForceCount;

	public double AckForceTimeout;

	public int WindowSize;

	public int SequenceBytes;

	public int SequenceBounds => WindowSize * 16;

	public int AckMaskBits => AckMaskBytes * 8;

	public static NetConfigNotify Defaults
	{
		get
		{
			NetConfigNotify result = default(NetConfigNotify);
			result.AckMaskBytes = 8;
			result.AckForceCount = 8;
			result.AckForceTimeout = 0.1;
			result.WindowSize = 128;
			result.SequenceBytes = 2;
			return result;
		}
	}
}
