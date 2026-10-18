using System;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Newtonsoft.Json;

namespace Modio.Mods;

public class GameTagCategory
{
	private static GameTagCategory[] _cachedTags;

	public readonly string Name;

	public readonly bool MultiSelect;

	public readonly ModTag[] Tags;

	public readonly bool Hidden;

	public readonly bool Locked;

	[JsonConstructor]
	internal GameTagCategory(string name, bool multiSelect, ModTag[] tags, bool hidden, bool locked)
	{
		Name = name;
		MultiSelect = multiSelect;
		Tags = tags;
		Hidden = hidden;
		Locked = locked;
	}

	internal GameTagCategory(GameTagOptionObject tagObject)
	{
		Name = tagObject.Name;
		MultiSelect = tagObject.Type == "checkboxes";
		Hidden = tagObject.Hidden;
		Locked = tagObject.Locked;
		Tags = tagObject.Tags.Select(ModTag.Get).ToArray();
		foreach (var (tagName, count) in tagObject.TagCountMap)
		{
			ModTag.Get(tagName).Count = count;
		}
		if (tagObject.TagsLocalization != null)
		{
			GameTagOptionObject.EmbeddedTagsLocalization[] tagsLocalization = tagObject.TagsLocalization;
			for (int i = 0; i < tagsLocalization.Length; i++)
			{
				GameTagOptionObject.EmbeddedTagsLocalization embeddedTagsLocalization = tagsLocalization[i];
				ModTag.Get(embeddedTagsLocalization.Tag).SetLocalizations(embeddedTagsLocalization.Translations);
			}
		}
	}

	static GameTagCategory()
	{
		ModioClient.OnInitialized += delegate
		{
			_cachedTags = null;
		};
	}

	public static async Task<(Error, GameTagCategory[])> GetGameTagOptions()
	{
		if (_cachedTags != null)
		{
			return (Error.None, _cachedTags);
		}
		var (error, pagination) = await ModioAPI.Tags.GetGameTagOptions();
		if ((bool)error)
		{
			var (error2, gameData) = await ModioClient.DataStorage.ReadGameData();
			if ((bool)error2)
			{
				return (error, Array.Empty<GameTagCategory>());
			}
			_cachedTags = gameData.Categories;
			return (Error.None, _cachedTags);
		}
		_cachedTags = pagination.Value.Data.Select((GameTagOptionObject options) => new GameTagCategory(options)).ToArray();
		GameData gameData2 = new GameData
		{
			Categories = _cachedTags
		};
		await ModioClient.DataStorage.WriteGameData(gameData2);
		return (Error.None, _cachedTags);
	}
}
