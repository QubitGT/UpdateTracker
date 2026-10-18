namespace g3;

public struct Vector3dTuple2
{
	public Vector3d V0;

	public Vector3d V1;

	public Vector3d this[int key]
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

	public Vector3dTuple2(Vector3d v0, Vector3d v1)
	{
		V0 = v0;
		V1 = v1;
	}
}
