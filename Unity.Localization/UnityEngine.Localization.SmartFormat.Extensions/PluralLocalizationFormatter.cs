using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.SmartFormat.Utilities;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.SmartFormat.Extensions;

[Serializable]
public class PluralLocalizationFormatter : FormatterBase, IFormatterLiteralExtractor
{
	[SerializeField]
	private string m_DefaultTwoLetterISOLanguageName = "en";

	private PluralRules.PluralRuleDelegate m_DefaultPluralRule;

	public string DefaultTwoLetterISOLanguageName
	{
		get
		{
			return m_DefaultTwoLetterISOLanguageName;
		}
		set
		{
			m_DefaultTwoLetterISOLanguageName = value;
			m_DefaultPluralRule = PluralRules.GetPluralRule(value);
		}
	}

	public override string[] DefaultNames => new string[3] { "plural", "p", "" };

	public PluralLocalizationFormatter()
	{
		base.Names = DefaultNames;
	}

	public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		object currentValue = formattingInfo.CurrentValue;
		if (format == null || format.baseString[format.startIndex] == ':')
		{
			return false;
		}
		IList<Format> list = format.Split('|');
		if (list.Count == 1)
		{
			return false;
		}
		decimal value;
		if (currentValue is IConvertible convertible && !(currentValue is DateTime) && !(currentValue is string) && !(currentValue is bool) && !(currentValue is Enum))
		{
			value = convertible.ToDecimal(null);
		}
		else
		{
			if (!(currentValue is IEnumerable<object> source))
			{
				return false;
			}
			value = source.Count();
		}
		PluralRules.PluralRuleDelegate pluralRule = GetPluralRule(formattingInfo);
		if (pluralRule == null)
		{
			return false;
		}
		int count = list.Count;
		int num = pluralRule(value, count);
		if (num < 0 || list.Count <= num)
		{
			throw new FormattingException(format, "Invalid number of plural parameters", list.Last().endIndex);
		}
		Format format2 = list[num];
		formattingInfo.Write(format2, currentValue);
		return true;
	}

	protected virtual PluralRules.PluralRuleDelegate GetPluralRule(IFormattingInfo formattingInfo)
	{
		string formatterOptions = formattingInfo.FormatterOptions;
		if (formatterOptions.Length != 0)
		{
			return PluralRules.GetPluralRule(formatterOptions);
		}
		IFormatProvider formatProvider = formattingInfo.FormatDetails.Provider;
		CustomPluralRuleProvider customPluralRuleProvider = (CustomPluralRuleProvider)(formatProvider?.GetFormat(typeof(CustomPluralRuleProvider)));
		if (customPluralRuleProvider != null)
		{
			return customPluralRuleProvider.GetPluralRule();
		}
		if (formatProvider is Locale { Identifier: var identifier })
		{
			formatProvider = identifier.CultureInfo;
		}
		if (formatProvider is CultureInfo cultureInfo)
		{
			return PluralRules.GetPluralRule(cultureInfo.TwoLetterISOLanguageName);
		}
		Locale locale2 = null;
		AsyncOperationHandle<Locale> selectedLocaleAsync = LocalizationSettings.SelectedLocaleAsync;
		if (selectedLocaleAsync.IsValid() && selectedLocaleAsync.IsDone)
		{
			locale2 = selectedLocaleAsync.Result;
		}
		if (locale2 != null)
		{
			CultureInfo cultureInfo2 = locale2.Identifier.CultureInfo;
			string twoLetterIsoLanguageName;
			if (cultureInfo2 != null)
			{
				twoLetterIsoLanguageName = cultureInfo2.TwoLetterISOLanguageName;
			}
			else
			{
				twoLetterIsoLanguageName = locale2.Identifier.Code;
				if (locale2.Identifier.Code.Length > 2)
				{
					twoLetterIsoLanguageName = locale2.Identifier.Code.Substring(0, 2);
				}
			}
			return PluralRules.GetPluralRule(twoLetterIsoLanguageName);
		}
		return m_DefaultPluralRule ?? (m_DefaultPluralRule = PluralRules.GetPluralRule(DefaultTwoLetterISOLanguageName));
	}

	public void WriteAllLiterals(IFormattingInfo formattingInfo)
	{
		Format format = formattingInfo.Format;
		if (format == null || format.baseString[format.startIndex] == ':')
		{
			return;
		}
		IList<Format> list = format.Split('|');
		if (list.Count != 1)
		{
			for (int i = 0; i < list.Count; i++)
			{
				formattingInfo.Write(list[i], null);
			}
		}
	}
}
