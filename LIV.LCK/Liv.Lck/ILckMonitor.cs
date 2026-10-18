using UnityEngine;

namespace Liv.Lck;

public interface ILckMonitor
{
	string MonitorId { get; }

	void SetRenderTexture(RenderTexture renderTexture);
}
