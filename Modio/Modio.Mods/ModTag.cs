using System.Collections.Generic;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Newtonsoft.Json;

namespace Modio.Mods;

public class ModTag
{
	private static readonly Dictionary<string, ModTag> Tags = new Dictionary<string, ModTag>();

	public readonly string ApiName;

	private Dictionary<string, string> _translations;

	public string NameLocalized { get; private set; }

	public bool IsVisible { get; private set; }

	public int Count { get; internal set; }

	private ModTag(string apiName)
	{
		ApiName = apiName;
	}

	[JsonConstructor]
	public ModTag(string apiName, Dictionary<string, string> translations, string nameLocalized, bool isVisible, int count)
	{
		ApiName = apiName;
		_translations = translations;
		NameLocalized = nameLocalized;
		IsVisible = isVisible;
		Count = count;
	}

	internal static ModTag Get(ModTagObject modTag)
	{
		if (Tags.TryGetValue(modTag.Name, out var value))
		{
			value.NameLocalized = modTag.NameLocalized;
			return value;
		}
		value = new ModTag(modTag.Name)
		{
			NameLocalized = modTag.NameLocalized
		};
		Tags.Add(modTag.Name, value);
		return value;
	}

	internal static ModTag Get(string tagName)
	{
		if (Tags.TryGetValue(tagName, out var value))
		{
			return value;
		}
		value = new ModTag(tagName)
		{
			NameLocalized = tagName
		};
		Tags.Add(tagName, value);
		return value;
	}

	public void SetLocalizations(Dictionary<string, string> translations)
	{
		_translations = translations;
		if (translations.TryGetValue(ModioAPI.LanguageCodeResponse, out var value))
		{
			NameLocalized = value;
		}
	}
}
