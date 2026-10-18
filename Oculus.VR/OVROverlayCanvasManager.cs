using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[DefaultExecutionOrder(-99)]
public class OVROverlayCanvasManager : MonoBehaviour
{
	private static OVROverlayCanvasManager _instance;

	private List<OVROverlayCanvas> _canvases = new List<OVROverlayCanvas>();

	public static OVROverlayCanvasManager Instance
	{
		get
		{
			if (!(_instance != null))
			{
				if (!Application.isPlaying)
				{
					return null;
				}
				return _instance = new GameObject("OVROverlayCanvasManager").AddComponent<OVROverlayCanvasManager>();
			}
			return _instance;
		}
	}

	public IEnumerable<OVROverlayCanvas> Canvases => _canvases;

	public static void AddCanvas(OVROverlayCanvas canvas)
	{
		Instance?._canvases.Add(canvas);
	}

	public static void RemoveCanvas(OVROverlayCanvas canvas)
	{
		_instance?._canvases.Remove(canvas);
	}

	public bool IsCanvasPriority(OVROverlayCanvas canvas)
	{
		if (canvas.GetViewPriorityScore().HasValue)
		{
			return _canvases.IndexOf(canvas) < OVROverlayCanvasSettings.Instance.MaxSimultaneousCanvases;
		}
		return false;
	}

	protected void Awake()
	{
		_instance = this;
		Object.DontDestroyOnLoad(this);
	}

	protected void Update()
	{
		_canvases.Sort(delegate(OVROverlayCanvas a, OVROverlayCanvas b)
		{
			float valueOrDefault = a.GetViewPriorityScore().GetValueOrDefault();
			float valueOrDefault2 = b.GetViewPriorityScore().GetValueOrDefault();
			return (!Mathf.Approximately(valueOrDefault, valueOrDefault2)) ? ((int)((valueOrDefault2 - valueOrDefault) * 10000f)) : (b.GetHashCode() - a.GetHashCode());
		});
	}

	protected void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}
}
