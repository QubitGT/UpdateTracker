using System;

namespace g3;

public class MeshICP
{
	public IPointSet Source;

	public DMeshAABBTree3 TargetSurface;

	public Vector3d Translation;

	public Quaterniond Rotation;

	public Action<string> VerboseF;

	public int MaxIterations = 50;

	public bool UseNormals;

	public double MaxAllowableDistance = double.MaxValue;

	public double ConvergeTolerance = 1E-05;

	public bool Converged;

	private bool is_initialized;

	private int[] MapV;

	private Vector3d[] From;

	private Vector3d[] To;

	private double[] Weights;

	private double LastError;

	public double Error => LastError;

	public MeshICP(IPointSet source, DMeshAABBTree3 target)
	{
		Source = source;
		TargetSurface = target;
		Translation = Vector3d.Zero;
		Rotation = Quaterniond.Identity;
	}

	public void Solve(bool bUpdate = false)
	{
		if (!bUpdate)
		{
			is_initialized = false;
		}
		if (!is_initialized)
		{
			initialize();
			is_initialized = true;
		}
		update_from();
		update_to();
		LastError = measure_error();
		int num = 0;
		int num2 = 5;
		for (int i = 0; i < MaxIterations; i++)
		{
			if (num >= num2)
			{
				break;
			}
			if (VerboseF != null)
			{
				VerboseF($"[ICP] iter {i} : error {LastError}");
			}
			update_transformation();
			update_from();
			update_to();
			double num3 = measure_error();
			if (Math.Abs(LastError - num3) < ConvergeTolerance)
			{
				num++;
				continue;
			}
			LastError = num3;
			num = 0;
		}
		Converged = num >= num2;
	}

	public void UpdateVertices(IDeformableMesh target)
	{
		bool hasVertexNormals = target.HasVertexNormals;
		update_from();
		foreach (int item in target.VertexIndices())
		{
			int num = MapV[item];
			target.SetVertex(item, From[num]);
			if (hasVertexNormals)
			{
				target.SetVertexNormal(item, (Vector3f)(Rotation * target.GetVertexNormal(item)));
			}
		}
	}

	private void initialize()
	{
		From = new Vector3d[Source.VertexCount];
		To = new Vector3d[Source.VertexCount];
		Weights = new double[Source.VertexCount];
		MapV = new int[Source.MaxVertexID];
		int num = 0;
		foreach (int item in Source.VertexIndices())
		{
			MapV[item] = num;
			Weights[num] = 1.0;
			From[num++] = Source.GetVertex(item);
		}
	}

	private void update_from()
	{
		int num = 0;
		foreach (int item in Source.VertexIndices())
		{
			Weights[num] = 1.0;
			Vector3d vertex = Source.GetVertex(item);
			From[num++] = Rotation * vertex + Translation;
		}
	}

	private void update_to()
	{
		double max_dist = double.MaxValue;
		bool bNormals = UseNormals && Source.HasVertexNormals;
		gParallel.ForEach(Interval1i.Range(From.Length), delegate(int vi)
		{
			int num = TargetSurface.FindNearestTriangle(From[vi], max_dist);
			if (num == -1)
			{
				Weights[vi] = 0.0;
			}
			else
			{
				DistPoint3Triangle3 distPoint3Triangle = MeshQueries.TriangleDistance(TargetSurface.Mesh, num, From[vi]);
				if (distPoint3Triangle.DistanceSquared > MaxAllowableDistance * MaxAllowableDistance)
				{
					Weights[vi] = 0.0;
				}
				else
				{
					To[vi] = distPoint3Triangle.TriangleClosest;
					Weights[vi] = 1.0;
					if (bNormals)
					{
						Vector3d vector3d = Rotation * Source.GetVertexNormal(vi);
						Vector3d triNormal = TargetSurface.Mesh.GetTriNormal(num);
						double num2 = vector3d.Dot(triNormal);
						if (num2 < 0.0)
						{
							Weights[vi] = 0.0;
						}
						else
						{
							Weights[vi] += Math.Sqrt(num2);
						}
					}
				}
			}
		});
	}

	private double measure_error()
	{
		double num = 0.0;
		double num2 = 0.0;
		for (int i = 0; i < From.Length; i++)
		{
			num += Weights[i] * From[i].Distance(To[i]);
			num2 += Weights[i];
		}
		return num / num2;
	}

	private void update_transformation()
	{
		int num = From.Length;
		double num2 = 0.0;
		for (int i = 0; i < num; i++)
		{
			num2 += Weights[i];
		}
		double num3 = 1.0 / num2;
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		for (int j = 0; j < num; j++)
		{
			zero += Weights[j] * num3 * From[j];
			zero2 += Weights[j] * num3 * To[j];
		}
		for (int k = 0; k < num; k++)
		{
			From[k] -= zero;
			To[k] -= zero2;
		}
		double[] array = new double[9];
		for (int l = 0; l < 3; l++)
		{
			int num4 = 3 * l;
			for (int m = 0; m < num; m++)
			{
				double num5 = Weights[m] * num3 * From[m][l];
				array[num4] += num5 * To[m].x;
				array[num4 + 1] += num5 * To[m].y;
				array[num4 + 2] += num5 * To[m].z;
			}
		}
		SingularValueDecomposition singularValueDecomposition = new SingularValueDecomposition(3, 3, 100);
		singularValueDecomposition.Solve(array);
		double[] array2 = new double[9];
		double[] array3 = new double[9];
		double[] array4 = new double[9];
		singularValueDecomposition.GetU(array2);
		singularValueDecomposition.GetV(array3);
		double[] array5 = new double[9];
		double num6 = MatrixUtil.Determinant3x3(array2);
		double num7 = MatrixUtil.Determinant3x3(array3);
		if (num6 * num7 < 0.0)
		{
			double[] b = MatrixUtil.MakeDiagonal3x3(1.0, 1.0, -1.0);
			MatrixUtil.Multiply3x3(array3, b, array4);
			MatrixUtil.Transpose3x3(array2);
			MatrixUtil.Multiply3x3(array4, array2, array5);
		}
		else
		{
			MatrixUtil.Transpose3x3(array2);
			MatrixUtil.Multiply3x3(array3, array2, array5);
		}
		Matrix3d mat = new Matrix3d(array5);
		Quaterniond quaterniond = new Quaterniond(mat);
		Vector3d vector3d = zero2 - quaterniond * zero;
		Translation += vector3d;
		Rotation = quaterniond * Rotation;
	}
}
