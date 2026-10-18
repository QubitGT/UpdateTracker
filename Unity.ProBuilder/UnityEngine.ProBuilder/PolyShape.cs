using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder;

[AddComponentMenu("")]
[DisallowMultipleComponent]
[ExcludeFromPreset]
[ExcludeFromObjectFactory]
[ProGridsConditionalSnap]
public sealed class PolyShape : MonoBehaviour
{
	internal enum PolyEditMode
	{
		None,
		Path,
		Height,
		Edit
	}

	private const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditableMesh.png";

	private ProBuilderMesh m_Mesh;

	[FormerlySerializedAs("points")]
	[SerializeField]
	internal List<Vector3> m_Points = new List<Vector3>();

	[FormerlySerializedAs("extrude")]
	[SerializeField]
	private float m_Extrude;

	[FormerlySerializedAs("polyEditMode")]
	[SerializeField]
	private PolyEditMode m_EditMode;

	[FormerlySerializedAs("flipNormals")]
	[SerializeField]
	private bool m_FlipNormals;

	[SerializeField]
	internal bool isOnGrid = true;

	public ReadOnlyCollection<Vector3> controlPoints => new ReadOnlyCollection<Vector3>(m_Points);

	public float extrude
	{
		get
		{
			return m_Extrude;
		}
		set
		{
			m_Extrude = value;
		}
	}

	internal PolyEditMode polyEditMode
	{
		get
		{
			return m_EditMode;
		}
		set
		{
			m_EditMode = value;
		}
	}

	public bool flipNormals
	{
		get
		{
			return m_FlipNormals;
		}
		set
		{
			m_FlipNormals = value;
		}
	}

	internal ProBuilderMesh mesh
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

	public void SetControlPoints(IList<Vector3> points)
	{
		m_Points = points.ToList();
	}

	private bool IsSnapEnabled()
	{
		return isOnGrid;
	}
}
