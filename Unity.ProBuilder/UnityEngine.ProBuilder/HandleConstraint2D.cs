namespace UnityEngine.ProBuilder;

internal sealed class HandleConstraint2D
{
	public int x;

	public int y;

	public static readonly HandleConstraint2D None = new HandleConstraint2D(1, 1);

	public HandleConstraint2D(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public HandleConstraint2D Inverse()
	{
		return new HandleConstraint2D((x != 1) ? 1 : 0, (y != 1) ? 1 : 0);
	}

	public Vector2 Mask(Vector2 v)
	{
		v.x *= x;
		v.y *= y;
		return v;
	}

	public Vector2 InverseMask(Vector2 v)
	{
		v.x *= ((x == 1) ? 0f : 1f);
		v.y *= ((y == 1) ? 0f : 1f);
		return v;
	}

	public static bool operator ==(HandleConstraint2D a, HandleConstraint2D b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(HandleConstraint2D a, HandleConstraint2D b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override bool Equals(object o)
	{
		if (o is HandleConstraint2D && ((HandleConstraint2D)o).x == x)
		{
			return ((HandleConstraint2D)o).y == y;
		}
		return false;
	}

	public override string ToString()
	{
		return "(" + x + ", " + y + ")";
	}
}
