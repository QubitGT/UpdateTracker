namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

[AddComponentMenu("XR/Input/Touchscreen Gesture Input Loader", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.TouchscreenGestureInputLoader.html")]
public class TouchscreenGestureInputLoader : MonoBehaviour
{
	[Header("Gesture Configuration")]
	[SerializeField]
	[Tooltip("Time (in seconds) within (≤) which a touch and release has to occur for it to be registered as a tap.")]
	private float m_TapDuration = 0.5f;

	public float tapDuration
	{
		get
		{
			return m_TapDuration;
		}
		set
		{
			m_TapDuration = value;
		}
	}

	protected void Awake()
	{
		Debug.LogWarning("Script requires AR Foundation (com.unity.xr.arfoundation) package to add the TouchscreenGestureInputController device. Install using Window > Package Manager or click Fix on the related issue in Edit > Project Settings > XR Plug-in Management > Project Validation.", this);
		base.enabled = false;
	}

	protected void OnEnable()
	{
		InitializeTouchscreenGestureController();
		RefreshGestureRecognizersConfiguration();
	}

	protected void OnDisable()
	{
		RemoveTouchscreenGestureController();
	}

	public void RefreshGestureRecognizersConfiguration()
	{
	}

	private void InitializeTouchscreenGestureController()
	{
	}

	private void RemoveTouchscreenGestureController()
	{
	}
}
