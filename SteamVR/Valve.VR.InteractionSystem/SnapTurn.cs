using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem;

public class SnapTurn : MonoBehaviour
{
	public float snapAngle = 90f;

	public bool showTurnAnimation = true;

	public AudioSource snapTurnSource;

	public AudioClip rotateSound;

	public GameObject rotateRightFX;

	public GameObject rotateLeftFX;

	public SteamVR_Action_Boolean snapLeftAction = SteamVR_Input.GetBooleanAction("SnapTurnLeft");

	public SteamVR_Action_Boolean snapRightAction = SteamVR_Input.GetBooleanAction("SnapTurnRight");

	public bool fadeScreen = true;

	public float fadeTime = 0.1f;

	public Color screenFadeColor = Color.black;

	public float distanceFromFace = 1.3f;

	public Vector3 additionalOffset = new Vector3(0f, -0.3f, 0f);

	public static float teleportLastActiveTime;

	private bool canRotate = true;

	public float canTurnEverySeconds = 0.4f;

	private Coroutine rotateCoroutine;

	private void Start()
	{
		AllOff();
	}

	private void AllOff()
	{
		if (rotateLeftFX != null)
		{
			rotateLeftFX.SetActive(value: false);
		}
		if (rotateRightFX != null)
		{
			rotateRightFX.SetActive(value: false);
		}
	}

	private void Update()
	{
		Player instance = Player.instance;
		if (canRotate && snapLeftAction != null && snapRightAction != null && snapLeftAction.activeBinding && snapRightAction.activeBinding && !(Time.time < teleportLastActiveTime + canTurnEverySeconds))
		{
			bool flag = instance.rightHand.currentAttachedObject == null || (instance.rightHand.currentAttachedObject != null && instance.rightHand.currentAttachedTeleportManager != null && instance.rightHand.currentAttachedTeleportManager.teleportAllowed);
			bool flag2 = instance.leftHand.currentAttachedObject == null || (instance.leftHand.currentAttachedObject != null && instance.leftHand.currentAttachedTeleportManager != null && instance.leftHand.currentAttachedTeleportManager.teleportAllowed);
			bool num = snapLeftAction.GetStateDown(SteamVR_Input_Sources.LeftHand) && flag2;
			bool flag3 = snapLeftAction.GetStateDown(SteamVR_Input_Sources.RightHand) && flag;
			bool flag4 = snapRightAction.GetStateDown(SteamVR_Input_Sources.LeftHand) && flag2;
			bool flag5 = snapRightAction.GetStateDown(SteamVR_Input_Sources.RightHand) && flag;
			if (num || flag3)
			{
				RotatePlayer(0f - snapAngle);
			}
			else if (flag4 || flag5)
			{
				RotatePlayer(snapAngle);
			}
		}
	}

	public void RotatePlayer(float angle)
	{
		if (rotateCoroutine != null)
		{
			StopCoroutine(rotateCoroutine);
			AllOff();
		}
		rotateCoroutine = StartCoroutine(DoRotatePlayer(angle));
	}

	private IEnumerator DoRotatePlayer(float angle)
	{
		Player player = Player.instance;
		canRotate = false;
		snapTurnSource.panStereo = angle / 90f;
		snapTurnSource.PlayOneShot(rotateSound);
		if (fadeScreen)
		{
			SteamVR_Fade.Start(Color.clear, 0f);
			Color color = screenFadeColor;
			color = color.linear * 0.6f;
			SteamVR_Fade.Start(color, fadeTime);
		}
		yield return new WaitForSeconds(fadeTime);
		Vector3 vector = player.trackingOriginTransform.position - player.feetPositionGuess;
		player.trackingOriginTransform.position -= vector;
		player.transform.Rotate(Vector3.up, angle);
		vector = Quaternion.Euler(0f, angle, 0f) * vector;
		player.trackingOriginTransform.position += vector;
		GameObject fx = ((angle > 0f) ? rotateRightFX : rotateLeftFX);
		if (showTurnAnimation)
		{
			ShowRotateFX(fx);
		}
		if (fadeScreen)
		{
			SteamVR_Fade.Start(Color.clear, fadeTime);
		}
		float time = Time.time;
		float endTime = time + canTurnEverySeconds;
		while (Time.time <= endTime)
		{
			yield return null;
			UpdateOrientation(fx);
		}
		fx.SetActive(value: false);
		canRotate = true;
	}

	private void ShowRotateFX(GameObject fx)
	{
		if (!(fx == null))
		{
			fx.SetActive(value: false);
			UpdateOrientation(fx);
			fx.SetActive(value: true);
			UpdateOrientation(fx);
		}
	}

	private void UpdateOrientation(GameObject fx)
	{
		Player instance = Player.instance;
		base.transform.position = instance.hmdTransform.position + instance.hmdTransform.forward * distanceFromFace;
		base.transform.rotation = Quaternion.LookRotation(instance.hmdTransform.position - base.transform.position, Vector3.up);
		base.transform.Translate(additionalOffset, Space.Self);
		base.transform.rotation = Quaternion.LookRotation(instance.hmdTransform.position - base.transform.position, Vector3.up);
	}
}
