namespace g3;

public struct Vector2dTuple4
{
	public Vector2d V0;

	public Vector2d V1;

	public Vector2d V2;

	public Vector2d V3;

	public Vector2d this[int key]
	{
		get
		{
			if (key <= 1)
			{
				if (key != 1)
				{
					return V0;
				}
				return V1;
			}
			if (key != 2)
			{
				return V3;
			}
			return V2;
		}
		set
		{
			if (key > 1)
			{
				if (key == 2)
				{
					V2 = value;
				}
				else
				{
					V3 = value;
				}
			}
			else if (key == 1)
			{
				V0 = value;
			}
			else
			{
				V1 = value;
			}
		}
	}

	public Vector2dTuple4(Vector2d v0, Vector2d v1, Vector2d v2, Vector2d v3)
	{
		V0 = v0;
		V1 = v1;
		V2 = v2;
		V3 = v3;
	}
}
