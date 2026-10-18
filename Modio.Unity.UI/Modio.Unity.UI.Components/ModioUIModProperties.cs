using System;
using Modio.Unity.UI.Components.ModProperties;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUIModProperties : ModioUIPropertiesBase<ModioUIMod, IModProperty>
{
	[SerializeReference]
	private IModProperty[] _properties = Array.Empty<IModProperty>();

	protected override IModProperty[] Properties => _properties;

	protected override void UpdateProperties()
	{
		if (Owner.Mod != null)
		{
			IModProperty[] properties = _properties;
			for (int i = 0; i < properties.Length; i++)
			{
				properties[i].OnModUpdate(Owner.Mod);
			}
		}
	}
}
