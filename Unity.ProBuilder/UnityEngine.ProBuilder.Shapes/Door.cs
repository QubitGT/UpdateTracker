using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Door")]
public class Door : Shape
{
	[Min(0.01f)]
	[SerializeField]
	private float m_DoorHeight = 0.5f;

	[Min(0.01f)]
	[SerializeField]
	private float m_LegWidth = 0.75f;

	internal override void SetParametersToBuiltInShape()
	{
		m_DoorHeight = 0.5f;
		m_LegWidth = 0.75f;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Door)
		{
			m_DoorHeight = ((Door)shape).m_DoorHeight;
			m_LegWidth = ((Door)shape).m_LegWidth;
		}
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = Vector3.Scale(rotation * Vector3.up, size);
		Vector3 vector2 = Vector3.Scale(rotation * Vector3.right, size);
		Vector3 vector3 = Vector3.Scale(rotation * Vector3.forward, size);
		float magnitude = vector2.magnitude;
		float magnitude2 = vector.magnitude;
		float magnitude3 = vector3.magnitude;
		float num = magnitude / 2f;
		float num2 = ((num - m_LegWidth > 0f) ? (num - m_LegWidth) : 0.001f);
		float y = ((magnitude2 - m_DoorHeight * 2f > 0f) ? (magnitude2 - m_DoorHeight * 2f) : 0.001f);
		float y2 = 0f - magnitude2;
		float z = magnitude3 / 2f;
		Vector3[] array = new Vector3[12]
		{
			new Vector3(0f - num, y2, z),
			new Vector3(0f - num2, y2, z),
			new Vector3(num2, y2, z),
			new Vector3(num, y2, z),
			new Vector3(0f - num, y, z),
			new Vector3(0f - num2, y, z),
			new Vector3(num2, y, z),
			new Vector3(num, y, z),
			new Vector3(0f - num, magnitude2, z),
			new Vector3(0f - num2, magnitude2, z),
			new Vector3(num2, magnitude2, z),
			new Vector3(num, magnitude2, z)
		};
		List<Vector3> list = new List<Vector3>();
		list.Add(array[4]);
		list.Add(array[0]);
		list.Add(array[5]);
		list.Add(array[1]);
		list.Add(array[2]);
		list.Add(array[3]);
		list.Add(array[6]);
		list.Add(array[7]);
		list.Add(array[4]);
		list.Add(array[5]);
		list.Add(array[8]);
		list.Add(array[9]);
		list.Add(array[10]);
		list.Add(array[6]);
		list.Add(array[11]);
		list.Add(array[7]);
		list.Add(array[5]);
		list.Add(array[6]);
		list.Add(array[9]);
		list.Add(array[10]);
		List<Vector3> list2 = new List<Vector3>();
		for (int i = 0; i < list.Count; i += 4)
		{
			list2.Add(list[i] - Vector3.forward * magnitude3);
			list2.Add(list[i + 2] - Vector3.forward * magnitude3);
			list2.Add(list[i + 1] - Vector3.forward * magnitude3);
			list2.Add(list[i + 3] - Vector3.forward * magnitude3);
		}
		list.AddRange(list2);
		list.Add(array[6]);
		list.Add(array[5]);
		list.Add(array[6] - Vector3.forward * magnitude3);
		list.Add(array[5] - Vector3.forward * magnitude3);
		list.Add(array[2] - Vector3.forward * magnitude3);
		list.Add(array[2]);
		list.Add(array[6] - Vector3.forward * magnitude3);
		list.Add(array[6]);
		list.Add(array[1]);
		list.Add(array[1] - Vector3.forward * magnitude3);
		list.Add(array[5]);
		list.Add(array[5] - Vector3.forward * magnitude3);
		Vector3 b = size.Sign();
		for (int j = 0; j < list.Count; j++)
		{
			list[j] = Vector3.Scale(rotation * list[j], b);
		}
		mesh.GeometryWithPoints(list.ToArray());
		if (b.x * b.y * b.z < 0f)
		{
			Face[] facesInternal = mesh.facesInternal;
			for (int k = 0; k < facesInternal.Length; k++)
			{
				facesInternal[k].Reverse();
			}
		}
		return mesh.mesh.bounds;
	}
}
