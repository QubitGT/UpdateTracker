using System.Collections;
using System.Threading.Tasks;
using Modio.Extensions;
using Modio.Unity.UI.Components.Selectables;
using Modio.Unity.UI.Input;
using Modio.Unity.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Modio.Unity.UI.Navigation;

public class ModioInputFieldSelectionWrapper : Selectable, ISubmitHandler, IEventSystemHandler
{
	private TMP_InputField _inputField;

	private LayoutElement _layoutElement;

	private bool _isExpanded;

	[SerializeField]
	private bool _keepFocusOnSubmit;

	[SerializeField]
	private bool _animateSelectionWidth;

	[SerializeField]
	private GameObject _disableWhenCollapsed;

	protected override void Awake()
	{
		base.Awake();
		_inputField = GetComponentInChildren<TMP_InputField>();
		UnityEngine.UI.Navigation navigation = _inputField.navigation;
		navigation.mode = UnityEngine.UI.Navigation.Mode.None;
		_inputField.navigation = navigation;
		_layoutElement = GetComponent<LayoutElement>();
		_inputField.onSelect.AddListener(delegate
		{
			ModioPanelManager.GetInstance().PushFocusSuppression();
			ModioUIInput.AddHandler(ModioUIInput.ModioAction.Cancel, OnPressedCancel);
			UpdateAnimation(gainingFocus: true);
		});
		_inputField.onDeselect.AddListener(delegate
		{
			DelayPopFocusSuppression().ForgetTaskSafely();
		});
		_inputField.onEndEdit.AddListener(OnEndEdit);
		_inputField.onValueChanged.AddListener(delegate(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				UpdateAnimation();
			}
		});
		if (_disableWhenCollapsed != null)
		{
			_disableWhenCollapsed.SetActive(value: false);
		}
		async Task DelayPopFocusSuppression()
		{
			await Task.Yield();
			ModioPanelManager.GetInstance().PopFocusSuppression(ModioPanelBase.GainedFocusCause.InputSuppressionChangeOnly);
			ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Cancel, OnPressedCancel);
			UpdateAnimation();
		}
	}

	protected override void OnDestroy()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.Cancel, OnPressedCancel);
		base.OnDestroy();
	}

	private void OnEndEdit(string s)
	{
		if (EventSystem.current.currentSelectedGameObject == _inputField.gameObject)
		{
			if (!_keepFocusOnSubmit)
			{
				ModioPanelManager.GetInstance().PopFocusSuppression(ModioPanelBase.GainedFocusCause.RegainingFocusFromStackedPanel);
			}
			else if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject);
			}
		}
	}

	private void OnPressedCancel()
	{
		if (EventSystem.current.currentSelectedGameObject == _inputField.gameObject)
		{
			_inputField.OnDeselect(null);
			UpdateSelectedVisuals(selected: true);
		}
	}

	private void UpdateAnimation(bool gainingFocus = false)
	{
		if (_animateSelectionWidth)
		{
			bool flag = _inputField.isFocused || gainingFocus || _inputField.text.Length > 0;
			if (_isExpanded != flag)
			{
				_isExpanded = flag;
				StartCoroutine(Animate(flag));
			}
		}
	}

	private IEnumerator Animate(bool hasFocus)
	{
		float startWidth = _layoutElement.flexibleWidth;
		int targetWidth = (hasFocus ? 40 : 0);
		float duration = 0.3f;
		if (_disableWhenCollapsed != null)
		{
			_disableWhenCollapsed.SetActive(value: true);
		}
		for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
		{
			float t2 = t * t;
			if (!hasFocus)
			{
				t2 = 1f - (1f - t) * (1f - t);
			}
			_layoutElement.flexibleWidth = Mathf.Lerp(startWidth, targetWidth, t2);
			yield return null;
		}
		_layoutElement.flexibleWidth = targetWidth;
		if (_disableWhenCollapsed != null)
		{
			_disableWhenCollapsed.SetActive(hasFocus);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		UpdateSelectedVisuals(selected: true);
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		UpdateSelectedVisuals(selected: false);
	}

	private void UpdateSelectedVisuals(bool selected)
	{
		if (_inputField is ModioUIInputField modioUIInputField)
		{
			IModioUISelectable.SelectionState state = (selected ? IModioUISelectable.SelectionState.Selected : IModioUISelectable.SelectionState.Normal);
			modioUIInputField.DoVisualOnlyStateTransition(state, instant: false);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		SelectInputField();
	}

	public void SelectInputField()
	{
		StartCoroutine(SelectChildDelayed());
	}

	private IEnumerator SelectChildDelayed()
	{
		yield return new WaitForEndOfFrame();
		_inputField.Select();
	}
}
