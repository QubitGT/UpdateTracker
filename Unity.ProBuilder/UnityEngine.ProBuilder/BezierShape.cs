using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[ExcludeFromPreset]
[ExcludeFromObjectFactory]
[RequireComponent(typeof(ProBuilderMesh))]
internal sealed class BezierShape : MonoBehaviour
{
	public List<BezierPoint> points = new List<BezierPoint>();

	public bool closeLoop;

	public float radius = 0.5f;

	public int rows = 8;

	public int columns = 16;

	public bool smooth = true;

	[SerializeField]
	private bool m_IsEditing;

	private ProBuilderMesh m_Mesh;

	public bool isEditing
	{
		get
		{
			return m_IsEditing;
		}
		set
		{
			m_IsEditing = value;
		}
	}

	public ProBuilderMesh mesh
	{
		get
		{
			if (m_Mesh == null)
			{
				m_Mesh = GetComponent<ProBuilderMesh>();
			}
			return m_Mesh;
		}
		set
		{
			m_Mesh = value;
		}
	}

	public void Init()
	{
		Vector3 vector = new Vector3(0f, 0f, 2f);
		Vector3 vector2 = new Vector3(3f, 0f, 0f);
		points.Add(new BezierPoint(Vector3.zero, -vector, vector, Quaternion.identity));
		points.Add(new BezierPoint(vector2, vector2 + vector, vector2 + -vector, Quaternion.identity));
	}

	public void Refresh()
	{
		if (points.Count < 2)
		{
			mesh.Clear();
			mesh.ToMesh();
			mesh.Refresh();
		}
		else
		{
			ProBuilderMesh target = mesh;
			Spline.Extrude(points, radius, columns, rows, closeLoop, smooth, ref target);
		}
	}
}
