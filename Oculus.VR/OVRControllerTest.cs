using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class OVRControllerTest : MonoBehaviour
{
	public class BoolMonitor
	{
		public delegate bool BoolGenerator();

		private string m_name = "";

		private BoolGenerator m_generator;

		private bool m_prevValue;

		private bool m_currentValue;

		private bool m_currentValueRecentlyChanged;

		private float m_displayTimeout;

		private float m_displayTimer;

		public BoolMonitor(string name, BoolGenerator generator, float displayTimeout = 0.5f)
		{
			m_name = name;
			m_generator = generator;
			m_displayTimeout = displayTimeout;
		}

		public void Update()
		{
			m_prevValue = m_currentValue;
			m_currentValue = m_generator();
			if (m_currentValue != m_prevValue)
			{
				m_currentValueRecentlyChanged = true;
				m_displayTimer = m_displayTimeout;
			}
			if (m_displayTimer > 0f)
			{
				m_displayTimer -= Time.deltaTime;
				if (m_displayTimer <= 0f)
				{
					m_currentValueRecentlyChanged = false;
					m_displayTimer = 0f;
				}
			}
		}

		public void AppendToStringBuilder(ref StringBuilder sb)
		{
			sb.Append(m_name);
			if (m_currentValue && m_currentValueRecentlyChanged)
			{
				sb.Append(": *True*\n");
			}
			else if (m_currentValue)
			{
				sb.Append(":  True \n");
			}
			else if (!m_currentValue && m_currentValueRecentlyChanged)
			{
				sb.Append(": *False*\n");
			}
			else if (!m_currentValue)
			{
				sb.Append(":  False \n");
			}
		}
	}

	public Text uiText;

	private List<BoolMonitor> monitors;

	private StringBuilder data;

	private static string prevConnected = "";

	private static BoolMonitor controllers = new BoolMonitor("Controllers Changed", () => OVRInput.GetConnectedControllers().ToString() != prevConnected);

	private void Start()
	{
		if (uiText != null)
		{
			uiText.supportRichText = false;
		}
		data = new StringBuilder(2048);
		monitors = new List<BoolMonitor>
		{
			new BoolMonitor("One", () => OVRInput.Get(OVRInput.Button.One)),
			new BoolMonitor("OneDown", () => OVRInput.GetDown(OVRInput.Button.One)),
			new BoolMonitor("OneUp", () => OVRInput.GetUp(OVRInput.Button.One)),
			new BoolMonitor("One (Touch)", () => OVRInput.Get(OVRInput.Touch.One)),
			new BoolMonitor("OneDown (Touch)", () => OVRInput.GetDown(OVRInput.Touch.One)),
			new BoolMonitor("OneUp (Touch)", () => OVRInput.GetUp(OVRInput.Touch.One)),
			new BoolMonitor("Two", () => OVRInput.Get(OVRInput.Button.Two)),
			new BoolMonitor("TwoDown", () => OVRInput.GetDown(OVRInput.Button.Two)),
			new BoolMonitor("TwoUp", () => OVRInput.GetUp(OVRInput.Button.Two)),
			new BoolMonitor("PrimaryIndexTrigger", () => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryIndexTriggerDown", () => OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryIndexTriggerUp", () => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryIndexTrigger (Touch)", () => OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryIndexTriggerDown (Touch)", () => OVRInput.GetDown(OVRInput.Touch.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryIndexTriggerUp (Touch)", () => OVRInput.GetUp(OVRInput.Touch.PrimaryIndexTrigger)),
			new BoolMonitor("PrimaryHandTrigger", () => OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)),
			new BoolMonitor("PrimaryHandTriggerDown", () => OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger)),
			new BoolMonitor("PrimaryHandTriggerUp", () => OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger)),
			new BoolMonitor("Up", () => OVRInput.Get(OVRInput.Button.Up)),
			new BoolMonitor("Down", () => OVRInput.Get(OVRInput.Button.Down)),
			new BoolMonitor("Left", () => OVRInput.Get(OVRInput.Button.Left)),
			new BoolMonitor("Right", () => OVRInput.Get(OVRInput.Button.Right)),
			new BoolMonitor("Start", () => OVRInput.Get(OVRInput.RawButton.Start)),
			new BoolMonitor("StartDown", () => OVRInput.GetDown(OVRInput.RawButton.Start)),
			new BoolMonitor("StartUp", () => OVRInput.GetUp(OVRInput.RawButton.Start)),
			new BoolMonitor("Back", () => OVRInput.Get(OVRInput.RawButton.Back)),
			new BoolMonitor("BackDown", () => OVRInput.GetDown(OVRInput.RawButton.Back)),
			new BoolMonitor("BackUp", () => OVRInput.GetUp(OVRInput.RawButton.Back)),
			new BoolMonitor("A", () => OVRInput.Get(OVRInput.RawButton.A)),
			new BoolMonitor("ADown", () => OVRInput.GetDown(OVRInput.RawButton.A)),
			new BoolMonitor("AUp", () => OVRInput.GetUp(OVRInput.RawButton.A))
		};
	}

	private void Update()
	{
		OVRInput.Controller activeController = OVRInput.GetActiveController();
		data.Length = 0;
		byte controllerBatteryPercentRemaining = OVRInput.GetControllerBatteryPercentRemaining();
		data.AppendFormat("Battery: {0}\n", controllerBatteryPercentRemaining);
		float appFramerate = OVRPlugin.GetAppFramerate();
		data.AppendFormat("Framerate: {0:F2}\n", appFramerate);
		string arg = activeController.ToString();
		data.AppendFormat("Active: {0}\n", arg);
		string arg2 = OVRInput.GetConnectedControllers().ToString();
		data.AppendFormat("Connected: {0}\n", arg2);
		data.AppendFormat("PrevConnected: {0}\n", prevConnected);
		controllers.Update();
		controllers.AppendToStringBuilder(ref data);
		prevConnected = arg2;
		Quaternion localControllerRotation = OVRInput.GetLocalControllerRotation(activeController);
		data.AppendFormat("Orientation: ({0:F2}, {1:F2}, {2:F2}, {3:F2})\n", localControllerRotation.x, localControllerRotation.y, localControllerRotation.z, localControllerRotation.w);
		Vector3 localControllerAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(activeController);
		data.AppendFormat("AngVel: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAngularVelocity.x, localControllerAngularVelocity.y, localControllerAngularVelocity.z);
		Vector3 localControllerAngularAcceleration = OVRInput.GetLocalControllerAngularAcceleration(activeController);
		data.AppendFormat("AngAcc: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAngularAcceleration.x, localControllerAngularAcceleration.y, localControllerAngularAcceleration.z);
		Vector3 localControllerPosition = OVRInput.GetLocalControllerPosition(activeController);
		data.AppendFormat("Position: ({0:F2}, {1:F2}, {2:F2})\n", localControllerPosition.x, localControllerPosition.y, localControllerPosition.z);
		Vector3 localControllerVelocity = OVRInput.GetLocalControllerVelocity(activeController);
		data.AppendFormat("Vel: ({0:F2}, {1:F2}, {2:F2})\n", localControllerVelocity.x, localControllerVelocity.y, localControllerVelocity.z);
		Vector3 localControllerAcceleration = OVRInput.GetLocalControllerAcceleration(activeController);
		data.AppendFormat("Acc: ({0:F2}, {1:F2}, {2:F2})\n", localControllerAcceleration.x, localControllerAcceleration.y, localControllerAcceleration.z);
		float num = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
		data.AppendFormat("PrimaryIndexTriggerAxis1D: ({0:F2})\n", num);
		float num2 = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
		data.AppendFormat("PrimaryHandTriggerAxis1D: ({0:F2})\n", num2);
		for (int i = 0; i < monitors.Count; i++)
		{
			monitors[i].Update();
			monitors[i].AppendToStringBuilder(ref data);
		}
		if (uiText != null)
		{
			uiText.text = data.ToString();
		}
	}
}
