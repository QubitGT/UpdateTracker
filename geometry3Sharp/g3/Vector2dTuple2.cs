namespace g3;

public struct Vector2dTuple2
{
	public Vector2d V0;

	public Vector2d V1;

	public Vector2d this[int key]
	{
		get
		{
			if (key != 0)
			{
				return V1;
			}
			return V0;
		}
		set
		{
			if (key == 0)
			{
				V0 = value;
			}
			else
			{
				V1 = value;
			}
		}
	}

	public Vector2dTuple2(Vector2d v0, Vector2d v1)
	{
		V0 = v0;
		V1 = v1;
	}
}
