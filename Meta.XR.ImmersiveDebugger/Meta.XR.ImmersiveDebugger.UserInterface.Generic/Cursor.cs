using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Cursor : OVRCursor
{
	private const float _pressedScale = 0.8f;

	private const float _releasedScale = 1f;

	private Vector3 _forward;

	private Vector3 _endPoint;

	private Vector3 _normal;

	private bool _hit;

	private PointerEventData.FramePressState _pressState = PointerEventData.FramePressState.Released;

	private Canvas _canvas;

	internal GameObject GameObject { get; private set; }

	private Transform Transform { get; set; }

	private void Awake()
	{
		GameObject = base.gameObject;
		GameObject.layer = RuntimeSettings.Instance.PanelLayer;
		_canvas = GameObject.AddComponent<Canvas>();
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 31000;
		CanvasGroup canvasGroup = GameObject.AddComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
		RawImage rawImage = GameObject.AddComponent<RawImage>();
		rawImage.texture = Resources.Load<Texture2D>("Textures/pointer");
		rawImage.rectTransform.sizeDelta = new Vector2(20f, 20f);
		rawImage.raycastTarget = false;
		Transform = base.transform;
	}

	public override void SetCursorStartDest(Vector3 start, Vector3 dest, Vector3 normal)
	{
		_endPoint = dest;
		_normal = normal;
		_hit = true;
	}

	public override void SetCursorRay(Transform t)
	{
		_forward = t.forward;
		_normal = _forward;
		_hit = false;
	}

	public void SetClickState(PointerEventData.FramePressState state)
	{
		if (state == PointerEventData.FramePressState.NotChanged)
		{
			if (_pressState == PointerEventData.FramePressState.PressedAndReleased)
			{
				_pressState = PointerEventData.FramePressState.Released;
			}
		}
		else
		{
			_pressState = state;
		}
	}

	private void LateUpdate()
	{
		if (_hit)
		{
			Transform.position = _endPoint;
			Transform.rotation = Quaternion.LookRotation(_normal, Vector3.up);
			PointerEventData.FramePressState pressState = _pressState;
			bool flag = pressState == PointerEventData.FramePressState.Pressed || pressState == PointerEventData.FramePressState.PressedAndReleased;
			Transform.localScale = Vector3.one * (flag ? 0.8f : 1f);
		}
		else
		{
			GameObject.SetActive(value: false);
		}
	}

	internal void Attach(Panel panel)
	{
		if (!(panel == null))
		{
			GameObject.SetActive(value: true);
			Transform.SetParent(panel.Transform, worldPositionStays: false);
			_canvas.overrideSorting = true;
			_canvas.sortingOrder = 31000;
		}
	}
}
