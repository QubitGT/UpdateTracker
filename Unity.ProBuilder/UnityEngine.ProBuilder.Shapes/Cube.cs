namespace UnityEngine.ProBuilder.Shapes;

[Shape("Cube")]
public class Cube : Shape
{
	private static readonly Vector3[] k_CubeVertices = new Vector3[8]
	{
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f)
	};

	private static readonly int[] k_CubeTriangles = new int[24]
	{
		0, 1, 4, 5, 1, 2, 5, 6, 2, 3,
		6, 7, 3, 0, 7, 4, 4, 5, 7, 6,
		3, 2, 0, 1
	};

	internal override void SetParametersToBuiltInShape()
	{
	}

	public override void CopyShape(Shape shape)
	{
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		mesh.Clear();
		Vector3[] array = new Vector3[k_CubeTriangles.Length];
		for (int i = 0; i < k_CubeTriangles.Length; i++)
		{
			array[i] = rotation * Vector3.Scale(k_CubeVertices[k_CubeTriangles[i]], size.Abs());
		}
		mesh.GeometryWithPoints(array);
		UvUnwrapping.SetAutoUV(mesh, mesh.facesInternal, auto: true);
		Face[] facesInternal = mesh.facesInternal;
		foreach (Face face in facesInternal)
		{
			face.uv = new AutoUnwrapSettings(face.uv)
			{
				anchor = AutoUnwrapSettings.Anchor.UpperLeft
			};
		}
		mesh.RefreshUV(mesh.faces);
		return mesh.mesh.bounds;
	}
}
