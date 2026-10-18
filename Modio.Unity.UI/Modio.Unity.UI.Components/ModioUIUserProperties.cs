using System;
using Modio.Unity.UI.Components.UserProperties;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUIUserProperties : ModioUIPropertiesBase<ModioUIUser, IUserProperty>
{
	[SerializeReference]
	private IUserProperty[] _properties = Array.Empty<IUserProperty>();

	protected override IUserProperty[] Properties => _properties;

	protected override void UpdateProperties()
	{
		IUserProperty[] properties = _properties;
		for (int i = 0; i < properties.Length; i++)
		{
			properties[i].OnUserUpdate(Owner.User);
		}
	}
}
