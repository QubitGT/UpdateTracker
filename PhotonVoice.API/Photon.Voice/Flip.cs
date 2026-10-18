namespace Photon.Voice;

public struct Flip
{
	public static Flip None;

	public static Flip Vertical = new Flip
	{
		IsVertical = true
	};

	public static Flip Horizontal = new Flip
	{
		IsHorizontal = true
	};

	public static Flip Both = Vertical * Horizontal;

	public bool IsVertical { get; private set; }

	public bool IsHorizontal { get; private set; }

	public static bool operator ==(Flip f1, Flip f2)
	{
		if (f1.IsVertical == f2.IsVertical)
		{
			return f1.IsHorizontal == f2.IsHorizontal;
		}
		return false;
	}

	public static bool operator !=(Flip f1, Flip f2)
	{
		if (f1.IsVertical == f2.IsVertical)
		{
			return f1.IsHorizontal != f2.IsHorizontal;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static Flip operator *(Flip f1, Flip f2)
	{
		Flip result = default(Flip);
		result.IsVertical = f1.IsVertical != f2.IsVertical;
		result.IsHorizontal = f1.IsHorizontal != f2.IsHorizontal;
		return result;
	}
}
