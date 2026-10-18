using System;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Panel : InteractableController
{
	private static OVRHapticsClip _hapticsClip;

	protected Canvas _canvas;

	private CanvasScaler _canvasScaler;

	private PanelRaycaster _ovrRaycaster;

	protected Background Background;

	protected ImageStyle _backgroundStyle;

	private Vector3 _sphericalCoordinates = new Vector3(1f, 0f, 0f);

	private static OVRHapticsClip HapticsClip
	{
		get
		{
			if (OVRHaptics.Config.SampleSizeInBytes == 0)
			{
				return null;
			}
			return _hapticsClip ?? (_hapticsClip = new OVRHapticsClip(new byte[5] { 10, 20, 40, 60, 40 }, 5));
		}
	}

	internal float PixelsPerUnit { get; private set; }

	internal bool Initialised { get; private set; }

	public Vector3 SphericalCoordinates
	{
		get
		{
			return _sphericalCoordinates;
		}
		set
		{
			_sphericalCoordinates = value;
			Vector3 position = SphericalToCartesian(_sphericalCoordinates.x, _sphericalCoordinates.y, _sphericalCoordinates.z);
			SetPosition(position);
		}
	}

	internal Interface Interface => base.Owner as Interface;

	public ImageStyle BackgroundStyle
	{
		set
		{
			_backgroundStyle = value;
			Background.Sprite = value.sprite;
			Background.Color = value.color;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_hapticsClip = null;
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		Hide();
		_canvas = base.GameObject.AddComponent<Canvas>();
		_canvasScaler = base.GameObject.AddComponent<CanvasScaler>();
		Background = Append<Background>("background");
		Background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		Background.RaycastTarget = true;
		Initialised = true;
	}

	protected void SetExpectedPixelsPerUnit(float pixelsPerUnit, float dynamicPixelsPerUnit, float referencePixelsPerUnit)
	{
		PixelsPerUnit = pixelsPerUnit;
		_canvasScaler.dynamicPixelsPerUnit = dynamicPixelsPerUnit;
		_canvasScaler.referencePixelsPerUnit = referencePixelsPerUnit;
		base.Transform.localScale = Vector3.one / PixelsPerUnit;
	}

	private void SetPosition(Vector3 position)
	{
		base.Transform.localPosition = position;
		base.Transform.rotation = Quaternion.LookRotation(base.Transform.position - base.Owner.Transform.position, Vector3.up);
	}

	private static Vector3 SphericalToCartesian(float radius, float theta, float phi)
	{
		theta = MathF.PI / 2f - theta;
		phi = MathF.PI / 2f - phi;
		float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
		float z = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
		float y = radius * Mathf.Cos(phi);
		return new Vector3(x, y, z);
	}

	protected override void OnTransparencyChanged()
	{
		base.OnTransparencyChanged();
		Background.Color = (base.Transparent ? _backgroundStyle.colorOff : _backgroundStyle.color);
	}

	protected override void OnHoverChanged()
	{
		base.OnHoverChanged();
		if (base.Hover)
		{
			PlayHaptics(HapticsClip);
			Interface.Cursor.Attach(this);
		}
	}

	private void RefreshCanvas()
	{
		if (_canvas.worldCamera != Interface.Camera)
		{
			_canvas.worldCamera = Interface.Camera;
		}
	}

	private void RefreshRaycaster()
	{
		if (!_ovrRaycaster && (bool)_canvas.worldCamera)
		{
			_ovrRaycaster = base.GameObject.AddComponent<PanelRaycaster>();
			_ovrRaycaster.pointer = Interface.Cursor.GameObject;
		}
	}

	private void LateUpdate()
	{
		RefreshCanvas();
		RefreshRaycaster();
	}

	protected virtual void OnEnable()
	{
		Telemetry.OnPanelActiveStateChanged(this);
	}

	protected override void OnDisable()
	{
		Telemetry.OnPanelActiveStateChanged(this);
		base.OnDisable();
	}
}
