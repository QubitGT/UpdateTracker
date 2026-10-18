using System;
using UnityEngine;

namespace Liv.Lck;

public class LckMonitor : MonoBehaviour, ILckMonitor
{
	public delegate void LckMonitorRenderTextureSetDelegate(RenderTexture renderTexture);

	[SerializeField]
	protected string _monitorId;

	public string MonitorId => _monitorId;

	public event LckMonitorRenderTextureSetDelegate OnRenderTextureSet;

	protected virtual void OnEnable()
	{
		if (string.IsNullOrEmpty(_monitorId))
		{
			_monitorId = Guid.NewGuid().ToString();
		}
		LckMediator.RegisterMonitor(this);
	}

	public virtual void SetRenderTexture(RenderTexture renderTexture)
	{
		this.OnRenderTextureSet?.Invoke(renderTexture);
	}

	protected virtual void OnDestroy()
	{
		LckMediator.UnregisterMonitor(this);
	}
}
