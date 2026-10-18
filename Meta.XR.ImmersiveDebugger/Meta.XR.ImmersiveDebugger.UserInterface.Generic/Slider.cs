using Meta.XR.ImmersiveDebugger.Manager;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Slider : Button, IDragHandler, IEventSystemHandler, IInitializePotentialDragHandler
{
	private Background _emptyBackground;

	private Background _fillBackground;

	private Icon _pill;

	private ImageStyle _emptyBackgroundStyle;

	private ImageStyle _fillBackgroundStyle;

	internal Tweak Tweak { get; set; }

	public ImageStyle EmptyBackgroundStyle
	{
		set
		{
			_emptyBackgroundStyle = value;
			_emptyBackground.Sprite = value.sprite;
			_emptyBackground.Color = value.color;
			_emptyBackground.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	public ImageStyle FillBackgroundStyle
	{
		set
		{
			_fillBackgroundStyle = value;
			_fillBackground.Sprite = value.sprite;
			_fillBackground.Color = value.color;
			_fillBackground.PixelDensityMultiplier = value.pixelDensityMultiplier;
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		Background background = Append<Background>("raycast_background");
		background.LayoutStyle = Style.Load<LayoutStyle>("Fill");
		background.Color = new Color(0f, 0f, 0f, 0f);
		background.Sprite = null;
		_emptyBackground = Append<Background>("empty_background");
		_emptyBackground.LayoutStyle = Style.Load<LayoutStyle>("SliderBackground");
		_emptyBackground.RaycastTarget = true;
		_fillBackground = Append<Background>("fill_background");
		_fillBackground.LayoutStyle = Style.Load<LayoutStyle>("SliderFill");
		_pill = Append<Icon>("pill");
		_pill.LayoutStyle = Style.Load<LayoutStyle>("SliderPill");
		_pill.Texture = Resources.Load<Texture2D>("Textures/icon_background_02");
		_pill.Color = Color.white;
		_pill.RaycastTarget = true;
	}

	private void UpdatePillPosition()
	{
		Tweak tweak = Tweak;
		if (tweak != null && tweak.Valid)
		{
			float width = base.RectTransform.rect.width;
			float num = Tweak.Tween * width;
			_pill.RectTransform.anchoredPosition = new Vector2(num, 0f);
			_fillBackground.RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		}
	}

	private void Update()
	{
		UpdatePillPosition();
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (MayDrag(eventData) && RectTransformUtility.ScreenPointToLocalPointInRectangle(base.RectTransform, eventData.position, eventData.enterEventCamera, out var localPoint))
		{
			Rect rect = base.RectTransform.rect;
			Tweak.Tween = Mathf.InverseLerp(rect.min.x, rect.max.x, localPoint.x);
		}
	}

	private bool MayDrag(PointerEventData eventData)
	{
		if (Tweak != null)
		{
			return eventData.button == PointerEventData.InputButton.Left;
		}
		return false;
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		eventData.useDragThreshold = false;
	}
}
