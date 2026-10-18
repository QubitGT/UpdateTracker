using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Input;

public class ModioUIScrollViewControllerInput : Selectable
{
	private ScrollRect _scrollRect;

	private readonly PointerEventData _cachedPointerEventData = new PointerEventData(null);

	[SerializeField]
	private float _inputSpeed = 100f;

	[SerializeField]
	private bool _resetPositionOnEnable = true;

	protected override void Awake()
	{
		base.Awake();
		_scrollRect = GetComponent<ScrollRect>();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_resetPositionOnEnable)
		{
			_scrollRect.verticalNormalizedPosition = 1f;
		}
	}

	private void Update()
	{
		_cachedPointerEventData.scrollDelta = ModioUIInput.GetRawCursor() * (_inputSpeed * Time.unscaledDeltaTime);
		_scrollRect.OnScroll(_cachedPointerEventData);
	}
}
