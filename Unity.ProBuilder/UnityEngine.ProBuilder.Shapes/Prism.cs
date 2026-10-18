namespace UnityEngine.ProBuilder.Shapes;

[Shape("Prism")]
public class Prism : Shape
{
	internal override void SetParametersToBuiltInShape()
	{
	}

	public override void CopyShape(Shape shape)
	{
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 b = size.Abs();
		b.y = ((b.y == 0f) ? (2f * Mathf.Epsilon) : b.y);
		Vector3 vector = new Vector3(0f, b.y / 2f, 0f);
		b.y *= 2f;
		Vector3[] array = new Vector3[6]
		{
			Vector3.Scale(new Vector3(-0.5f, 0f, -0.5f), b) - vector,
			Vector3.Scale(new Vector3(0.5f, 0f, -0.5f), b) - vector,
			Vector3.Scale(new Vector3(0f, 0.5f, -0.5f), b) - vector,
			Vector3.Scale(new Vector3(-0.5f, 0f, 0.5f), b) - vector,
			Vector3.Scale(new Vector3(0.5f, 0f, 0.5f), b) - vector,
			Vector3.Scale(new Vector3(0f, 0.5f, 0.5f), b) - vector
		};
		Vector3[] array2 = new Vector3[18]
		{
			array[0],
			array[1],
			array[2],
			array[1],
			array[4],
			array[2],
			array[5],
			array[4],
			array[3],
			array[5],
			array[3],
			array[0],
			array[5],
			array[2],
			array[0],
			array[1],
			array[3],
			array[4]
		};
		Face[] array3 = new Face[5]
		{
			new Face(new int[3] { 2, 1, 0 }),
			new Face(new int[6] { 5, 4, 3, 5, 6, 4 }),
			new Face(new int[3] { 9, 8, 7 }),
			new Face(new int[6] { 12, 11, 10, 12, 13, 11 }),
			new Face(new int[6] { 14, 15, 16, 15, 17, 16 })
		};
		Vector3 b2 = size.Sign();
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = Vector3.Scale(rotation * array2[i], b2);
		}
		if (Mathf.Sign(size.x) * Mathf.Sign(size.y) * Mathf.Sign(size.z) < 0f)
		{
			Face[] array4 = array3;
			for (int j = 0; j < array4.Length; j++)
			{
				array4[j].Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(array2, array3);
		return mesh.mesh.bounds;
	}
}
