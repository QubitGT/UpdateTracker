using System;
using Modio.Images;
using Modio.Mods;
using Modio.Users;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyCreatorAvatar : IModProperty
{
	[SerializeField]
	private RawImage _image;

	[SerializeField]
	private UserProfile.AvatarResolution _resolution;

	private LazyImage<Texture2D> _lazyImage;

	public void OnModUpdate(Mod mod)
	{
		if (_lazyImage == null)
		{
			_lazyImage = new LazyImage<Texture2D>(ImageCacheTexture2D.Instance, delegate(Texture2D texture2D)
			{
				if (_image != null)
				{
					_image.texture = texture2D;
				}
			});
		}
		_lazyImage.SetImage(mod.Creator.Avatar, _resolution);
	}
}
