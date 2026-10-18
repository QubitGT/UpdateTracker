namespace UnityEngine.Rendering;

internal class ProbeSamplingDebugData
{
	public ProbeSamplingDebugUpdate update;

	public Vector2 coordinates = new Vector2(0.5f, 0.5f);

	public bool forceScreenCenterCoordinates;

	public Camera camera;

	public bool shortcutPressed;

	public GraphicsBuffer positionNormalBuffer;
}
