using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[AddComponentMenu("XR/Debug/Simulated Hand Expression Manager", 11)]
[DefaultExecutionOrder(-29994)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.SimulatedHandExpressionManager.html")]
public class SimulatedHandExpressionManager : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The list of hand expressions to simulate.")]
	private List<SimulatedHandExpression> m_SimulatedHandExpressions = new List<SimulatedHandExpression>();

	[SerializeField]
	[Tooltip("The resting hand expression to use when no other hand expression is active.")]
	private HandExpressionCapture m_RestingHandExpressionCapture;

	private SimulatedDeviceLifecycleManager m_DeviceLifecycleManager;

	public List<SimulatedHandExpression> simulatedHandExpressions => m_SimulatedHandExpressions;

	internal HandExpressionCapture restingHandExpressionCapture
	{
		get
		{
			return m_RestingHandExpressionCapture;
		}
		set
		{
			m_RestingHandExpressionCapture = value;
		}
	}

	protected virtual void Start()
	{
		m_DeviceLifecycleManager = XRSimulatorUtility.FindCreateSimulatedDeviceLifecycleManager(base.gameObject);
		InitializeHandExpressions();
	}

	private void InitializeHandExpressions()
	{
	}
}
