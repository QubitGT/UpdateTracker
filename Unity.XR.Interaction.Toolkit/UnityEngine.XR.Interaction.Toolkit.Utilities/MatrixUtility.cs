namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

internal static class MatrixUtility
{
	public static void ApplyMatrix4x4(Transform transform, Matrix4x4 worldMatrix)
	{
		Vector3 position = worldMatrix.GetPosition();
		Quaternion rotation = worldMatrix.rotation;
		Vector3 localScale = new Vector3(worldMatrix.GetColumn(0).magnitude, worldMatrix.GetColumn(1).magnitude, worldMatrix.GetColumn(2).magnitude);
		transform.SetPositionAndRotation(position, rotation);
		transform.localScale = localScale;
	}
}
