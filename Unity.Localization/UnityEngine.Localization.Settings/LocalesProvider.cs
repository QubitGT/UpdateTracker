using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings;

[Serializable]
public class LocalesProvider : ILocalesProvider, IPreloadRequired, IReset, IDisposable
{
	private readonly List<Locale> m_Locales = new List<Locale>();

	private AsyncOperationHandle m_LoadOperation;

	public List<Locale> Locales
	{
		get
		{
			if (LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode && !PreloadOperation.IsDone)
			{
				PreloadOperation.WaitForCompletion();
			}
			return m_Locales;
		}
	}

	public AsyncOperationHandle PreloadOperation
	{
		get
		{
			if (!m_LoadOperation.IsValid())
			{
				m_Locales.Clear();
				m_LoadOperation = AddressablesInterface.LoadAssetsWithLabel<Locale>("Locale", AddLocale);
			}
			return m_LoadOperation;
		}
	}

	public Locale GetLocale(LocaleIdentifier id)
	{
		foreach (Locale locale in Locales)
		{
			if (!(locale == null) && !(locale is PseudoLocale) && locale.Identifier.Equals(id))
			{
				return locale;
			}
		}
		return FindFallbackLocale(id);
	}

	public Locale GetLocale(string code)
	{
		return GetLocale(new LocaleIdentifier(code));
	}

	public Locale GetLocale(SystemLanguage systemLanguage)
	{
		return GetLocale(new LocaleIdentifier(systemLanguage));
	}

	public void AddLocale(Locale locale)
	{
		if (locale == null)
		{
			return;
		}
		if (!(locale is PseudoLocale))
		{
			foreach (Locale locale2 in m_Locales)
			{
				if (!(locale2 is PseudoLocale) && locale2.Identifier == locale.Identifier)
				{
					Debug.LogWarning($"Ignoring locale {locale}. The locale {locale2} has the same Id `{locale.Identifier}`");
					return;
				}
			}
		}
		int num = m_Locales.BinarySearch(locale);
		if (num < 0)
		{
			m_Locales.Insert(~num, locale);
		}
	}

	public bool RemoveLocale(Locale locale)
	{
		if (locale == null)
		{
			return false;
		}
		bool result = Locales.Remove(locale);
		LocalizationSettings instanceDontCreateDefault = LocalizationSettings.GetInstanceDontCreateDefault();
		if ((object)instanceDontCreateDefault != null)
		{
			instanceDontCreateDefault.OnLocaleRemoved(locale);
			return result;
		}
		return result;
	}

	public Locale FindFallbackLocale(LocaleIdentifier localeIdentifier)
	{
		CultureInfo cultureInfo = localeIdentifier.CultureInfo;
		if (cultureInfo == null)
		{
			return null;
		}
		Locale locale = null;
		cultureInfo = cultureInfo.Parent;
		while (cultureInfo != CultureInfo.InvariantCulture && locale == null)
		{
			locale = GetLocale(cultureInfo);
			cultureInfo = cultureInfo.Parent;
		}
		return locale;
	}

	public void ResetState()
	{
		m_Locales.Clear();
		m_LoadOperation = default(AsyncOperationHandle);
	}

	~LocalesProvider()
	{
		AddressablesInterface.SafeRelease(m_LoadOperation);
	}

	void IDisposable.Dispose()
	{
		m_Locales.Clear();
		AddressablesInterface.SafeRelease(m_LoadOperation);
		GC.SuppressFinalize(this);
	}
}
