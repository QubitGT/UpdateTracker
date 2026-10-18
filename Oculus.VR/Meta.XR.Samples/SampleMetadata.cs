using UnityEngine;

namespace Meta.XR.Samples;

[ExecuteAlways]
internal class SampleMetadata : MonoBehaviour
{
	private float _timestampOpen;

	private void Awake()
	{
		_timestampOpen = Time.realtimeSinceStartup;
	}

	private void OnDestroy()
	{
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			SendEvent(163061602);
		}
		else
		{
			SendEvent(163055403);
		}
	}

	public void OnEditorShutdown()
	{
		SendEvent(163056880);
	}

	private void SendEvent(int eventType)
	{
		float num = Time.realtimeSinceStartup - _timestampOpen;
		OVRTelemetry.Start(eventType, 0, -1L).AddAnnotation("Sample", base.gameObject.scene.name).AddAnnotation("RuntimePlatform", Application.platform.ToString())
			.AddAnnotation("InEditor", Application.isEditor.ToString())
			.AddAnnotation("TimeSinceEditorStart", Time.realtimeSinceStartup.ToString("F0"))
			.AddAnnotation("TimeSpent", num.ToString("F0"))
			.Send();
	}
}
