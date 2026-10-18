namespace UnityEngine.ProBuilder.Shapes;

[Shape("Sprite")]
public class Sprite : Shape
{
	internal override void SetParametersToBuiltInShape()
	{
	}

	public override void CopyShape(Shape shape)
	{
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = size.Abs();
		if (vector.x < float.Epsilon || vector.z < float.Epsilon)
		{
			mesh.Clear();
			if (mesh.mesh != null)
			{
				mesh.mesh.Clear();
			}
			return default(Bounds);
		}
		float x = vector.x;
		float z = vector.z;
		Vector2[] array = new Vector2[4];
		Vector3[] array2 = new Vector3[4];
		Face[] array3 = new Face[1];
		float x2 = 0f - x / 2f;
		float x3 = x / 2f;
		float y = 0f - z / 2f;
		float y2 = z / 2f;
		array[0] = new Vector2(x2, y);
		array[1] = new Vector2(x3, y);
		array[2] = new Vector2(x2, y2);
		array[3] = new Vector2(x3, y2);
		array3[0] = new Face(new int[6] { 0, 1, 2, 1, 3, 2 });
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = new Vector3(array[i].y, 0f, array[i].x);
		}
		mesh.RebuildWithPositionsAndFaces(array2, array3);
		return mesh.mesh.bounds;
	}
}
