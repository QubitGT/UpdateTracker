using System;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEngine.ProBuilder.Shapes;

[AddComponentMenu("")]
[DisallowMultipleComponent]
internal sealed class ProBuilderShape : MonoBehaviour
{
	private const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditableMesh.png";

	[SerializeReference]
	private Shape m_Shape = new Cube();

	[SerializeField]
	private Quaternion m_ShapeRotation = Quaternion.identity;

	private ProBuilderMesh m_Mesh;

	[SerializeField]
	internal ushort m_UnmodifiedMeshVersion;

	[SerializeField]
	private Vector3 m_Size = Vector3.one;

	private Bounds m_EditionBounds;

	[SerializeField]
	private Vector3 m_LocalCenter;

	public Shape shape => m_Shape;

	public Vector3 size
	{
		get
		{
			return m_Size;
		}
		set
		{
			m_Size.x = ((System.Math.Abs(value.x) == 0f) ? (Mathf.Sign(m_Size.x) * 0.001f) : value.x);
			m_Size.y = value.y;
			m_Size.z = ((System.Math.Abs(value.z) == 0f) ? (Mathf.Sign(m_Size.z) * 0.001f) : value.z);
		}
	}

	public Quaternion shapeRotation
	{
		get
		{
			return m_ShapeRotation;
		}
		set
		{
			m_ShapeRotation = value;
		}
	}

	public Vector3 shapeWorldCenter => base.transform.TransformPoint(m_LocalCenter);

	public Bounds editionBounds
	{
		get
		{
			m_EditionBounds.center = m_LocalCenter;
			m_EditionBounds.size = m_Size;
			if (Mathf.Abs(m_Size.y) < Mathf.Epsilon)
			{
				m_EditionBounds.size = new Vector3(m_Size.x, 0f, m_Size.z);
			}
			return m_EditionBounds;
		}
	}

	public Bounds shapeLocalBounds => new Bounds(m_LocalCenter, size);

	public Bounds shapeWorldBounds => new Bounds(shapeWorldCenter, size);

	public bool isEditable => m_UnmodifiedMeshVersion == mesh.versionIndex;

	public ProBuilderMesh mesh
	{
		get
		{
			if (m_Mesh == null)
			{
				m_Mesh = GetComponent<ProBuilderMesh>();
			}
			if (m_Mesh == null)
			{
				m_Mesh = base.gameObject.AddComponent<ProBuilderMesh>();
			}
			return m_Mesh;
		}
	}

	private void OnValidate()
	{
		m_Size.x = ((System.Math.Abs(m_Size.x) == 0f) ? 0.001f : m_Size.x);
		m_Size.z = ((System.Math.Abs(m_Size.z) == 0f) ? 0.001f : m_Size.z);
	}

	internal void UpdateShape()
	{
		if (!(base.gameObject == null) && base.gameObject.hideFlags != HideFlags.HideAndDontSave)
		{
			Rebuild(mesh.transform.position, mesh.transform.rotation, new Bounds(shapeWorldCenter, size));
		}
	}

	internal void UpdateBounds(Bounds bounds)
	{
		Rebuild(mesh.transform.position, mesh.transform.rotation, bounds);
	}

	internal void Rebuild(Vector3 pivotPosition, Quaternion rotation, Bounds bounds)
	{
		Transform obj = base.transform;
		obj.position = bounds.center;
		obj.rotation = rotation;
		size = bounds.size;
		Rebuild();
		mesh.SetPivot(pivotPosition);
		m_LocalCenter = mesh.transform.InverseTransformPoint(bounds.center);
		m_UnmodifiedMeshVersion = mesh.versionIndex;
	}

	internal void Rebuild(Bounds bounds, Quaternion rotation)
	{
		Transform obj = base.transform;
		obj.position = bounds.center;
		obj.rotation = rotation;
		size = bounds.size;
		Rebuild();
		m_UnmodifiedMeshVersion = mesh.versionIndex;
	}

	private void Rebuild()
	{
		if (!(base.gameObject == null) && base.gameObject.hideFlags != HideFlags.HideAndDontSave)
		{
			Bounds currentSize = m_Shape.RebuildMesh(mesh, size, shapeRotation);
			currentSize.size = currentSize.size.Abs();
			MeshUtility.FitToSize(mesh, currentSize, size);
		}
	}

	internal void SetShape(Shape shape)
	{
		m_Shape = shape;
		if (m_Shape is Plane || m_Shape is Sprite)
		{
			Bounds bounds = new Bounds(m_LocalCenter, size);
			Vector3 center = bounds.center;
			Vector3 vector = bounds.size;
			center.y = 0f;
			vector.y = 0f;
			m_LocalCenter = center;
			size = vector;
			m_Size.y = 0f;
		}
		UpdateShape();
		m_UnmodifiedMeshVersion = mesh.versionIndex;
	}

	internal void RotateInsideBounds(Quaternion deltaRotation)
	{
		shapeRotation = deltaRotation * shapeRotation;
		Rebuild(bounds: new Bounds(mesh.transform.TransformPoint(m_LocalCenter), size), pivotPosition: mesh.transform.position, rotation: mesh.transform.rotation);
	}
}
