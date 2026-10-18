using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-samples-overview/")]
public class OVRSceneSampleController : MonoBehaviour
{
	public KeyCode quitKey = KeyCode.Escape;

	public Texture fadeInTexture;

	public float speedRotationIncrement = 0.05f;

	private OVRPlayerController playerController;

	private OVRCameraRig cameraController;

	public string layerName = "Default";

	private bool visionMode = true;

	private OVRGridCube gridCube;

	private void Awake()
	{
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRCameraRig attached.");
		}
		else
		{
			cameraController = componentsInChildren[0];
		}
		OVRPlayerController[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<OVRPlayerController>();
		if (componentsInChildren2.Length == 0)
		{
			Debug.LogWarning("OVRMainMenu: No OVRPlayerController attached.");
		}
		else if (componentsInChildren2.Length > 1)
		{
			Debug.LogWarning("OVRMainMenu: More then 1 OVRPlayerController attached.");
		}
		else
		{
			playerController = componentsInChildren2[0];
		}
	}

	private void Start()
	{
		if (!Application.isEditor)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
		if (cameraController != null)
		{
			gridCube = base.gameObject.AddComponent<OVRGridCube>();
			gridCube.SetOVRCameraController(ref cameraController);
		}
	}

	private void Update()
	{
		UpdateRecenterPose();
		UpdateVisionMode();
		if (playerController != null)
		{
			UpdateSpeedAndRotationScaleMultiplier();
		}
	}

	private void UpdateVisionMode()
	{
	}

	private void UpdateSpeedAndRotationScaleMultiplier()
	{
		float moveScaleMultiplier = 0f;
		playerController.GetMoveScaleMultiplier(ref moveScaleMultiplier);
		playerController.SetMoveScaleMultiplier(moveScaleMultiplier);
		float rotationScaleMultiplier = 0f;
		playerController.GetRotationScaleMultiplier(ref rotationScaleMultiplier);
		playerController.SetRotationScaleMultiplier(rotationScaleMultiplier);
	}

	private void UpdateRecenterPose()
	{
	}
}
