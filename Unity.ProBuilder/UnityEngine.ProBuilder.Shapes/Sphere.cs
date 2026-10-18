namespace UnityEngine.ProBuilder.Shapes;

[Shape("Sphere")]
public class Sphere : Shape
{
	private static readonly Vector3[] k_IcosphereVertices = new Vector3[12]
	{
		new Vector3(-1f, 1.618034f, 0f),
		new Vector3(1f, 1.618034f, 0f),
		new Vector3(-1f, -1.618034f, 0f),
		new Vector3(1f, -1.618034f, 0f),
		new Vector3(0f, -1f, 1.618034f),
		new Vector3(0f, 1f, 1.618034f),
		new Vector3(0f, -1f, -1.618034f),
		new Vector3(0f, 1f, -1.618034f),
		new Vector3(1.618034f, 0f, -1f),
		new Vector3(1.618034f, 0f, 1f),
		new Vector3(-1.618034f, 0f, -1f),
		new Vector3(-1.618034f, 0f, 1f)
	};

	private static readonly int[] k_IcosphereTriangles = new int[60]
	{
		0, 11, 5, 0, 5, 1, 0, 1, 7, 0,
		7, 10, 0, 10, 11, 1, 5, 9, 5, 11,
		4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
		3, 9, 4, 3, 4, 2, 3, 2, 6, 3,
		6, 8, 3, 8, 9, 4, 9, 5, 2, 4,
		11, 6, 2, 10, 8, 6, 7, 9, 8, 1
	};

	[Range(1f, 5f)]
	[SerializeField]
	private int m_Subdivisions = 3;

	private int m_BottomMostVertexIndex;

	[SerializeField]
	private bool m_Smooth = true;

	internal override void SetParametersToBuiltInShape()
	{
		m_Subdivisions = 2;
		m_Smooth = false;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Sphere)
		{
			Sphere sphere = (Sphere)shape;
			m_Subdivisions = sphere.m_Subdivisions;
			m_BottomMostVertexIndex = sphere.m_BottomMostVertexIndex;
			m_Smooth = sphere.m_Smooth;
		}
	}

	public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		bounds = mesh.mesh.bounds;
		return bounds;
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		float num = 0.5f;
		Vector3[] array = new Vector3[k_IcosphereTriangles.Length];
		for (int i = 0; i < k_IcosphereTriangles.Length; i += 3)
		{
			array[i] = k_IcosphereVertices[k_IcosphereTriangles[i]].normalized * num;
			array[i + 1] = k_IcosphereVertices[k_IcosphereTriangles[i + 1]].normalized * num;
			array[i + 2] = k_IcosphereVertices[k_IcosphereTriangles[i + 2]].normalized * num;
		}
		for (int j = 0; j < m_Subdivisions; j++)
		{
			array = SubdivideIcosahedron(array, num);
		}
		Face[] array2 = new Face[array.Length / 3];
		Vector3 vector = Vector3.positiveInfinity;
		for (int k = 0; k < array.Length; k += 3)
		{
			array2[k / 3] = new Face(new int[3]
			{
				k,
				k + 1,
				k + 2
			});
			array2[k / 3].smoothingGroup = (m_Smooth ? 1 : 0);
			array2[k / 3].manualUV = false;
			for (int l = 0; l < array2[k / 3].indexes.Count; l++)
			{
				int num2 = array2[k / 3].indexes[l];
				if (array[num2].y < vector.y)
				{
					vector = array[num2];
					m_BottomMostVertexIndex = num2;
				}
			}
		}
		for (int m = 0; m < array2.Length; m++)
		{
			switch (Projection.VectorToProjectionAxis(Math.Normal(array[array2[m].indexesInternal[0]], array[array2[m].indexesInternal[1]], array[array2[m].indexesInternal[2]])))
			{
			case ProjectionAxis.X:
				array2[m].textureGroup = 2;
				break;
			case ProjectionAxis.Y:
				array2[m].textureGroup = 3;
				break;
			case ProjectionAxis.Z:
				array2[m].textureGroup = 4;
				break;
			case ProjectionAxis.XNegative:
				array2[m].textureGroup = 5;
				break;
			case ProjectionAxis.YNegative:
				array2[m].textureGroup = 6;
				break;
			case ProjectionAxis.ZNegative:
				array2[m].textureGroup = 7;
				break;
			}
		}
		mesh.unwrapParameters = new UnwrapParameters
		{
			packMargin = 30f
		};
		mesh.RebuildWithPositionsAndFaces(array, array2);
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}

	private static Vector3[] SubdivideIcosahedron(Vector3[] vertices, float radius)
	{
		Vector3[] array = new Vector3[vertices.Length * 4];
		int num = 0;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		Vector3 zero5 = Vector3.zero;
		Vector3 zero6 = Vector3.zero;
		for (int i = 0; i < vertices.Length; i += 3)
		{
			zero = vertices[i];
			zero3 = vertices[i + 1];
			zero6 = vertices[i + 2];
			zero2 = ((zero + zero3) * 0.5f).normalized * radius;
			zero4 = ((zero + zero6) * 0.5f).normalized * radius;
			zero5 = ((zero3 + zero6) * 0.5f).normalized * radius;
			array[num++] = zero;
			array[num++] = zero2;
			array[num++] = zero4;
			array[num++] = zero2;
			array[num++] = zero3;
			array[num++] = zero5;
			array[num++] = zero2;
			array[num++] = zero5;
			array[num++] = zero4;
			array[num++] = zero4;
			array[num++] = zero5;
			array[num++] = zero6;
		}
		return array;
	}
}
