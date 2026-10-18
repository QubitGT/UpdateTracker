namespace Unity.Cinemachine;

internal struct LocalMinima
{
	public readonly Vertex vertex;

	public readonly PathType polytype;

	public readonly bool isOpen;

	public LocalMinima(Vertex vertex, PathType polytype, bool isOpen = false)
	{
		this.vertex = vertex;
		this.polytype = polytype;
		this.isOpen = isOpen;
	}

	public static bool operator ==(LocalMinima lm1, LocalMinima lm2)
	{
		return lm1.vertex == lm2.vertex;
	}

	public static bool operator !=(LocalMinima lm1, LocalMinima lm2)
	{
		return !(lm1 == lm2);
	}

	public override bool Equals(object obj)
	{
		if (obj is LocalMinima localMinima)
		{
			return this == localMinima;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return vertex.GetHashCode();
	}
}
