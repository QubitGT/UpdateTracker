using System;

namespace UnityEngine.Experimental.Rendering;

internal readonly struct XRView
{
	internal readonly Matrix4x4 projMatrix;

	internal readonly Matrix4x4 viewMatrix;

	internal readonly Matrix4x4 prevViewMatrix;

	internal readonly Rect viewport;

	internal readonly Mesh occlusionMesh;

	internal readonly Mesh visibleMesh;

	internal readonly int textureArraySlice;

	internal readonly Vector2 eyeCenterUV;

	internal readonly bool isPrevViewMatrixValid;

	internal XRView(Matrix4x4 projMatrix, Matrix4x4 viewMatrix, Matrix4x4 prevViewMatrix, bool isPrevViewMatrixValid, Rect viewport, Mesh occlusionMesh, Mesh visibleMesh, int textureArraySlice)
	{
		this.projMatrix = projMatrix;
		this.viewMatrix = viewMatrix;
		this.prevViewMatrix = prevViewMatrix;
		this.viewport = viewport;
		this.occlusionMesh = occlusionMesh;
		this.visibleMesh = visibleMesh;
		this.textureArraySlice = textureArraySlice;
		this.isPrevViewMatrixValid = isPrevViewMatrixValid;
		eyeCenterUV = ComputeEyeCenterUV(projMatrix);
	}

	private static Vector2 ComputeEyeCenterUV(Matrix4x4 proj)
	{
		FrustumPlanes decomposeProjection = proj.decomposeProjection;
		float num = Math.Abs(decomposeProjection.left);
		float num2 = Math.Abs(decomposeProjection.right);
		float num3 = Math.Abs(decomposeProjection.top);
		float num4 = Math.Abs(decomposeProjection.bottom);
		return new Vector2(num / (num2 + num), num3 / (num3 + num4));
	}
}
