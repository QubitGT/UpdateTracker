using System;

namespace DigitalOpus.MB.Core;

[Serializable]
public struct AtlasPadding
{
	public int topBottom;

	public int leftRight;

	public AtlasPadding(int p)
	{
		topBottom = p;
		leftRight = p;
	}

	public AtlasPadding(int px, int py)
	{
		topBottom = py;
		leftRight = px;
	}
}
