namespace Modio.Mods;

public readonly struct ModId
{
	public readonly long _id;

	public static ModId Null => -1L;

	public ModId(long id)
	{
		_id = id;
	}

	public bool IsValid()
	{
		return _id > 0;
	}

	internal long GetResourceId()
	{
		return _id;
	}

	public static bool operator ==(ModId left, ModId right)
	{
		return left._id == right._id;
	}

	public static bool operator !=(ModId left, ModId right)
	{
		return left._id != right._id;
	}

	public override bool Equals(object obj)
	{
		if (obj is ModId modId)
		{
			return this == modId;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _id.GetHashCode();
	}

	public static implicit operator long(ModId modId)
	{
		return modId._id;
	}

	public static implicit operator ModId(long id)
	{
		return new ModId(id);
	}

	public override string ToString()
	{
		return _id.ToString();
	}
}
