namespace g3;

public class TransformedMeshProjectionTarget : MeshProjectionTarget
{
	public TransformSequence SourceToTargetXForm;

	public TransformSequence TargetToSourceXForm;

	public TransformedMeshProjectionTarget()
	{
	}

	public TransformedMeshProjectionTarget(DMesh3 mesh, ISpatial spatial)
		: base(mesh, spatial)
	{
	}

	public TransformedMeshProjectionTarget(DMesh3 mesh)
		: base(mesh)
	{
	}

	public void SetTransform(TransformSequence sourceToTargetX)
	{
		SourceToTargetXForm = sourceToTargetX;
		TargetToSourceXForm = SourceToTargetXForm.MakeInverse();
	}

	public override Vector3d Project(Vector3d vPoint, int identifier = -1)
	{
		Vector3d vPoint2 = SourceToTargetXForm.TransformP(vPoint);
		Vector3d p = base.Project(vPoint2, identifier);
		return TargetToSourceXForm.TransformP(p);
	}

	public override Vector3d Project(Vector3d vPoint, out Vector3d vProjectNormal, int identifier = -1)
	{
		Vector3d vPoint2 = SourceToTargetXForm.TransformP(vPoint);
		Vector3d vProjectNormal2;
		Vector3d p = base.Project(vPoint2, out vProjectNormal2, identifier);
		vProjectNormal = TargetToSourceXForm.TransformV(vProjectNormal2).Normalized;
		return TargetToSourceXForm.TransformP(p);
	}
}
