namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

internal struct XRSimulatedHandState
{
	public Vector3 position { get; set; }

	public Quaternion rotation { get; set; }

	public Vector3 euler { get; set; }

	public bool isTracked { get; set; }

	public HandExpressionName expressionName { get; set; }

	public void Reset()
	{
		position = default(Vector3);
		rotation = Quaternion.identity;
		euler = default(Vector3);
		isTracked = false;
		expressionName = HandExpressionName.Default;
	}
}
