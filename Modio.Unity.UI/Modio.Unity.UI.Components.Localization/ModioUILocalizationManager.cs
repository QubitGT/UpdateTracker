using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Modio.API;
using UnityEngine;

namespace Modio.Unity.UI.Components.Localization;

public class ModioUILocalizationManager : MonoBehaviour
{
	public delegate string LocalizationHandler(string key, string isoLanguageCode);

	private static LocalizationHandler customLocalizationHandler;

	private static string _languageCode;

	private static List<Dictionary<string, string>> _languageTables;

	private static Dictionary<string, string> _currentTable;

	[SerializeField]
	private TextAsset _locTable;

	[SerializeField]
	private bool _setCurrentSystemCulture = true;

	public static bool LocalizationExists
	{
		get
		{
			if (customLocalizationHandler == null)
			{
				if (_languageTables != null)
				{
					return _languageTables.Count > 0;
				}
				return false;
			}
			return true;
		}
	}

	public static bool LocalizationReady
	{
		get
		{
			if (customLocalizationHandler == null)
			{
				return _currentTable != null;
			}
			return true;
		}
	}

	public static CultureInfo CultureInfo { get; private set; } = new CultureInfo("en");


	public static event Action LanguageSet
	{
		add
		{
			LanguageSetInternal += value;
			if (_currentTable != null || customLocalizationHandler != null)
			{
				value();
			}
		}
		remove
		{
			LanguageSetInternal -= value;
		}
	}

	private static event Action LanguageSetInternal;

	public static void SetCustomHandler(LocalizationHandler handler)
	{
		customLocalizationHandler = handler;
		if (customLocalizationHandler != null)
		{
			ModioUILocalizationManager.LanguageSetInternal?.Invoke();
		}
	}

	public void SetLanguageCode(string isoCode)
	{
		if (string.IsNullOrEmpty(isoCode))
		{
			isoCode = "en";
		}
		_languageCode = isoCode;
		try
		{
			CultureInfo = new CultureInfo(isoCode);
		}
		catch (CultureNotFoundException)
		{
			ModioLog.Warning?.Log("Language code " + isoCode + " not found by CultureInfo. Using default culture.");
			CultureInfo = new CultureInfo("en");
		}
		if (_setCurrentSystemCulture)
		{
			CultureInfo.CurrentCulture = CultureInfo;
		}
		if (_languageTables == null)
		{
			return;
		}
		foreach (Dictionary<string, string> languageTable in _languageTables)
		{
			if (languageTable.TryGetValue("modio_languagecode", out var value) && isoCode == value)
			{
				_currentTable = languageTable;
				ModioUILocalizationManager.LanguageSetInternal?.Invoke();
				break;
			}
		}
	}

	public static string GetLocalizedText(string key, bool errorIfMissing = true)
	{
		if (customLocalizationHandler != null)
		{
			return customLocalizationHandler(key, _languageCode);
		}
		if (_currentTable == null)
		{
			if (!errorIfMissing)
			{
				return null;
			}
			return key;
		}
		if (_currentTable.TryGetValue(key, out var value))
		{
			return value;
		}
		if (!errorIfMissing)
		{
			return null;
		}
		Debug.LogError("Missing localized key " + key + " for language " + _languageCode);
		return "MISSING KEY " + key;
	}

	private void Awake()
	{
		string[] array = _locTable.text.Split('\n');
		_languageTables = null;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string input = text;
			if (text.EndsWith("\r"))
			{
				input = text.Substring(0, text.Length - 1);
			}
			string pattern = "(?:,\"|^\")(\"\"|[\\w\\W]*?)(?=\",|\"$)|(?:,(?!\")|^(?!\"))([^,]*?)(?=$|,)|(\\r\\n|\\n)";
			string[] array3 = Regex.Split(input, pattern);
			if (array3.Length < 2)
			{
				continue;
			}
			string key = array3[1];
			if (_languageTables == null)
			{
				_languageTables = new List<Dictionary<string, string>>();
				for (int j = 1; j < array3.Length / 2; j++)
				{
					_languageTables.Add(new Dictionary<string, string> { 
					{
						key,
						array3[j * 2 + 1]
					} });
				}
			}
			else
			{
				for (int k = 1; k * 2 + 1 < array3.Length && k - 1 < _languageTables.Count; k++)
				{
					_languageTables[k - 1].Add(key, array3[k * 2 + 1]);
				}
			}
		}
		ModioClient.OnInitialized += OnPluginInitialized;
	}

	private void OnDestroy()
	{
		ModioClient.OnInitialized -= OnPluginInitialized;
	}

	private void OnPluginInitialized()
	{
		SetLanguageCode(ModioAPI.LanguageCodeResponse);
	}
}
