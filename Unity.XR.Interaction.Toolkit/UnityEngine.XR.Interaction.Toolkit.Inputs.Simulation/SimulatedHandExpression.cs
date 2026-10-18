using System;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[Serializable]
public class SimulatedHandExpression : ISerializationCallbackReceiver
{
	[SerializeField]
	[Tooltip("The unique name for the hand expression.")]
	[Delayed]
	private string m_Name;

	[SerializeField]
	[Tooltip("The input to trigger the simulated hand expression.")]
	private XRInputButtonReader m_ToggleInput;

	[SerializeField]
	[Tooltip("The captured hand expression to simulate when the input action is performed.")]
	private HandExpressionCapture m_Capture;

	[SerializeField]
	[Tooltip("Whether or not this expression appears in the quick action list in the simulator.")]
	private bool m_IsQuickAction;

	private HandExpressionName m_ExpressionName;

	public string name => m_ExpressionName.ToString();

	public XRInputButtonReader toggleInput
	{
		get
		{
			return m_ToggleInput;
		}
		set
		{
			m_ToggleInput = value;
		}
	}

	internal HandExpressionCapture capture
	{
		get
		{
			return m_Capture;
		}
		set
		{
			m_Capture = value;
		}
	}

	public bool isQuickAction
	{
		get
		{
			return m_IsQuickAction;
		}
		set
		{
			m_IsQuickAction = value;
		}
	}

	internal HandExpressionName expressionName
	{
		get
		{
			return m_ExpressionName;
		}
		set
		{
			m_ExpressionName = value;
		}
	}

	public Sprite icon => m_Capture.icon;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		m_Name = m_ExpressionName.ToString();
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_ExpressionName = new HandExpressionName(m_Name);
	}
}
