using System;
using System.Collections.Generic;

namespace Modio.Images;

[Serializable]
public struct ImageReference : IEquatable<ImageReference>
{
	private sealed class UrlEqualityComparer : IEqualityComparer<ImageReference>
	{
		public bool Equals(ImageReference x, ImageReference y)
		{
			return x.Url == y.Url;
		}

		public int GetHashCode(ImageReference obj)
		{
			if (obj.Url == null)
			{
				return 0;
			}
			return obj.Url.GetHashCode();
		}
	}

	public bool IsValid => !string.IsNullOrWhiteSpace(Url);

	public string Url { get; private set; }

	internal ImageReference(string url)
	{
		Url = url;
	}

	public static bool operator ==(ImageReference left, ImageReference right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ImageReference left, ImageReference right)
	{
		return !left.Equals(right);
	}

	public bool Equals(ImageReference other)
	{
		return Url == other.Url;
	}

	public override bool Equals(object obj)
	{
		if (obj is ImageReference other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (Url == null)
		{
			return 0;
		}
		return Url.GetHashCode();
	}
}
