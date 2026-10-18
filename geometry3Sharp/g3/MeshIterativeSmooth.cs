using System;

namespace g3;

public class MeshIterativeSmooth
{
	public enum SmoothTypes
	{
		Uniform,
		Cotan,
		MeanValue
	}

	public DMesh3 Mesh;

	public int[] Vertices;

	public double Alpha = 0.25;

	public int Rounds = 10;

	public SmoothTypes SmoothType;

	public Func<Vector3d, Vector3f, int, Vector3d> ProjectF;

	private Vector3d[] SmoothedPostions;

	public MeshIterativeSmooth(DMesh3 mesh, int[] vertices, bool bOwnVertices = false)
	{
		Mesh = mesh;
		Vertices = (bOwnVertices ? vertices : ((int[])vertices.Clone()));
		SmoothedPostions = new Vector3d[Vertices.Length];
		ProjectF = null;
	}

	public virtual ValidationStatus Validate()
	{
		return ValidationStatus.Ok;
	}

	public virtual bool Smooth()
	{
		int num = Vertices.Length;
		double a = MathUtil.Clamp(Alpha, 0.0, 1.0);
		double num2 = MathUtil.Clamp(Rounds, 0, 10000);
		Func<DMesh3, int, double, Vector3d> smoothFunc = MeshUtil.UniformSmooth;
		if (SmoothType == SmoothTypes.MeanValue)
		{
			smoothFunc = MeshUtil.MeanValueSmooth;
		}
		else if (SmoothType == SmoothTypes.Cotan)
		{
			smoothFunc = MeshUtil.CotanSmooth;
		}
		Action<int> body = delegate(int i)
		{
			int arg2 = Vertices[i];
			SmoothedPostions[i] = smoothFunc(Mesh, arg2, a);
		};
		Action<int> body2 = delegate(int i)
		{
			Vector3d arg = SmoothedPostions[i];
			SmoothedPostions[i] = ProjectF(arg, Vector3f.AxisY, Vertices[i]);
		};
		IndexRangeEnumerator source = new IndexRangeEnumerator(0, num);
		for (int j = 0; (double)j < num2; j++)
		{
			gParallel.ForEach(source, body);
			if (ProjectF != null)
			{
				gParallel.ForEach(source, body2);
			}
			for (int k = 0; k < num; k++)
			{
				Mesh.SetVertex(Vertices[k], SmoothedPostions[k]);
			}
		}
		return true;
	}
}
