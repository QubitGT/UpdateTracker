using System;
using Modio.Unity.UI.Components.SearchProperties;
using Modio.Unity.UI.Search;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUISearchProperties : ModioUIPropertiesBase<ModioUISearch, ISearchProperty>
{
	[SerializeReference]
	private ISearchProperty[] _properties = Array.Empty<ISearchProperty>();

	protected override ISearchProperty[] Properties => _properties;

	protected override void UpdateProperties()
	{
		ISearchProperty[] properties = _properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].OnSearchUpdate(Owner);
		}
	}
}
