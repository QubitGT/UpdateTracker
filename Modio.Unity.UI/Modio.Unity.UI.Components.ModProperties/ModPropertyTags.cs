using System.Collections.Generic;
using System.Linq;
using Modio.Mods;
using UnityEngine;

namespace Modio.Unity.UI.Components.ModProperties;

public class ModPropertyTags : IModProperty
{
	[SerializeField]
	private ModioUITag _tagTemplate;

	[SerializeField]
	private GameObject _noTagsActive;

	[SerializeField]
	private GameObject _tagsActive;

	private readonly List<ModioUITag> _tags = new List<ModioUITag>();

	public void OnModUpdate(Mod mod)
	{
		if (!_tags.Any())
		{
			if (_tagTemplate == null)
			{
				bool flag = mod.Tags.Any((ModTag tag) => tag.IsVisible);
				if (_noTagsActive != null)
				{
					_noTagsActive.SetActive(!flag);
				}
				if (_tagsActive != null)
				{
					_tagsActive.SetActive(flag);
				}
				return;
			}
			_tags.Add(_tagTemplate);
		}
		int num = 0;
		ModTag[] tags = mod.Tags;
		foreach (ModTag tag2 in tags)
		{
			if (num >= _tags.Count)
			{
				_tags.Add(Object.Instantiate(_tags[0], _tags[0].transform.parent));
			}
			ModioUITag modioUITag = _tags[num];
			modioUITag.gameObject.SetActive(value: true);
			modioUITag.Set(tag2);
			num++;
		}
		for (int j = num; j < _tags.Count; j++)
		{
			_tags[j].gameObject.SetActive(value: false);
		}
		if (_noTagsActive != null)
		{
			_noTagsActive.SetActive(num == 0);
		}
		if (_tagsActive != null)
		{
			_tagsActive.SetActive(num != 0);
		}
	}
}
