using System;
using Modio.Images;
using Modio.Users;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.UserProperties;

[Serializable]
public class UserPropertyCreatorAvatar : IUserProperty
{
	[SerializeField]
	private RawImage _image;

	[SerializeField]
	private UserProfile.AvatarResolution _resolution;

	[SerializeField]
	private bool _useHighestAvailableResolutionAsFallback = true;

	[SerializeField]
	private Texture _noUserImage;

	private LazyImage<Texture2D> _lazyImage;

	public void OnUserUpdate(UserProfile user)
	{
		if (user == null)
		{
			_image.texture = _noUserImage;
			return;
		}
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
		_lazyImage.SetImage(user.Avatar, _resolution);
	}
}
