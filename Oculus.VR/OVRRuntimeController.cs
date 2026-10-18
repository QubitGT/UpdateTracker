using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-runtime-controller/")]
public class OVRRuntimeController : MonoBehaviour
{
	public OVRInput.Controller m_controller;

	public Shader m_controllerModelShader;

	public bool m_supportAnimation = true;

	private GameObject m_controllerObject;

	private static string leftControllerModelPath = "/model_fb/controller/left";

	private static string rightControllerModelPath = "/model_fb/controller/right";

	private string m_controllerModelPath;

	private bool m_modelSupported;

	private bool m_hasInputFocus = true;

	private bool m_hasInputFocusPrev;

	private bool m_controllerConnectedPrev;

	private Dictionary<OVRGLTFInputNode, OVRGLTFAnimatinonNode> m_animationNodes;

	private void Start()
	{
		if (m_controller == OVRInput.Controller.LTouch)
		{
			m_controllerModelPath = leftControllerModelPath;
		}
		else if (m_controller == OVRInput.Controller.RTouch)
		{
			m_controllerModelPath = rightControllerModelPath;
		}
		m_modelSupported = IsModelSupported(m_controllerModelPath);
		if (m_modelSupported)
		{
			StartCoroutine(UpdateControllerModel());
		}
		OVRManager.InputFocusAcquired += InputFocusAquired;
		OVRManager.InputFocusLost += InputFocusLost;
	}

	private void Update()
	{
		bool flag = OVRInput.IsControllerConnected(m_controller);
		if (m_hasInputFocus != m_hasInputFocusPrev || flag != m_controllerConnectedPrev)
		{
			if (m_controllerObject != null)
			{
				m_controllerObject.SetActive(flag && m_hasInputFocus);
			}
			m_hasInputFocusPrev = m_hasInputFocus;
			m_controllerConnectedPrev = flag;
		}
		if (flag)
		{
			UpdateControllerAnimation();
		}
	}

	private bool IsModelSupported(string modelPath)
	{
		string[] renderModelPaths = OVRPlugin.GetRenderModelPaths();
		if (renderModelPaths.Length == 0)
		{
			Debug.LogError("Failed to enumerate model paths from the runtime. Check that the render model feature is enabled in OVRManager.");
			return false;
		}
		for (int i = 0; i < renderModelPaths.Length; i++)
		{
			if (renderModelPaths[i].Equals(modelPath))
			{
				return true;
			}
		}
		Debug.LogError("Render model path " + modelPath + " not supported by this device.");
		return false;
	}

	private bool LoadControllerModel(string modelPath)
	{
		OVRPlugin.RenderModelProperties modelProperties = default(OVRPlugin.RenderModelProperties);
		if (OVRPlugin.GetRenderModelProperties(modelPath, ref modelProperties))
		{
			if (modelProperties.ModelKey != 0L)
			{
				byte[] array = OVRPlugin.LoadRenderModel(modelProperties.ModelKey);
				if (array != null)
				{
					OVRGLTFLoader oVRGLTFLoader = new OVRGLTFLoader(array);
					oVRGLTFLoader.SetModelShader(m_controllerModelShader);
					OVRGLTFScene oVRGLTFScene = oVRGLTFLoader.LoadGLB(m_supportAnimation);
					m_controllerObject = oVRGLTFScene.root;
					m_animationNodes = oVRGLTFScene.animationNodes;
					if (m_controllerObject != null)
					{
						m_controllerObject.transform.SetParent(base.transform, worldPositionStays: false);
						m_controllerObject.transform.parent.localPosition = new Vector3(0f, -0.03f, -0.04f);
						m_controllerObject.transform.parent.localRotation = Quaternion.AngleAxis(-60f, new Vector3(1f, 0f, 0f));
						return true;
					}
				}
			}
			Debug.LogError("Retrived a null model key of " + modelPath);
		}
		Debug.LogError("Failed to load controller model of " + modelPath);
		return false;
	}

	private IEnumerator UpdateControllerModel()
	{
		while (true)
		{
			bool flag = OVRInput.IsControllerConnected(m_controller);
			if (m_controllerObject == null && flag)
			{
				LoadControllerModel(m_controllerModelPath);
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	private void UpdateControllerAnimation()
	{
		if (m_animationNodes != null)
		{
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.Button_A_X))
			{
				m_animationNodes[OVRGLTFInputNode.Button_A_X].UpdatePose(OVRInput.Get((m_controller != OVRInput.Controller.LTouch) ? OVRInput.RawButton.A : OVRInput.RawButton.X));
			}
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.Button_B_Y))
			{
				m_animationNodes[OVRGLTFInputNode.Button_B_Y].UpdatePose(OVRInput.Get((m_controller == OVRInput.Controller.LTouch) ? OVRInput.RawButton.Y : OVRInput.RawButton.B));
			}
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.Button_Oculus_Menu))
			{
				m_animationNodes[OVRGLTFInputNode.Button_Oculus_Menu].UpdatePose(OVRInput.Get(OVRInput.RawButton.Start));
			}
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.Trigger_Grip))
			{
				m_animationNodes[OVRGLTFInputNode.Trigger_Grip].UpdatePose(OVRInput.Get((m_controller == OVRInput.Controller.LTouch) ? OVRInput.RawAxis1D.LHandTrigger : OVRInput.RawAxis1D.RHandTrigger));
			}
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.Trigger_Front))
			{
				m_animationNodes[OVRGLTFInputNode.Trigger_Front].UpdatePose(OVRInput.Get((m_controller == OVRInput.Controller.LTouch) ? OVRInput.RawAxis1D.LIndexTrigger : OVRInput.RawAxis1D.RIndexTrigger));
			}
			if (m_animationNodes.ContainsKey(OVRGLTFInputNode.ThumbStick))
			{
				m_animationNodes[OVRGLTFInputNode.ThumbStick].UpdatePose(OVRInput.Get((m_controller == OVRInput.Controller.LTouch) ? OVRInput.RawAxis2D.LThumbstick : OVRInput.RawAxis2D.RThumbstick));
			}
		}
	}

	public void InputFocusAquired()
	{
		m_hasInputFocus = true;
	}

	public void InputFocusLost()
	{
		m_hasInputFocus = false;
	}
}
