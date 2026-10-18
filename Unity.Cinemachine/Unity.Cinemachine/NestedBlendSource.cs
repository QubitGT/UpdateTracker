using UnityEngine;

namespace Unity.Cinemachine;

public class NestedBlendSource : ICinemachineCamera
{
	private string m_Name;

	public CinemachineBlend Blend { get; internal set; }

	public string Name
	{
		get
		{
			if (m_Name == null)
			{
				m_Name = ((Blend == null || Blend.CamB == null) ? "(null)" : ("mid-blend to " + Blend.CamB.Name));
			}
			return m_Name;
		}
	}

	public string Description
	{
		get
		{
			if (Blend != null)
			{
				return Blend.Description;
			}
			return "(null)";
		}
	}

	public CameraState State { get; private set; }

	public bool IsValid
	{
		get
		{
			if (Blend != null)
			{
				return Blend.IsValid;
			}
			return false;
		}
	}

	public ICinemachineMixer ParentCamera => null;

	public NestedBlendSource(CinemachineBlend blend)
	{
		Blend = blend;
	}

	public void UpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		if (Blend != null)
		{
			Blend.UpdateCameraState(worldUp, deltaTime);
			State = Blend.State;
		}
	}

	public void OnCameraActivated(ICinemachineCamera.ActivationEventParams evt)
	{
	}
}
