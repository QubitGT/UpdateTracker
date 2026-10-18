using System.Text;
using UnityEngine;

namespace Unity.Cinemachine;

public class CinemachineBlend
{
	public interface IBlender
	{
		CameraState GetIntermediateState(ICinemachineCamera CamA, ICinemachineCamera CamB, float t);
	}

	public ICinemachineCamera CamA;

	public ICinemachineCamera CamB;

	public AnimationCurve BlendCurve;

	public float TimeInBlend;

	public float Duration;

	public float BlendWeight
	{
		get
		{
			if (BlendCurve == null || BlendCurve.length < 2 || IsComplete)
			{
				return 1f;
			}
			return Mathf.Clamp01(BlendCurve.Evaluate(TimeInBlend / Duration));
		}
	}

	public IBlender CustomBlender { get; set; }

	public bool IsValid
	{
		get
		{
			if (CamA == null || !CamA.IsValid)
			{
				if (CamB != null)
				{
					return CamB.IsValid;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsComplete
	{
		get
		{
			if (!(TimeInBlend >= Duration))
			{
				return !IsValid;
			}
			return true;
		}
	}

	public string Description
	{
		get
		{
			if (CamB == null || !CamB.IsValid)
			{
				return "(none)";
			}
			StringBuilder stringBuilder = CinemachineDebug.SBFromPool();
			stringBuilder.Append(CamB.Name);
			stringBuilder.Append(" ");
			stringBuilder.Append((int)(BlendWeight * 100f));
			stringBuilder.Append("% from ");
			if (CamA == null || !CamA.IsValid)
			{
				stringBuilder.Append("(none)");
			}
			else
			{
				stringBuilder.Append(CamA.Name);
			}
			string result = stringBuilder.ToString();
			CinemachineDebug.ReturnToPool(stringBuilder);
			return result;
		}
	}

	public CameraState State
	{
		get
		{
			if (CamA == null || !CamA.IsValid)
			{
				if (CamB == null || !CamB.IsValid)
				{
					return CameraState.Default;
				}
				return CamB.State;
			}
			if (CamB == null || !CamB.IsValid)
			{
				return CamA.State;
			}
			if (CustomBlender != null)
			{
				return CustomBlender.GetIntermediateState(CamA, CamB, BlendWeight);
			}
			CameraState stateA = CamA.State;
			CameraState stateB = CamB.State;
			return CameraState.Lerp(in stateA, in stateB, BlendWeight);
		}
	}

	public bool Uses(ICinemachineCamera cam)
	{
		if (cam == null)
		{
			return false;
		}
		if (cam == CamA || cam == CamB)
		{
			return true;
		}
		if (CamA is NestedBlendSource nestedBlendSource && nestedBlendSource.Blend.Uses(cam))
		{
			return true;
		}
		if (CamB is NestedBlendSource nestedBlendSource2)
		{
			return nestedBlendSource2.Blend.Uses(cam);
		}
		return false;
	}

	public void CopyFrom(CinemachineBlend src)
	{
		CamA = src.CamA;
		CamB = src.CamB;
		BlendCurve = src.BlendCurve;
		TimeInBlend = src.TimeInBlend;
		Duration = src.Duration;
		CustomBlender = src.CustomBlender;
	}

	public void ClearBlend()
	{
		CamA = null;
		BlendCurve = null;
		TimeInBlend = (Duration = 0f);
		CustomBlender = null;
	}

	public void UpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		if (CamA != null && CamA.IsValid)
		{
			CamA.UpdateCameraState(worldUp, deltaTime);
		}
		if (CamB != null && CamB.IsValid)
		{
			CamB.UpdateCameraState(worldUp, deltaTime);
		}
	}
}
