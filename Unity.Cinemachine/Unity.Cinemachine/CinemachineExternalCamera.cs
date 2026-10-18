using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[AddComponentMenu("Cinemachine/Cinemachine External Camera")]
[ExecuteAlways]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineExternalCamera.html")]
public class CinemachineExternalCamera : CinemachineVirtualCameraBase
{
	[Tooltip("Hint for transitioning to and from this CinemachineCamera.  Hints can be combined, although not all combinations make sense.  In the case of conflicting hints, Cinemachine will make an arbitrary choice.")]
	[FormerlySerializedAs("m_PositionBlending")]
	[FormerlySerializedAs("m_BlendHint")]
	public CinemachineCore.BlendHints BlendHint;

	[Tooltip("The object that the camera is looking at.  Setting this may improve the quality of the blends to and from this camera")]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_LookAt")]
	public Transform LookAtTarget;

	private Camera m_Camera;

	private CameraState m_State = CameraState.Default;

	public override CameraState State => m_State;

	public override Transform LookAt
	{
		get
		{
			return LookAtTarget;
		}
		set
		{
			LookAtTarget = value;
		}
	}

	[HideInInspector]
	public override Transform Follow { get; set; }

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		if (m_Camera == null)
		{
			TryGetComponent<Camera>(out m_Camera);
		}
		m_State = CameraState.Default;
		m_State.RawPosition = base.transform.position;
		m_State.RawOrientation = base.transform.rotation;
		m_State.ReferenceUp = m_State.RawOrientation * Vector3.up;
		if (m_Camera != null)
		{
			m_State.Lens = LensSettings.FromCamera(m_Camera);
		}
		if (LookAtTarget != null)
		{
			m_State.ReferenceLookAt = LookAtTarget.transform.position;
			Vector3 vector = m_State.ReferenceLookAt - State.RawPosition;
			if (!vector.AlmostZero())
			{
				m_State.ReferenceLookAt = m_State.RawPosition + Vector3.Project(vector, State.RawOrientation * Vector3.forward);
			}
		}
		m_State.BlendHint = (CameraState.BlendHints)BlendHint;
		InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Finalize, ref m_State, deltaTime);
	}
}
