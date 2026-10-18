using System;

namespace g3;

public class MeshLoopSmooth
{
	public DMesh3 Mesh;

	public EdgeLoop Loop;

	public double Alpha = 0.25;

	public int Rounds = 10;

	public Func<Vector3d, int, Vector3d> ProjectF;

	private Vector3d[] SmoothedPostions;

	public MeshLoopSmooth(DMesh3 mesh, EdgeLoop loop)
	{
		Mesh = mesh;
		Loop = loop;
		SmoothedPostions = new Vector3d[Loop.Vertices.Length];
		ProjectF = null;
	}

	public virtual ValidationStatus Validate()
	{
		return MeshValidation.IsEdgeLoop(Mesh, Loop);
	}

	public virtual bool Smooth()
	{
		int NV = Loop.Vertices.Length;
		double a = MathUtil.Clamp(Alpha, 0.0, 1.0);
		double num = MathUtil.Clamp(Rounds, 0, 10000);
		for (int j = 0; (double)j < num; j++)
		{
			gParallel.ForEach(Interval1i.Range(NV), delegate(int i)
			{
				int vID = Loop.Vertices[(i + 1) % NV];
				Vector3d vertex = Mesh.GetVertex(Loop.Vertices[i]);
				Vector3d vertex2 = Mesh.GetVertex(vID);
				Vector3d vertex3 = Mesh.GetVertex(Loop.Vertices[(i + 2) % NV]);
				Vector3d vector3d2 = (vertex + vertex3) * 0.5;
				SmoothedPostions[i] = (1.0 - a) * vertex2 + a * vector3d2;
			});
			gParallel.ForEach(Interval1i.Range(NV), delegate(int i)
			{
				int num2 = Loop.Vertices[(i + 1) % NV];
				Vector3d vector3d = SmoothedPostions[i];
				if (ProjectF != null)
				{
					vector3d = ProjectF(vector3d, num2);
				}
				Mesh.SetVertex(num2, vector3d);
			});
		}
		return true;
	}
}
