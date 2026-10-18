using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

[AddComponentMenu("XR/Input/Screen Space Ray Pose Driver", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceRayPoseDriver.html")]
[DefaultExecutionOrder(-31000)]
public class ScreenSpaceRayPoseDriver : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The camera associated with the screen, and through which screen presses/touches will be interpreted.")]
	private Camera m_ControllerCamera;

	[SerializeField]
	private XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

	[SerializeField]
	private XRInputValueReader<Vector2> m_DragStartPositionInput = new XRInputValueReader<Vector2>("Drag Start Position");

	[SerializeField]
	private XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

	[SerializeField]
	[Tooltip("The input used to read the screen touch count value.")]
	private XRInputValueReader<int> m_ScreenTouchCountInput = new XRInputValueReader<int>("Screen Touch Count");

	private readonly UnityObjectReferenceCache<Transform> m_ParentTransformCache = new UnityObjectReferenceCache<Transform>();

	private Vector2 m_TapStartPosition;

	private Vector2 m_DragStartPosition;

	public Camera controllerCamera
	{
		get
		{
			return m_ControllerCamera;
		}
		set
		{
			m_ControllerCamera = value;
		}
	}

	public XRInputValueReader<Vector2> tapStartPositionInput
	{
		get
		{
			return m_TapStartPositionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> dragStartPositionInput
	{
		get
		{
			return m_DragStartPositionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_DragStartPositionInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> dragCurrentPositionInput
	{
		get
		{
			return m_DragCurrentPositionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
		}
	}

	public XRInputValueReader<int> screenTouchCountInput
	{
		get
		{
			return m_ScreenTouchCountInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ScreenTouchCountInput, value, this);
		}
	}

	protected void OnEnable()
	{
		if (m_ControllerCamera == null)
		{
			m_ControllerCamera = Camera.main;
			if (m_ControllerCamera == null)
			{
				Debug.LogWarning("Could not find associated Camera in scene.This ScreenSpaceRayPoseDriver will be disabled.", this);
				base.enabled = false;
				return;
			}
		}
		m_TapStartPositionInput.EnableDirectActionIfModeUsed();
		m_DragStartPositionInput.EnableDirectActionIfModeUsed();
		m_DragCurrentPositionInput.EnableDirectActionIfModeUsed();
		m_ScreenTouchCountInput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_TapStartPositionInput.DisableDirectActionIfModeUsed();
		m_DragStartPositionInput.DisableDirectActionIfModeUsed();
		m_DragCurrentPositionInput.DisableDirectActionIfModeUsed();
		m_ScreenTouchCountInput.DisableDirectActionIfModeUsed();
	}

	protected void Update()
	{
		Vector2 tapStartPosition = m_TapStartPosition;
		bool flag = m_TapStartPositionInput.TryReadValue(out m_TapStartPosition) && tapStartPosition != m_TapStartPosition;
		Vector2 dragStartPosition = m_DragStartPosition;
		bool flag2 = m_DragStartPositionInput.TryReadValue(out m_DragStartPosition) && dragStartPosition != m_DragStartPosition;
		if (!m_ScreenTouchCountInput.TryReadValue(out var value) || value <= 1)
		{
			Vector2 value2;
			if (flag2)
			{
				ApplyPose(m_DragStartPosition);
			}
			else if (m_DragCurrentPositionInput.TryReadValue(out value2))
			{
				ApplyPose(value2);
			}
			else if (flag)
			{
				ApplyPose(m_TapStartPosition);
			}
		}
	}

	private void ApplyPose(Vector2 screenPosition)
	{
		Vector3 vector = m_ControllerCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, m_ControllerCamera.nearClipPlane));
		Vector3 normalized = (vector - m_ControllerCamera.transform.position).normalized;
		Transform fieldOrNull;
		Vector3 position = (m_ParentTransformCache.TryGet(base.transform.parent, out fieldOrNull) ? fieldOrNull.InverseTransformPoint(vector) : vector);
		base.transform.SetLocalPose(new Pose(position, Quaternion.LookRotation(normalized)));
	}
}
