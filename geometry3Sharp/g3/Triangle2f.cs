namespace g3;

public struct Triangle2f
{
	public Vector2f V0;

	public Vector2f V1;

	public Vector2f V2;

	public Vector2f this[int key]
	{
		get
		{
			return key switch
			{
				1 => V1, 
				0 => V0, 
				_ => V2, 
			};
		}
		set
		{
			switch (key)
			{
			case 0:
				V0 = value;
				break;
			case 1:
				V1 = value;
				break;
			default:
				V2 = value;
				break;
			}
		}
	}

	public Triangle2f(Vector2f v0, Vector2f v1, Vector2f v2)
	{
		V0 = v0;
		V1 = v1;
		V2 = v2;
	}

	public Vector2f PointAt(float bary0, float bary1, float bary2)
	{
		return bary0 * V0 + bary1 * V1 + bary2 * V2;
	}

	public Vector2f PointAt(Vector3f bary)
	{
		return bary.x * V0 + bary.y * V1 + bary.z * V2;
	}

	public Vector2f Centroid()
	{
		return PointAt(0.3f, 0.3f, 0.3f);
	}
}
