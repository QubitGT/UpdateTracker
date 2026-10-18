using System.Collections.Generic;
using System.Text;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.UI;

[Feature(Feature.Hands)]
public class OVRHandTest : MonoBehaviour
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

	private OVRPlugin.HandState hs_LH;

	private OVRPlugin.HandState hs_RH;

	private OVRPlugin.Skeleton skel_LH;

	private OVRPlugin.Skeleton skel_RH;

	private OVRPlugin.Mesh mesh_LH = new OVRPlugin.Mesh();

	private OVRPlugin.Mesh mesh_RH = new OVRPlugin.Mesh();

	private bool result_skel_LH;

	private bool result_skel_RH;

	private bool result_mesh_LH;

	private bool result_mesh_RH;

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
			new BoolMonitor("One", () => OVRInput.Get(OVRInput.Button.One))
		};
		result_skel_LH = OVRPlugin.GetSkeleton(OVRPlugin.SkeletonType.HandLeft, out skel_LH);
		result_skel_RH = OVRPlugin.GetSkeleton(OVRPlugin.SkeletonType.HandRight, out skel_RH);
		result_mesh_LH = OVRPlugin.GetMesh(OVRPlugin.MeshType.HandLeft, out mesh_LH);
		result_mesh_RH = OVRPlugin.GetMesh(OVRPlugin.MeshType.HandRight, out mesh_RH);
	}

	private void Update()
	{
		data.Length = 0;
		OVRInput.Controller activeController = OVRInput.GetActiveController();
		string arg = activeController.ToString();
		data.AppendFormat("Active: {0}\n", arg);
		string arg2 = OVRInput.GetConnectedControllers().ToString();
		data.AppendFormat("Connected: {0}\n", arg2);
		data.AppendFormat("PrevConnected: {0}\n", prevConnected);
		controllers.Update();
		controllers.AppendToStringBuilder(ref data);
		prevConnected = arg2;
		Vector3 localControllerPosition = OVRInput.GetLocalControllerPosition(activeController);
		data.AppendFormat("Position: ({0:F2}, {1:F2}, {2:F2})\n", localControllerPosition.x, localControllerPosition.y, localControllerPosition.z);
		Quaternion localControllerRotation = OVRInput.GetLocalControllerRotation(activeController);
		data.AppendFormat("Orientation: ({0:F2}, {1:F2}, {2:F2}, {3:F2})\n", localControllerRotation.x, localControllerRotation.y, localControllerRotation.z, localControllerRotation.w);
		data.AppendFormat("HandTrackingEnabled: {0}\n", OVRPlugin.GetHandTrackingEnabled());
		bool handState = OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandLeft, ref hs_LH);
		data.AppendFormat("LH HS Query Res: {0}\n", handState);
		data.AppendFormat("LH HS Status: {0}\n", hs_LH.Status);
		data.AppendFormat("LH HS Pose: {0}\n", hs_LH.RootPose);
		data.AppendFormat("LH HS HandConf: {0}\n", hs_LH.HandConfidence);
		bool handState2 = OVRPlugin.GetHandState(OVRPlugin.Step.Render, OVRPlugin.Hand.HandRight, ref hs_RH);
		data.AppendFormat("RH HS Query Res: {0}\n", handState2);
		data.AppendFormat("RH HS Status: {0}\n", hs_RH.Status);
		data.AppendFormat("RH HS Pose: {0}\n", hs_RH.RootPose);
		data.AppendFormat("RH HS HandConf: {0}\n", hs_RH.HandConfidence);
		data.AppendFormat("LH Skel Query Res: {0}\n", result_skel_LH);
		data.AppendFormat("LH Skel Type: {0}\n", skel_LH.Type);
		data.AppendFormat("LH Skel NumBones: {0}\n", skel_LH.NumBones);
		data.AppendFormat("RH Skel Query Res: {0}\n", result_skel_RH);
		data.AppendFormat("RH Skel Type: {0}\n", skel_RH.Type);
		data.AppendFormat("RH Skel NumBones: {0}\n", skel_RH.NumBones);
		data.AppendFormat("LH Mesh Query Res: {0}\n", result_mesh_LH);
		data.AppendFormat("LH Mesh Type: {0}\n", mesh_LH.Type);
		data.AppendFormat("LH Mesh NumVers: {0}\n", mesh_LH.NumVertices);
		data.AppendFormat("RH Mesh Query Res: {0}\n", result_mesh_RH);
		data.AppendFormat("RH Mesh Type: {0}\n", mesh_RH.Type);
		data.AppendFormat("RH Mesh NumVers: {0}\n", mesh_RH.NumVertices);
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
