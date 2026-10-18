using System.Collections.Generic;

namespace Unity.Cinemachine;

internal sealed class VirtualCameraRegistry
{
	private readonly List<CinemachineVirtualCameraBase> m_ActiveCameras = new List<CinemachineVirtualCameraBase>();

	private readonly List<List<CinemachineVirtualCameraBase>> m_AllCameras = new List<List<CinemachineVirtualCameraBase>>();

	private bool m_ActiveCamerasAreSorted;

	private int m_ActivationSequence;

	public List<List<CinemachineVirtualCameraBase>> AllCamerasSortedByNestingLevel => m_AllCameras;

	public int ActiveCameraCount => m_ActiveCameras.Count;

	public CinemachineVirtualCameraBase GetActiveCamera(int index)
	{
		if (!m_ActiveCamerasAreSorted && m_ActiveCameras.Count > 1)
		{
			m_ActiveCameras.Sort((CinemachineVirtualCameraBase x, CinemachineVirtualCameraBase y) => (x.Priority.Value != y.Priority.Value) ? y.Priority.Value.CompareTo(x.Priority.Value) : y.ActivationId.CompareTo(x.ActivationId));
			m_ActiveCamerasAreSorted = true;
		}
		return m_ActiveCameras[index];
	}

	public void AddActiveCamera(CinemachineVirtualCameraBase vcam)
	{
		vcam.ActivationId = m_ActivationSequence++;
		m_ActiveCameras.Add(vcam);
		m_ActiveCamerasAreSorted = false;
	}

	public void RemoveActiveCamera(CinemachineVirtualCameraBase vcam)
	{
		if (m_ActiveCameras.Contains(vcam))
		{
			m_ActiveCameras.Remove(vcam);
		}
	}

	public void CameraDestroyed(CinemachineVirtualCameraBase vcam)
	{
		if (m_ActiveCameras.Contains(vcam))
		{
			m_ActiveCameras.Remove(vcam);
		}
	}

	public void CameraEnabled(CinemachineVirtualCameraBase vcam)
	{
		int num = 0;
		for (ICinemachineMixer parentCamera = vcam.ParentCamera; parentCamera != null; parentCamera = parentCamera.ParentCamera)
		{
			num++;
		}
		while (m_AllCameras.Count <= num)
		{
			m_AllCameras.Add(new List<CinemachineVirtualCameraBase>());
		}
		m_AllCameras[num].Add(vcam);
	}

	public void CameraDisabled(CinemachineVirtualCameraBase vcam)
	{
		for (int i = 0; i < m_AllCameras.Count; i++)
		{
			m_AllCameras[i].Remove(vcam);
		}
	}
}
