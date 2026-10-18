using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Modio.Unity.UI.Input;

public class ModioUIInputPrompt : MonoBehaviour
{
	[SerializeField]
	private ModioUIInput.ModioAction _action;

	[FormerlySerializedAs("_text")]
	[SerializeField]
	private TMP_Text _inputPromptText;

	[SerializeField]
	private Image _textBackground;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private bool _hideIfNoBindings;

	[SerializeField]
	private bool _hideIfNoListener;

	[SerializeField]
	private bool _hideIfController;

	[SerializeField]
	private bool _hideIfNotController;

	[SerializeField]
	private GameObject[] _additionalToHideIfNoBindings;

	private Button _button;

	private LayoutElement _layoutElement;

	private bool _layoutElementIgnoreLayout;

	private void Awake()
	{
		_button = GetComponent<Button>();
		_layoutElement = GetComponent<LayoutElement>();
		if (_layoutElement != null)
		{
			_layoutElementIgnoreLayout = _layoutElement.ignoreLayout;
		}
	}

	private void OnEnable()
	{
		ModioUIInput.InputPromptDisplayInfo inputPromptDisplayInfo = ModioUIInput.GetInputPromptDisplayInfo(_action);
		inputPromptDisplayInfo.OnUpdated += DisplayInfoUpdated;
		DisplayInfoUpdated(inputPromptDisplayInfo);
	}

	private void OnDisable()
	{
		ModioUIInput.GetInputPromptDisplayInfo(_action).OnUpdated -= DisplayInfoUpdated;
	}

	public void PressedAction()
	{
		ModioUIInput.PressedAction(_action);
	}

	private void DisplayInfoUpdated(ModioUIInput.InputPromptDisplayInfo info)
	{
		if (_hideIfNoListener && !info.InputHasListeners)
		{
			SetElementsVisible(textVisible: false, imageVisible: false);
			return;
		}
		if ((_hideIfController && ModioUIInput.IsUsingGamepad) || (_hideIfNotController && !ModioUIInput.IsUsingGamepad))
		{
			SetElementsVisible(textVisible: false, imageVisible: false);
			return;
		}
		List<Sprite> icons = info.Icons;
		if (icons != null && icons.Count > 0)
		{
			SetElementsVisible(textVisible: false, imageVisible: true);
			_image.sprite = info.Icons[0];
			return;
		}
		List<string> textPrompts = info.TextPrompts;
		if (textPrompts != null && textPrompts.Count > 0)
		{
			SetElementsVisible(textVisible: true, imageVisible: false);
			if (_inputPromptText != null)
			{
				_inputPromptText.text = info.TextPrompts[0];
			}
		}
		else if (_hideIfNoBindings)
		{
			SetElementsVisible(textVisible: false, imageVisible: false);
		}
		else if (!ModioUIInput.AnyBindingsExist)
		{
			SetElementsVisible(textVisible: false, imageVisible: false);
		}
		else
		{
			if (_inputPromptText != null)
			{
				_inputPromptText.text = "UNBOUND";
			}
			SetElementsVisible(textVisible: true, imageVisible: false);
		}
		void SetElementsVisible(bool textVisible, bool imageVisible)
		{
			if (_inputPromptText != null)
			{
				_inputPromptText.gameObject.SetActive(textVisible);
			}
			if (_textBackground != null)
			{
				_textBackground.gameObject.SetActive(textVisible);
			}
			if (_image != null)
			{
				_image.gameObject.SetActive(imageVisible);
			}
			bool flag = textVisible || imageVisible;
			if (_button != null)
			{
				_button.interactable = flag;
			}
			if (_layoutElement != null)
			{
				_layoutElement.ignoreLayout = _layoutElementIgnoreLayout || !flag;
			}
			GameObject[] additionalToHideIfNoBindings = _additionalToHideIfNoBindings;
			for (int i = 0; i < additionalToHideIfNoBindings.Length; i++)
			{
				additionalToHideIfNoBindings[i].SetActive(flag);
			}
		}
	}
}
