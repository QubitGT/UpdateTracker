using System;
using Meta.XR.ImmersiveDebugger.Hierarchy;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal struct Category : IEquatable<Category>
{
	private const string DefaultCategoryName = "Uncategorized";

	public static Category Default;

	public string Id;

	public Item Item;

	public string Label
	{
		get
		{
			object obj = Item?.Label;
			if (obj == null)
			{
				if (!string.IsNullOrEmpty(Id))
				{
					return Id;
				}
				obj = "Uncategorized";
			}
			return (string)obj;
		}
	}

	private string Uid => (Item?.Id.ToString() ?? Id) ?? string.Empty;

	public bool Equals(Category other)
	{
		return Uid == other.Uid;
	}

	public override bool Equals(object obj)
	{
		if (obj is Category other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Uid.GetHashCode();
	}
}
