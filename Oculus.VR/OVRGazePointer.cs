using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-ovroverlay/#using-cylinder-overlays")]
public class OVRGazePointer : OVRCursor
{
	private Transform gazeIcon;

	[Tooltip("Should the pointer be hidden when not over interactive objects.")]
	public bool hideByDefault = true;

	[Tooltip("Time after leaving interactive object before pointer fades.")]
	public float showTimeoutPeriod = 1f;

	[Tooltip("Time after mouse pointer becoming inactive before pointer unfades.")]
	public float hideTimeoutPeriod = 0.1f;

	[Tooltip("Keep a faint version of the pointer visible while using a mouse")]
	public bool dimOnHideRequest = true;

	[Tooltip("Angular scale of pointer")]
	public float depthScaleMultiplier = 0.03f;

	public bool matchNormalOnPhysicsColliders;

	public Transform rayTransform;

	private float depth;

	private float hideUntilTime;

	private int positionSetsThisFrame;

	private float lastShowRequestTime;

	private float lastHideRequestTime;

	private OVRProgressIndicator progressIndicator;

	private static OVRGazePointer _instance;

	public bool hidden { get; private set; }

	public float currentScale { get; private set; }

	public static OVRGazePointer instance
	{
		get
		{
			if (_instance == null)
			{
				Debug.Log(string.Format("Instanciating GazePointer", 0));
				_instance = Object.Instantiate((OVRGazePointer)Resources.Load("Prefabs/GazePointerRing", typeof(OVRGazePointer)));
			}
			return _instance;
		}
	}

	public float visibilityStrength
	{
		get
		{
			float a = ((!hideByDefault) ? 1f : Mathf.Clamp01(1f - (Time.time - lastShowRequestTime) / showTimeoutPeriod));
			float b = ((!(lastHideRequestTime + hideTimeoutPeriod > Time.time)) ? 1f : (dimOnHideRequest ? 0.1f : 0f));
			return Mathf.Min(a, b);
		}
	}

	public float SelectionProgress
	{
		get
		{
			if (!progressIndicator)
			{
				return 0f;
			}
			return progressIndicator.currentProgress;
		}
		set
		{
			if ((bool)progressIndicator)
			{
				progressIndicator.currentProgress = value;
			}
		}
	}

	public void Awake()
	{
		currentScale = 1f;
		if (_instance != null && _instance != this)
		{
			base.enabled = false;
			Object.DestroyImmediate(this);
		}
		else
		{
			_instance = this;
			gazeIcon = base.transform.Find("GazeIcon");
			progressIndicator = base.transform.GetComponent<OVRProgressIndicator>();
		}
	}

	private void Update()
	{
		if (rayTransform == null && Camera.main != null)
		{
			rayTransform = Camera.main.transform;
		}
		base.transform.position = rayTransform.position + rayTransform.forward * depth;
		if (visibilityStrength == 0f && !hidden)
		{
			Hide();
		}
		else if (visibilityStrength > 0f && hidden)
		{
			Show();
		}
	}

	public override void SetCursorStartDest(Vector3 _, Vector3 pos, Vector3 normal)
	{
		base.transform.position = pos;
		if (!matchNormalOnPhysicsColliders)
		{
			normal = rayTransform.forward;
		}
		Quaternion rotation = base.transform.rotation;
		rotation.SetLookRotation(normal, rayTransform.up);
		base.transform.rotation = rotation;
		depth = (rayTransform.position - pos).magnitude;
		currentScale = depth * depthScaleMultiplier;
		base.transform.localScale = new Vector3(currentScale, currentScale, currentScale);
		positionSetsThisFrame++;
		RequestShow();
	}

	public override void SetCursorRay(Transform ray)
	{
	}

	private void LateUpdate()
	{
		if (positionSetsThisFrame == 0)
		{
			Quaternion rotation = base.transform.rotation;
			rotation.SetLookRotation(rayTransform.forward, rayTransform.up);
			base.transform.rotation = rotation;
		}
		Quaternion rotation2 = gazeIcon.rotation;
		rotation2.SetLookRotation(base.transform.rotation * new Vector3(0f, 0f, 1f));
		gazeIcon.rotation = rotation2;
		positionSetsThisFrame = 0;
	}

	public void RequestHide()
	{
		if (!dimOnHideRequest)
		{
			Hide();
		}
		lastHideRequestTime = Time.time;
	}

	public void RequestShow()
	{
		Show();
		lastShowRequestTime = Time.time;
	}

	private void Hide()
	{
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(value: false);
		}
		if ((bool)GetComponent<Renderer>())
		{
			GetComponent<Renderer>().enabled = false;
		}
		hidden = true;
	}

	private void Show()
	{
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(value: true);
		}
		if ((bool)GetComponent<Renderer>())
		{
			GetComponent<Renderer>().enabled = true;
		}
		hidden = false;
	}
}
