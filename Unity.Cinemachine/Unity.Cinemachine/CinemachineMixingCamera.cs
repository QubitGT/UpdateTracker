using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[AddComponentMenu("Cinemachine/Cinemachine Mixing Camera")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineMixingCamera.html")]
public class CinemachineMixingCamera : CinemachineCameraManagerBase
{
	public const int MaxCameras = 8;

	[Tooltip("The weight of the first tracked camera")]
	[FormerlySerializedAs("m_Weight0")]
	public float Weight0 = 0.5f;

	[Tooltip("The weight of the second tracked camera")]
	[FormerlySerializedAs("m_Weight1")]
	public float Weight1 = 0.5f;

	[Tooltip("The weight of the third tracked camera")]
	[FormerlySerializedAs("m_Weight2")]
	public float Weight2 = 0.5f;

	[Tooltip("The weight of the fourth tracked camera")]
	[FormerlySerializedAs("m_Weight3")]
	public float Weight3 = 0.5f;

	[Tooltip("The weight of the fifth tracked camera")]
	[FormerlySerializedAs("m_Weight4")]
	public float Weight4 = 0.5f;

	[Tooltip("The weight of the sixth tracked camera")]
	[FormerlySerializedAs("m_Weight5")]
	public float Weight5 = 0.5f;

	[Tooltip("The weight of the seventh tracked camera")]
	[FormerlySerializedAs("m_Weight6")]
	public float Weight6 = 0.5f;

	[Tooltip("The weight of the eighth tracked camera")]
	[FormerlySerializedAs("m_Weight7")]
	public float Weight7 = 0.5f;

	private CameraState m_CameraState = CameraState.Default;

	private Dictionary<CinemachineVirtualCameraBase, int> m_IndexMap;

	private float m_LiveChildPercent;

	public override CameraState State => m_CameraState;

	public override string Description
	{
		get
		{
			if (base.LiveChild == null)
			{
				return "[(none)]";
			}
			StringBuilder stringBuilder = CinemachineDebug.SBFromPool();
			stringBuilder.Append("[");
			stringBuilder.Append(base.LiveChild.Name);
			stringBuilder.Append(" ");
			stringBuilder.Append(Mathf.RoundToInt(m_LiveChildPercent));
			stringBuilder.Append("%]");
			string result = stringBuilder.ToString();
			CinemachineDebug.ReturnToPool(stringBuilder);
			return result;
		}
	}

	private void OnValidate()
	{
		for (int i = 0; i < 8; i++)
		{
			SetWeight(i, Mathf.Max(0f, GetWeight(i)));
		}
	}

	protected override void Reset()
	{
		base.Reset();
		for (int i = 0; i < 8; i++)
		{
			SetWeight(i, (i == 0) ? 1 : 0);
		}
	}

	public float GetWeight(int index)
	{
		switch (index)
		{
		case 0:
			return Weight0;
		case 1:
			return Weight1;
		case 2:
			return Weight2;
		case 3:
			return Weight3;
		case 4:
			return Weight4;
		case 5:
			return Weight5;
		case 6:
			return Weight6;
		case 7:
			return Weight7;
		default:
			Debug.LogError("CinemachineMixingCamera: Invalid index: " + index);
			return 0f;
		}
	}

	public void SetWeight(int index, float w)
	{
		switch (index)
		{
		case 0:
			Weight0 = w;
			break;
		case 1:
			Weight1 = w;
			break;
		case 2:
			Weight2 = w;
			break;
		case 3:
			Weight3 = w;
			break;
		case 4:
			Weight4 = w;
			break;
		case 5:
			Weight5 = w;
			break;
		case 6:
			Weight6 = w;
			break;
		case 7:
			Weight7 = w;
			break;
		default:
			Debug.LogError("CinemachineMixingCamera: Invalid index: " + index);
			break;
		}
	}

	public float GetWeight(CinemachineVirtualCameraBase vcam)
	{
		UpdateCameraCache();
		if (m_IndexMap.TryGetValue(vcam, out var value))
		{
			return GetWeight(value);
		}
		return 0f;
	}

	public void SetWeight(CinemachineVirtualCameraBase vcam, float w)
	{
		UpdateCameraCache();
		if (m_IndexMap.TryGetValue(vcam, out var value))
		{
			SetWeight(value, w);
		}
		else
		{
			Debug.LogError("CinemachineMixingCamera: Invalid child: " + ((vcam != null) ? vcam.Name : "(null)"));
		}
	}

	public override bool IsLiveChild(ICinemachineCamera vcam, bool dominantChildOnly = false)
	{
		if (dominantChildOnly)
		{
			return base.LiveChild == vcam;
		}
		List<CinemachineVirtualCameraBase> childCameras = base.ChildCameras;
		for (int i = 0; i < 8 && i < childCameras.Count; i++)
		{
			if (childCameras[i] == vcam)
			{
				if (GetWeight(i) > 0.0001f)
				{
					return childCameras[i].isActiveAndEnabled;
				}
				return false;
			}
		}
		return false;
	}

	protected override bool UpdateCameraCache()
	{
		if (!base.UpdateCameraCache())
		{
			return false;
		}
		m_IndexMap = new Dictionary<CinemachineVirtualCameraBase, int>();
		for (int i = 0; i < base.ChildCameras.Count; i++)
		{
			m_IndexMap.Add(base.ChildCameras[i], i);
		}
		return true;
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		for (int i = 0; i < 8 && i < base.ChildCameras.Count; i++)
		{
			base.ChildCameras[i].OnTransitionFromCamera(fromCam, worldUp, deltaTime);
		}
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		UpdateCameraCache();
		CinemachineVirtualCameraBase activeCamera = null;
		List<CinemachineVirtualCameraBase> childCameras = base.ChildCameras;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < 8 && i < childCameras.Count; i++)
		{
			CinemachineVirtualCameraBase cinemachineVirtualCameraBase = childCameras[i];
			if (!cinemachineVirtualCameraBase.isActiveAndEnabled)
			{
				continue;
			}
			float num3 = Mathf.Max(0f, GetWeight(i));
			if (num3 > 0.0001f)
			{
				num2 += num3;
				if (num2 == num3)
				{
					m_CameraState = cinemachineVirtualCameraBase.State;
				}
				else
				{
					ref CameraState cameraState = ref m_CameraState;
					CameraState stateB = cinemachineVirtualCameraBase.State;
					m_CameraState = CameraState.Lerp(in cameraState, in stateB, num3 / num2);
				}
				if (num3 > num)
				{
					num = num3;
					activeCamera = cinemachineVirtualCameraBase;
				}
			}
		}
		m_LiveChildPercent = ((num2 > 0.001f) ? (num * 100f / num2) : 0f);
		SetLiveChild(activeCamera, worldUp, deltaTime);
		InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Finalize, ref m_CameraState, deltaTime);
		PreviousStateIsValid = true;
	}

	protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime)
	{
		return null;
	}
}
