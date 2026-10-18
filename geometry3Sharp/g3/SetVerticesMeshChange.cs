using System;

namespace g3;

public class SetVerticesMeshChange
{
	public DVector<double> OldPositions;

	public DVector<double> NewPositions;

	public DVector<float> OldNormals;

	public DVector<float> NewNormals;

	public DVector<float> OldColors;

	public DVector<float> NewColors;

	public DVector<float> OldUVs;

	public DVector<float> NewUVs;

	public Action<SetVerticesMeshChange> OnApplyF;

	public Action<SetVerticesMeshChange> OnRevertF;

	public void Apply(DMesh3 mesh)
	{
		if (NewPositions != null)
		{
			mesh.VerticesBuffer.copy(NewPositions);
		}
		if (mesh.HasVertexNormals && NewNormals != null)
		{
			mesh.NormalsBuffer.copy(NewNormals);
		}
		if (mesh.HasVertexColors && NewColors != null)
		{
			mesh.ColorsBuffer.copy(NewColors);
		}
		if (mesh.HasVertexUVs && NewUVs != null)
		{
			mesh.UVBuffer.copy(NewUVs);
		}
		if (OnApplyF != null)
		{
			OnApplyF(this);
		}
	}

	public void Revert(DMesh3 mesh)
	{
		if (OldPositions != null)
		{
			mesh.VerticesBuffer.copy(OldPositions);
		}
		if (mesh.HasVertexNormals && OldNormals != null)
		{
			mesh.NormalsBuffer.copy(OldNormals);
		}
		if (mesh.HasVertexColors && OldColors != null)
		{
			mesh.ColorsBuffer.copy(OldColors);
		}
		if (mesh.HasVertexUVs && OldUVs != null)
		{
			mesh.UVBuffer.copy(OldUVs);
		}
		if (OnRevertF != null)
		{
			OnRevertF(this);
		}
	}
}
