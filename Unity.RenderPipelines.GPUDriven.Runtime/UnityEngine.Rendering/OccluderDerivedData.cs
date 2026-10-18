namespace UnityEngine.Rendering;

internal struct OccluderDerivedData
{
	public Matrix4x4 viewProjMatrix;

	public Vector4 viewOriginWorldSpace;

	public Vector4 radialDirWorldSpace;

	public Vector4 facingDirWorldSpace;

	public static OccluderDerivedData FromParameters(in OccluderSubviewUpdate occluderSubviewUpdate)
	{
		Vector3 vector = occluderSubviewUpdate.viewOffsetWorldSpace + (Vector3)occluderSubviewUpdate.invViewMatrix.GetColumn(3);
		Vector3 vector2 = occluderSubviewUpdate.invViewMatrix.GetColumn(0);
		Vector3 vector3 = occluderSubviewUpdate.invViewMatrix.GetColumn(1);
		Vector3 vector4 = occluderSubviewUpdate.invViewMatrix.GetColumn(2);
		Matrix4x4 viewMatrix = occluderSubviewUpdate.viewMatrix;
		viewMatrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
		OccluderDerivedData result = default(OccluderDerivedData);
		result.viewOriginWorldSpace = vector;
		result.facingDirWorldSpace = vector4.normalized;
		result.radialDirWorldSpace = (vector2 + vector3).normalized;
		result.viewProjMatrix = occluderSubviewUpdate.gpuProjMatrix * viewMatrix;
		return result;
	}
}
