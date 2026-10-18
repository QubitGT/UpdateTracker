using System.Collections.Generic;

namespace Pathfinding.Poly2Tri;

public class TriangulationPoint
{
	public double X;

	public double Y;

	public List<DTSweepConstraint> Edges { get; private set; }

	public float Xf
	{
		get
		{
			return (float)X;
		}
		set
		{
			X = value;
		}
	}

	public float Yf
	{
		get
		{
			return (float)Y;
		}
		set
		{
			Y = value;
		}
	}

	public bool HasEdges => Edges != null;

	public TriangulationPoint(double x, double y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return "[" + X + "," + Y + "]";
	}

	public void AddEdge(DTSweepConstraint e)
	{
		if (Edges == null)
		{
			Edges = new List<DTSweepConstraint>();
		}
		Edges.Add(e);
	}
}
