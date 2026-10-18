namespace UnityEngine.Rendering;

internal struct BinningConfig
{
	public int viewCount;

	public bool supportsCrossFade;

	public bool supportsMotionCheck;

	public int visibilityConfigCount
	{
		get
		{
			int num = 1 + viewCount + (supportsCrossFade ? 1 : 0) + (supportsMotionCheck ? 1 : 0);
			return 1 << num;
		}
	}
}
