using System.Collections.Generic;

namespace UnityEngine.ProBuilder.MeshOperations;

internal static class Subdivision
{
	public static ActionResult Subdivide(this ProBuilderMesh pb)
	{
		if (pb.Subdivide(pb.facesInternal) == null)
		{
			return new ActionResult(ActionResult.Status.Failure, "Subdivide Failed");
		}
		return new ActionResult(ActionResult.Status.Success, "Subdivide");
	}

	public static Face[] Subdivide(this ProBuilderMesh pb, IList<Face> faces)
	{
		return pb.Connect(faces);
	}
}
