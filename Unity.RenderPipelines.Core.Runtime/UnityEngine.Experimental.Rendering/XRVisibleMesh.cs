using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering;

internal class XRVisibleMesh
{
	private XRPass m_Pass;

	private Mesh m_CombinedMesh;

	private int m_CombinedMeshHashCode;

	private static readonly ProfilingSampler k_VisibleMeshProfilingSampler = new ProfilingSampler("XR Visible Mesh");

	internal bool hasValidVisibleMesh
	{
		get
		{
			if (IsVisibleMeshSupported())
			{
				if (m_Pass.singlePassEnabled)
				{
					return m_CombinedMesh != null;
				}
				return m_Pass.GetVisibleMesh() != null;
			}
			return false;
		}
	}

	internal XRVisibleMesh(XRPass xrPass)
	{
		m_Pass = xrPass;
	}

	internal void Dispose()
	{
		if ((bool)m_CombinedMesh)
		{
			CoreUtils.Destroy(m_CombinedMesh);
			m_CombinedMesh = null;
		}
	}

	internal void RenderVisibleMeshCustomMaterial(CommandBuffer cmd, float occlusionMeshScale, Material material, MaterialPropertyBlock materialBlock, int shaderPass, bool yFlip = false)
	{
		if (IsVisibleMeshSupported())
		{
			using (new ProfilingScope(cmd, k_VisibleMeshProfilingSampler))
			{
				Vector3 vector = new Vector3(occlusionMeshScale, yFlip ? occlusionMeshScale : (0f - occlusionMeshScale), 1f);
				Mesh mesh = (m_Pass.singlePassEnabled ? m_CombinedMesh : m_Pass.GetVisibleMesh());
				cmd.DrawMesh(mesh, Matrix4x4.Scale(vector), material, 0, shaderPass, materialBlock);
			}
		}
	}

	internal void UpdateCombinedMesh()
	{
		if (IsVisibleMeshSupported() && m_Pass.singlePassEnabled && TryGetVisibleMeshCombinedHashCode(out var hashCode))
		{
			if (m_CombinedMesh == null || hashCode != m_CombinedMeshHashCode)
			{
				CreateVisibleMeshCombined();
				m_CombinedMeshHashCode = hashCode;
			}
		}
		else
		{
			m_CombinedMesh = null;
			m_CombinedMeshHashCode = 0;
		}
	}

	private bool IsVisibleMeshSupported()
	{
		if (m_Pass.enabled)
		{
			return m_Pass.occlusionMeshScale > 0f;
		}
		return false;
	}

	private bool TryGetVisibleMeshCombinedHashCode(out int hashCode)
	{
		hashCode = 17;
		for (int i = 0; i < m_Pass.viewCount; i++)
		{
			Mesh visibleMesh = m_Pass.GetVisibleMesh(i);
			if (visibleMesh != null)
			{
				hashCode = hashCode * 23 + visibleMesh.GetHashCode();
				continue;
			}
			hashCode = 0;
			return false;
		}
		return true;
	}

	private void CreateVisibleMeshCombined()
	{
		CoreUtils.Destroy(m_CombinedMesh);
		m_CombinedMesh = new Mesh();
		m_CombinedMesh.indexFormat = IndexFormat.UInt16;
		int num = 0;
		uint num2 = 0u;
		for (int i = 0; i < m_Pass.viewCount; i++)
		{
			Mesh visibleMesh = m_Pass.GetVisibleMesh(i);
			num += visibleMesh.vertexCount;
			num2 += visibleMesh.GetIndexCount(0);
		}
		Vector3[] array = new Vector3[num];
		ushort[] array2 = new ushort[num2];
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < m_Pass.viewCount; j++)
		{
			Mesh visibleMesh2 = m_Pass.GetVisibleMesh(j);
			int[] indices = visibleMesh2.GetIndices(0);
			visibleMesh2.vertices.CopyTo(array, num3);
			for (int k = 0; k < visibleMesh2.vertices.Length; k++)
			{
				array[num3 + k].z = j;
			}
			for (int l = 0; l < indices.Length; l++)
			{
				int num5 = num3 + indices[l];
				array2[num4 + l] = (ushort)num5;
			}
			num3 += visibleMesh2.vertexCount;
			num4 += indices.Length;
		}
		m_CombinedMesh.vertices = array;
		m_CombinedMesh.SetIndices(array2, MeshTopology.Triangles, 0);
	}
}
