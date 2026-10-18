using UnityEngine;

namespace Oculus.Interaction.Samples.PalmMenu;

public class PalmMenuExample : MonoBehaviour
{
	[SerializeField]
	private PokeInteractable _menuInteractable;

	[SerializeField]
	private GameObject _menuParent;

	[SerializeField]
	private RectTransform _menuPanel;

	[SerializeField]
	private RectTransform[] _buttons;

	[SerializeField]
	private RectTransform[] _paginationDots;

	[SerializeField]
	private RectTransform _selectionIndicatorDot;

	[SerializeField]
	private AnimationCurve _paginationButtonScaleCurve;

	[SerializeField]
	private float _defaultButtonDistance = 50f;

	[SerializeField]
	private AudioSource _paginationSwipeAudio;

	[SerializeField]
	private AudioSource _showMenuAudio;

	[SerializeField]
	private AudioSource _hideMenuAudio;

	private int _currentSelectedButtonIdx;

	private void Start()
	{
		_currentSelectedButtonIdx = CalculateNearestButtonIdx();
		_selectionIndicatorDot.position = _paginationDots[_currentSelectedButtonIdx].position;
	}

	private void Update()
	{
		int num = CalculateNearestButtonIdx();
		if (num != _currentSelectedButtonIdx)
		{
			_currentSelectedButtonIdx = num;
			_paginationSwipeAudio.Play();
			_selectionIndicatorDot.position = _paginationDots[_currentSelectedButtonIdx].position;
		}
		if (_menuInteractable.State != InteractableState.Select)
		{
			LerpToButton();
		}
	}

	private int CalculateNearestButtonIdx()
	{
		int result = 0;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _buttons.Length; i++)
		{
			float num2 = _buttons[i].localPosition.x + _menuPanel.anchoredPosition.x;
			int num3 = ((num2 < 0f) ? (i + 1) : (i - 1));
			float num4 = Mathf.Abs(num2);
			if (num4 < num)
			{
				result = i;
				num = num4;
			}
			float num5 = _defaultButtonDistance;
			if (num3 >= 0 && num3 < _buttons.Length)
			{
				num5 = Mathf.Abs(_buttons[num3].localPosition.x - _buttons[i].localPosition.x);
			}
			float num6 = _paginationButtonScaleCurve.Evaluate(num4 / num5);
			_buttons[i].localScale = num6 * Vector3.one;
		}
		return result;
	}

	private void LerpToButton()
	{
		float num = _buttons[0].localPosition.x;
		float num2 = Mathf.Abs(num + _menuPanel.anchoredPosition.x);
		for (int i = 1; i < _buttons.Length; i++)
		{
			float x = _buttons[i].localPosition.x;
			float num3 = Mathf.Abs(x + _menuPanel.anchoredPosition.x);
			if (num3 < num2)
			{
				num = x;
				num2 = num3;
			}
		}
		_menuPanel.anchoredPosition = Vector2.Lerp(_menuPanel.anchoredPosition, new Vector2(0f - num, 0f), 0.2f);
	}

	public void ToggleMenu()
	{
		if (_menuParent.activeSelf)
		{
			_hideMenuAudio.Play();
			_menuParent.SetActive(value: false);
		}
		else
		{
			_showMenuAudio.Play();
			_menuParent.SetActive(value: true);
		}
	}
}
