using System;
using Modio.Images;
using Modio.Mods;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModProperties;

[Serializable]
public class ModPropertyLogo : IModProperty
{
	[SerializeField]
	private RawImage _image;

	[SerializeField]
	private Mod.LogoResolution _resolution;

	[SerializeField]
	private bool _useHighestAvailableResolutionAsFallback = true;

	[Space]
	[Tooltip("(Optional) Active while loading, inactive once loaded.")]
	[SerializeField]
	private GameObject _loadingActive;

	[Tooltip("(Optional) Inactive while loading, active once loaded.")]
	[SerializeField]
	private GameObject _loadedActive;

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
			}, delegate(bool isLoading)
			{
				if ((bool)_loadingActive)
				{
					_loadingActive.SetActive(isLoading);
				}
				if ((bool)_loadedActive)
				{
					_loadedActive.SetActive(!isLoading);
				}
			});
		}
		_lazyImage.SetImage(mod.Logo, _resolution);
	}
}
