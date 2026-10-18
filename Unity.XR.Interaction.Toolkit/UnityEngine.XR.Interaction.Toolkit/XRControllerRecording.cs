using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit;

[Serializable]
[CreateAssetMenu(menuName = "XR/XR Controller Recording")]
[PreferBinarySerialization]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRControllerRecording.html")]
public class XRControllerRecording : ScriptableObject, ISerializationCallbackReceiver
{
	[SerializeField]
	private bool m_SelectActivatedInFirstFrame;

	[SerializeField]
	private bool m_ActivateActivatedInFirstFrame;

	[SerializeField]
	private bool m_FirstUIPressActivatedInFirstFrame;

	[SerializeField]
	private List<XRControllerState> m_Frames = new List<XRControllerState>();

	public List<XRControllerState> frames => m_Frames;

	public double duration
	{
		get
		{
			if (m_Frames.Count != 0)
			{
				return m_Frames[m_Frames.Count - 1].time;
			}
			return 0.0;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (m_Frames != null && m_Frames.Count > 0)
		{
			XRControllerState xRControllerState = m_Frames[0];
			m_SelectActivatedInFirstFrame = xRControllerState.selectInteractionState.activatedThisFrame;
			m_ActivateActivatedInFirstFrame = xRControllerState.activateInteractionState.activatedThisFrame;
			m_FirstUIPressActivatedInFirstFrame = xRControllerState.uiPressInteractionState.activatedThisFrame;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		SetFrameDependentData();
	}

	internal void SetFrameDependentData()
	{
		if (m_Frames != null && m_Frames.Count > 0)
		{
			XRControllerState xRControllerState = m_Frames[0];
			xRControllerState.selectInteractionState.SetFrameDependent(!m_SelectActivatedInFirstFrame && xRControllerState.selectInteractionState.active);
			xRControllerState.activateInteractionState.SetFrameDependent(!m_ActivateActivatedInFirstFrame && xRControllerState.activateInteractionState.active);
			xRControllerState.uiPressInteractionState.SetFrameDependent(!m_FirstUIPressActivatedInFirstFrame && xRControllerState.uiPressInteractionState.active);
			for (int i = 1; i < m_Frames.Count; i++)
			{
				XRControllerState xRControllerState2 = m_Frames[i];
				XRControllerState xRControllerState3 = m_Frames[i - 1];
				xRControllerState2.selectInteractionState.SetFrameDependent(xRControllerState3.selectInteractionState.active);
				xRControllerState2.activateInteractionState.SetFrameDependent(xRControllerState3.activateInteractionState.active);
				xRControllerState2.uiPressInteractionState.SetFrameDependent(xRControllerState3.uiPressInteractionState.active);
			}
		}
	}

	public void AddRecordingFrame(XRControllerState state)
	{
		frames.Add(new XRControllerState(state));
	}

	public void AddRecordingFrameNonAlloc(XRControllerState state)
	{
		frames.Add(state);
	}

	public void InitRecording()
	{
		m_SelectActivatedInFirstFrame = false;
		m_ActivateActivatedInFirstFrame = false;
		m_FirstUIPressActivatedInFirstFrame = false;
		m_Frames.Clear();
	}

	public void SaveRecording()
	{
	}

	[Obsolete("AddRecordingFrame has been deprecated. Use the overload method with the XRControllerState parameter or the method AddRecordingFrameNonAlloc.", true)]
	public void AddRecordingFrame(double time, Vector3 position, Quaternion rotation, bool selectActive, bool activateActive, bool pressActive)
	{
	}
}
