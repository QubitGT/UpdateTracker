using System.Collections.Generic;
using System.Linq;
using Modio.Images;
using Modio.Mods;
using Modio.Unity.UI.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.ModGallery;

public class ModioUIModGallery : ModioUIModProperties
{
	[SerializeField]
	private RawImage _image;

	[SerializeField]
	private Mod.GalleryResolution _resolution = Mod.GalleryResolution.X1280_Y720;

	[SerializeField]
	private bool _useHighestAvailableResolutionAsFallback = true;

	[SerializeField]
	private ModioUIModGalleryPagination _paginationTemplate;

	[SerializeField]
	private int _max = 10;

	[SerializeField]
	private bool _wrap = true;

	[Space]
	[Tooltip("(Optional) Active while loading, inactive once loaded.")]
	[SerializeField]
	private GameObject _loadingActive;

	[Tooltip("(Optional) Inactive while loading, active once loaded.")]
	[SerializeField]
	private GameObject _loadedActive;

	private Mod _mod;

	private int _galleryCount;

	private int _index;

	private readonly List<ModioUIModGalleryPagination> _pagination = new List<ModioUIModGalleryPagination>();

	private LazyImage<Texture2D> _lazyImage;

	protected override void Awake()
	{
		base.Awake();
		if (_paginationTemplate != null)
		{
			_paginationTemplate.Gallery = this;
			_pagination.Add(_paginationTemplate);
		}
	}

	private void OnDisable()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.TabLeft, Prev);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.TabRight, Next);
	}

	protected override void UpdateProperties()
	{
		if (Owner.Mod != null)
		{
			if (Owner.Mod != _mod)
			{
				SetMod(Owner.Mod);
			}
			UpdateTabListener();
			GoTo(_index);
		}
	}

	private void SetMod(Mod mod)
	{
		_mod = mod;
		_galleryCount = Mathf.Min(mod.Gallery.Length, _max);
		_index = 0;
		if (_pagination.Any())
		{
			for (int i = _pagination.Count; i < _galleryCount; i++)
			{
				ModioUIModGalleryPagination modioUIModGalleryPagination = Object.Instantiate(_pagination[0], _pagination[0].transform.parent);
				modioUIModGalleryPagination.Gallery = this;
				modioUIModGalleryPagination.Index = i;
				_pagination.Add(modioUIModGalleryPagination);
			}
			for (int j = 0; j < _pagination.Count; j++)
			{
				_pagination[j].gameObject.SetActive(_galleryCount > 1 && j < _galleryCount);
			}
		}
	}

	private void UpdateTabListener()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.TabLeft, Prev);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.TabRight, Next);
		if (_galleryCount > 1)
		{
			ModioUIInput.AddHandler(ModioUIInput.ModioAction.TabLeft, Prev);
			ModioUIInput.AddHandler(ModioUIInput.ModioAction.TabRight, Next);
		}
	}

	public void GoTo(int index)
	{
		index = ((_galleryCount != 0) ? ((index + _galleryCount) % _galleryCount) : 0);
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
		if (_galleryCount > 0)
		{
			_lazyImage.SetImage(_mod.Gallery[index], _resolution);
		}
		else
		{
			_lazyImage.SetImage(_mod.Logo, (Mod.LogoResolution)_resolution);
		}
		if (_pagination.Any())
		{
			for (int i = 0; i < _galleryCount; i++)
			{
				_pagination[i].SetState(i == index);
			}
		}
		_index = index;
	}

	public void Prev()
	{
		if (_wrap || _index > 0)
		{
			GoTo(_index - 1);
		}
	}

	public void Next()
	{
		if (_wrap || _index < _galleryCount - 1)
		{
			GoTo(_index + 1);
		}
	}
}
