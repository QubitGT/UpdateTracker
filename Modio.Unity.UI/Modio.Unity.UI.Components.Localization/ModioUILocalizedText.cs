using System;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.Localization;

public class ModioUILocalizedText : MonoBehaviour
{
	[SerializeField]
	private string _key;

	[SerializeField]
	private TMP_Text _tmpText;

	[SerializeField]
	private TMP_Text[] _splitFormatArgs;

	private object[] _args;

	private string _initialKey;

	private void Reset()
	{
		_tmpText = GetComponent<TMP_Text>();
	}

	private void OnEnable()
	{
		ModioUILocalizationManager.LanguageSet += UpdateText;
	}

	private void OnDisable()
	{
		ModioUILocalizationManager.LanguageSet -= UpdateText;
	}

	public void SetFormatArgs(params object[] args)
	{
		_args = args;
		UpdateText();
	}

	private void UpdateText()
	{
		if (string.IsNullOrEmpty(_key))
		{
			return;
		}
		string text = ModioUILocalizationManager.GetLocalizedText(_key);
		TMP_Text[] splitFormatArgs = _splitFormatArgs;
		if (splitFormatArgs != null && splitFormatArgs.Length != 0)
		{
			string[] array = text.Split(new string[1] { "{0}" }, StringSplitOptions.None);
			_splitFormatArgs[0].text = ((array.Length != 0) ? array[0] : "");
			if (_splitFormatArgs.Length > 2)
			{
				_splitFormatArgs[2].text = ((array.Length > 1) ? array[1] : "");
			}
			if (_splitFormatArgs.Length > 1)
			{
				TMP_Text obj = _splitFormatArgs[1];
				object[] args = _args;
				obj.text = ((args == null || args.Length == 0) ? "" : _args[0]?.ToString());
			}
		}
		else if (_tmpText != null)
		{
			if (_args != null)
			{
				text = string.Format(text, _args);
			}
			_tmpText.text = text;
		}
	}

	public bool SetKeyIfItExists(string key)
	{
		if (!string.IsNullOrEmpty(ModioUILocalizationManager.GetLocalizedText(key, errorIfMissing: false)))
		{
			SetKey(key);
			return true;
		}
		return false;
	}

	public void SetKey(string key)
	{
		if (string.IsNullOrEmpty(_initialKey))
		{
			_initialKey = _key;
		}
		_key = key;
		UpdateText();
	}

	public void ResetKey()
	{
		if (!string.IsNullOrEmpty(_initialKey))
		{
			SetKey(_initialKey);
		}
	}

	public void SetKey(string key, params object[] args)
	{
		_args = args;
		SetKey(key);
	}
}
