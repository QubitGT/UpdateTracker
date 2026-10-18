using System;

namespace g3;

public class ModifyVerticesMeshChange
{
	public DVector<int> ModifiedV;

	public DVector<Vector3d> OldPositions;

	public DVector<Vector3d> NewPositions;

	public DVector<Vector3f> OldNormals;

	public DVector<Vector3f> NewNormals;

	public DVector<Vector3f> OldColors;

	public DVector<Vector3f> NewColors;

	public DVector<Vector2f> OldUVs;

	public DVector<Vector2f> NewUVs;

	public Action<ModifyVerticesMeshChange> OnApplyF;

	public Action<ModifyVerticesMeshChange> OnRevertF;

	public ModifyVerticesMeshChange(DMesh3 mesh, MeshComponents wantComponents = MeshComponents.All)
	{
		initialize_buffers(mesh, wantComponents);
	}

	public int AppendNewVertex(DMesh3 mesh, int vid)
	{
		int length = ModifiedV.Length;
		ModifiedV.Add(vid);
		OldPositions.Add(mesh.GetVertex(vid));
		NewPositions.Add(OldPositions[length]);
		if (NewNormals != null)
		{
			OldNormals.Add(mesh.GetVertexNormal(vid));
			NewNormals.Add(OldNormals[length]);
		}
		if (NewColors != null)
		{
			OldColors.Add(mesh.GetVertexColor(vid));
			NewColors.Add(OldColors[length]);
		}
		if (NewUVs != null)
		{
			OldUVs.Add(mesh.GetVertexUV(vid));
			NewUVs.Add(OldUVs[length]);
		}
		return length;
	}

	public void Apply(DMesh3 mesh)
	{
		int size = ModifiedV.size;
		for (int i = 0; i < size; i++)
		{
			int vID = ModifiedV[i];
			mesh.SetVertex(vID, NewPositions[i]);
			if (NewNormals != null)
			{
				mesh.SetVertexNormal(vID, NewNormals[i]);
			}
			if (NewColors != null)
			{
				mesh.SetVertexColor(vID, NewColors[i]);
			}
			if (NewUVs != null)
			{
				mesh.SetVertexUV(vID, NewUVs[i]);
			}
		}
		if (OnApplyF != null)
		{
			OnApplyF(this);
		}
	}

	public void Revert(DMesh3 mesh)
	{
		int size = ModifiedV.size;
		for (int i = 0; i < size; i++)
		{
			int vID = ModifiedV[i];
			mesh.SetVertex(vID, OldPositions[i]);
			if (NewNormals != null)
			{
				mesh.SetVertexNormal(vID, OldNormals[i]);
			}
			if (NewColors != null)
			{
				mesh.SetVertexColor(vID, OldColors[i]);
			}
			if (NewUVs != null)
			{
				mesh.SetVertexUV(vID, OldUVs[i]);
			}
		}
		if (OnRevertF != null)
		{
			OnRevertF(this);
		}
	}

	private void initialize_buffers(DMesh3 mesh, MeshComponents components)
	{
		ModifiedV = new DVector<int>();
		NewPositions = new DVector<Vector3d>();
		OldPositions = new DVector<Vector3d>();
		if (mesh.HasVertexNormals && (components & MeshComponents.VertexNormals) != 0)
		{
			NewNormals = new DVector<Vector3f>();
			OldNormals = new DVector<Vector3f>();
		}
		if (mesh.HasVertexColors && (components & MeshComponents.VertexColors) != 0)
		{
			NewColors = new DVector<Vector3f>();
			OldColors = new DVector<Vector3f>();
		}
		if (mesh.HasVertexUVs && (components & MeshComponents.VertexUVs) != 0)
		{
			NewUVs = new DVector<Vector2f>();
			OldUVs = new DVector<Vector2f>();
		}
	}
}
