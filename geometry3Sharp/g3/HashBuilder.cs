namespace g3;

public struct HashBuilder
{
	public int Hash;

	public HashBuilder(int init = -2128831035)
	{
		Hash = init;
	}

	public void Add(int i)
	{
		Hash = (Hash * 16777619) ^ i.GetHashCode();
	}

	public void Add(double d)
	{
		Hash = (Hash * 16777619) ^ d.GetHashCode();
	}

	public void Add(float f)
	{
		Hash = (Hash * 16777619) ^ f.GetHashCode();
	}

	public void Add(Vector2f v)
	{
		Hash = (Hash * 16777619) ^ v.x.GetHashCode();
		Hash = (Hash * 16777619) ^ v.y.GetHashCode();
	}

	public void Add(Vector2d v)
	{
		Hash = (Hash * 16777619) ^ v.x.GetHashCode();
		Hash = (Hash * 16777619) ^ v.y.GetHashCode();
	}

	public void Add(Vector3f v)
	{
		Hash = (Hash * 16777619) ^ v.x.GetHashCode();
		Hash = (Hash * 16777619) ^ v.y.GetHashCode();
		Hash = (Hash * 16777619) ^ v.z.GetHashCode();
	}

	public void Add(Vector3d v)
	{
		Hash = (Hash * 16777619) ^ v.x.GetHashCode();
		Hash = (Hash * 16777619) ^ v.y.GetHashCode();
		Hash = (Hash * 16777619) ^ v.z.GetHashCode();
	}

	public void Add(Frame3f f)
	{
		Hash = (Hash * 16777619) ^ f.Origin.x.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Origin.y.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Origin.z.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Rotation.x.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Rotation.y.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Rotation.z.GetHashCode();
		Hash = (Hash * 16777619) ^ f.Rotation.w.GetHashCode();
	}

	public void Add(Index3i v)
	{
		Hash = (Hash * 16777619) ^ v.a.GetHashCode();
		Hash = (Hash * 16777619) ^ v.b.GetHashCode();
		Hash = (Hash * 16777619) ^ v.c.GetHashCode();
	}

	public void Add(Index2i v)
	{
		Hash = (Hash * 16777619) ^ v.a.GetHashCode();
		Hash = (Hash * 16777619) ^ v.b.GetHashCode();
	}
}
