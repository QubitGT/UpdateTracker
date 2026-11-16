using System;
using System.Collections;
using System.Globalization;
using GorillaExtensions;
using GorillaTag;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public abstract class GorillaKeyButton<TBinding> : MonoBehaviour where TBinding : Enum
{
	private const int TapSoundIndex = 66;

	private const float TapSoundVolume = 0.1f;

	public string characterString;

	public TBinding Binding;

	public bool functionKey;

	public Renderer ButtonRenderer;

	public ButtonColorSettings ButtonColorSettings;

	public TextMeshPro ButtonText;

	[Tooltip("These GameObjects will be Activated/Deactivated when this button is Activated/Deactivated")]
	public GameObject[] linkedObjects;

	[Tooltip("Intended for use with GorillaKeyWrapper")]
	public UnityEvent<TBinding> OnKeyButtonPressed = new UnityEvent<TBinding>();

	[Header("Key Press")]
	[Tooltip("Ignore new presses until the hand that pressed the key leaves it, so resting a hand on a key or brushing past doesn't type twice")]
	public bool requireReleaseBeforeRepress;

	[Tooltip("Seconds after a hand press before the key can be pressed again (real time, so hitches can't skip it)")]
	public float pressCooldown;

	[Tooltip("Seconds a hand has to stay out of the key before the next press counts, so a hand on the edge of the key flickering out and back in doesn't type twice")]
	public float releaseDebounce = 0.05f;

	[Tooltip("Local offset the key moves by while pressed, e.g. (0, -0.01, 0). Zero leaves the key where it is")]
	public Vector3 pressTravel = Vector3.zero;

	[Tooltip("Seconds the key takes to move down or back up")]
	public float pressTravelTime = 0.05f;

	public bool testClick;

	public bool repeatTestClick;

	public float repeatCooldown = 2f;

	protected MaterialPropertyBlock propBlock;

	private Coroutine pressColorRoutine;

	private Color? unpressedColorOverride;

	private Collider pressingCollider;

	private double lastHandPressTime = double.MinValue;

	private double lastReleaseTime = double.MinValue;

	private int lastHandPressFrame = -1;

	private Vector3 restLocalPosition;

	private Coroutine pressTravelRoutine;

	private float lastTestClick;

	private float PressedTime
	{
		get
		{
			if (!(ButtonColorSettings != null))
			{
				return 0f;
			}
			return ButtonColorSettings.PressedTime;
		}
	}

	private void Awake()
	{
		if (ButtonRenderer == null)
		{
			ButtonRenderer = GetComponent<Renderer>();
		}
		propBlock = new MaterialPropertyBlock();
		restLocalPosition = base.transform.localPosition;
	}

	private void OnEnable()
	{
		SetLinkedObjectsActive(active: true);
		OnEnableEvents();
	}

	private void OnDisable()
	{
		SetLinkedObjectsActive(active: false);
		pressingCollider = null;
		if (pressColorRoutine != null)
		{
			pressColorRoutine = null;
			SetButtonColor(pressed: false);
		}
		if (pressTravelRoutine != null)
		{
			pressTravelRoutine = null;
			base.transform.localPosition = restLocalPosition;
		}
		OnDisableEvents();
	}

	private void SetLinkedObjectsActive(bool active)
	{
		for (int i = 0; i < linkedObjects.Length; i++)
		{
			if (linkedObjects[i].IsNotNull())
			{
				linkedObjects[i].SetActive(active);
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		GorillaTriggerColliderHandIndicator componentInParent = collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
		if (!componentInParent || (requireReleaseBeforeRepress && IsHeldDown()))
		{
			return;
		}
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		if (Time.frameCount != lastHandPressFrame && !(realtimeSinceStartupAsDouble - lastHandPressTime < (double)pressCooldown))
		{
			if (requireReleaseBeforeRepress && realtimeSinceStartupAsDouble - lastReleaseTime < (double)releaseDebounce)
			{
				pressingCollider = collider;
				return;
			}
			lastHandPressTime = realtimeSinceStartupAsDouble;
			lastHandPressFrame = Time.frameCount;
			pressingCollider = collider;
			PressButton(componentInParent.isLeftHand);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (collider == pressingCollider)
		{
			pressingCollider = null;
			lastReleaseTime = Time.realtimeSinceStartupAsDouble;
		}
	}

	private bool IsHeldDown()
	{
		if (pressingCollider != null && pressingCollider.enabled)
		{
			return pressingCollider.gameObject.activeInHierarchy;
		}
		return false;
	}

	public void Click(bool leftHand = false)
	{
		PressButton(leftHand);
	}

	private void PressButton(bool isLeftHand)
	{
		OnButtonPressedEvent();
		OnKeyButtonPressed?.Invoke(Binding);
		PressButtonColorUpdate();
		PressButtonTravel();
		GorillaTagger instance = GorillaTagger.Instance;
		instance.StartVibration(isLeftHand, instance.tapHapticStrength / 2f, instance.tapHapticDuration);
		instance.offlineVRRig.PlayHandTapLocal(66, isLeftHand, 0.1f);
		if (NetworkSystem.Instance.InRoom && instance.myVRRig != null)
		{
			instance.myVRRig.SendRPC("RPC_PlayHandTap", RpcTarget.Others, 66, isLeftHand, 0.1f);
		}
	}

	protected abstract void OnButtonPressedEvent();

	protected virtual void OnEnableEvents()
	{
	}

	protected virtual void OnDisableEvents()
	{
	}

	public virtual void PressButtonColorUpdate()
	{
		if (base.gameObject.activeInHierarchy && !(ButtonRenderer == null) && !(ButtonColorSettings == null))
		{
			if (pressColorRoutine != null)
			{
				StopCoroutine(pressColorRoutine);
			}
			SetButtonColor(pressed: true);
			pressColorRoutine = StartCoroutine(PressColorRoutine());
		}
	}

	private IEnumerator PressColorRoutine()
	{
		yield return new WaitForSeconds(PressedTime);
		SetButtonColor(pressed: false);
		pressColorRoutine = null;
	}

	protected void SetUnpressedColorOverride(Color? color)
	{
		if (!(unpressedColorOverride == color))
		{
			unpressedColorOverride = color;
			if (pressColorRoutine == null)
			{
				SetButtonColor(pressed: false);
			}
		}
	}

	public void SetColorHex(string color)
	{
		SetUnpressedColorOverride(new Color((float)int.Parse(color.Substring(0, 2), NumberStyles.HexNumber) / 255f, (float)int.Parse(color.Substring(2, 2), NumberStyles.HexNumber) / 255f, (float)int.Parse(color.Substring(4, 2), NumberStyles.HexNumber) / 255f));
	}

	public void UnsetColor()
	{
		unpressedColorOverride = null;
		if (pressColorRoutine == null)
		{
			SetButtonColor(pressed: false);
		}
	}

	private void SetButtonColor(bool pressed)
	{
		if (ButtonRenderer == null)
		{
			return;
		}
		Color value;
		if (pressed)
		{
			if (ButtonColorSettings == null)
			{
				return;
			}
			value = ButtonColorSettings.PressedColor;
		}
		else if (unpressedColorOverride.HasValue)
		{
			value = unpressedColorOverride.Value;
		}
		else
		{
			if (!(ButtonColorSettings != null))
			{
				propBlock.Clear();
				ButtonRenderer.SetPropertyBlock(propBlock);
				return;
			}
			value = ButtonColorSettings.UnpressedColor;
		}
		propBlock.SetColor(ShaderProps._BaseColor, value);
		propBlock.SetColor(ShaderProps._Color, value);
		ButtonRenderer.SetPropertyBlock(propBlock);
	}

	private void PressButtonTravel()
	{
		if (!(pressTravel == Vector3.zero) && base.gameObject.activeInHierarchy)
		{
			if (pressTravelRoutine != null)
			{
				StopCoroutine(pressTravelRoutine);
			}
			pressTravelRoutine = StartCoroutine(PressTravelRoutine());
		}
	}

	private IEnumerator PressTravelRoutine()
	{
		Vector3 pressedLocalPosition = restLocalPosition + pressTravel;
		yield return MoveKey(base.transform.localPosition, pressedLocalPosition);
		float releaseTime = Time.time + PressedTime;
		while (Time.time < releaseTime || IsHeldDown())
		{
			yield return null;
		}
		yield return MoveKey(pressedLocalPosition, restLocalPosition);
		pressTravelRoutine = null;
	}

	private IEnumerator MoveKey(Vector3 from, Vector3 to)
	{
		float duration = Mathf.Max(0.001f, pressTravelTime);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			base.transform.localPosition = Vector3.Lerp(from, to, t / duration);
			yield return null;
		}
		base.transform.localPosition = to;
	}
}
