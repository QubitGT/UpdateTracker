namespace UnityEngine.Rendering;

public class DebugOverlay
{
	private int m_InitialPositionX;

	private int m_ScreenWidth;

	public int x { get; private set; }

	public int y { get; private set; }

	public int overlaySize { get; private set; }

	public void StartOverlay(int initialX, int initialY, int overlaySize, int screenWidth)
	{
		x = initialX;
		y = initialY;
		this.overlaySize = overlaySize;
		m_InitialPositionX = initialX;
		m_ScreenWidth = screenWidth;
	}

	public Rect Next(float aspect = 1f)
	{
		int num = (int)((float)overlaySize * aspect);
		if (x + num > m_ScreenWidth && x > m_InitialPositionX)
		{
			x = m_InitialPositionX;
			y -= overlaySize;
		}
		Rect result = new Rect(x, y, num, overlaySize);
		x += num;
		return result;
	}

	public void SetViewport(CommandBuffer cmd)
	{
		cmd.SetViewport(new Rect(x, y, overlaySize, overlaySize));
	}
}
