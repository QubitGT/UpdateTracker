using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Cinemachine Sequencer Camera")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSequencerCamera.html")]
public class CinemachineSequencerCamera : CinemachineCameraManagerBase
{
	[Serializable]
	public struct Instruction
	{
		[Tooltip("The camera to activate when this instruction becomes active")]
		[FormerlySerializedAs("m_VirtualCamera")]
		[ChildCameraProperty]
		public CinemachineVirtualCameraBase Camera;

		[Tooltip("How to blend to the next camera in the list (if any)")]
		[FormerlySerializedAs("m_Blend")]
		public CinemachineBlendDefinition Blend;

		[Tooltip("How long to wait (in seconds) before activating the next camera in the list (if any)")]
		[FormerlySerializedAs("m_Hold")]
		public float Hold;

		public void Validate()
		{
			Hold = Mathf.Max(Hold, 0f);
		}
	}

	[Tooltip("When enabled, the child vcams will cycle indefinitely instead of just stopping at the last one")]
	[FormerlySerializedAs("m_Loop")]
	public bool Loop;

	[Tooltip("The set of instructions for enabling child cameras.")]
	[FormerlySerializedAs("m_Instructions")]
	public List<Instruction> Instructions = new List<Instruction>();

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_LookAt")]
	private Transform m_LegacyLookAt;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("m_Follow")]
	private Transform m_LegacyFollow;

	private float m_ActivationTime = -1f;

	private int m_CurrentInstruction;

	protected override void Reset()
	{
		base.Reset();
		Loop = false;
		Instructions = null;
	}

	private void OnValidate()
	{
		if (Instructions != null)
		{
			for (int i = 0; i < Instructions.Count; i++)
			{
				Instruction value = Instructions[i];
				value.Validate();
				Instructions[i] = value;
			}
		}
	}

	protected internal override void PerformLegacyUpgrade(int streamedVersion)
	{
		base.PerformLegacyUpgrade(streamedVersion);
		if (streamedVersion < 20220721 && (m_LegacyLookAt != null || m_LegacyFollow != null))
		{
			DefaultTarget = new DefaultTargetSettings
			{
				Enabled = true,
				Target = new CameraTarget
				{
					LookAtTarget = m_LegacyLookAt,
					TrackingTarget = m_LegacyFollow,
					CustomLookAtTarget = (m_LegacyLookAt != m_LegacyFollow)
				}
			};
			m_LegacyLookAt = (m_LegacyFollow = null);
		}
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		m_ActivationTime = CinemachineCore.CurrentTime;
		m_CurrentInstruction = 0;
	}

	protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
	{
		if (!PreviousStateIsValid)
		{
			m_CurrentInstruction = -1;
		}
		AdvanceCurrentInstruction(deltaTime);
		if (m_CurrentInstruction < 0 || m_CurrentInstruction >= Instructions.Count)
		{
			return null;
		}
		return Instructions[m_CurrentInstruction].Camera;
	}

	protected override CinemachineBlendDefinition LookupBlend(ICinemachineCamera outgoing, ICinemachineCamera incoming)
	{
		return Instructions[m_CurrentInstruction].Blend;
	}

	protected override bool UpdateCameraCache()
	{
		if (Instructions == null)
		{
			Instructions = new List<Instruction>();
		}
		return base.UpdateCameraCache();
	}

	private void AdvanceCurrentInstruction(float deltaTime)
	{
		if (base.ChildCameras == null || base.ChildCameras.Count == 0 || m_ActivationTime < 0f || Instructions.Count == 0)
		{
			m_ActivationTime = -1f;
			m_CurrentInstruction = -1;
			return;
		}
		float currentTime = CinemachineCore.CurrentTime;
		if (m_CurrentInstruction < 0 || deltaTime < 0f)
		{
			m_ActivationTime = currentTime;
			m_CurrentInstruction = 0;
		}
		if (m_CurrentInstruction > Instructions.Count - 1)
		{
			m_ActivationTime = currentTime;
			m_CurrentInstruction = Instructions.Count - 1;
		}
		float b = Instructions[m_CurrentInstruction].Hold + Instructions[m_CurrentInstruction].Blend.BlendTime;
		float a = ((m_CurrentInstruction < Instructions.Count - 1 || Loop) ? 0f : float.MaxValue);
		if (currentTime - m_ActivationTime > Mathf.Max(a, b))
		{
			m_ActivationTime = currentTime;
			m_CurrentInstruction++;
			if (Loop && m_CurrentInstruction == Instructions.Count)
			{
				m_CurrentInstruction = 0;
			}
		}
	}
}
