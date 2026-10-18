using System;
using UnityEngine;

namespace Meta.XR.BuildingBlocks;

[HelpURL("https://developer.oculus.com/documentation/unity/bb-overview/")]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class BuildingBlock : MonoBehaviour
{
	[SerializeField]
	[OVRReadOnly]
	internal string blockId;

	[SerializeField]
	[HideInInspector]
	internal string instanceId = Guid.NewGuid().ToString();

	[SerializeField]
	[OVRReadOnly]
	internal int version = 1;

	[SerializeField]
	[HideInInspector]
	private InstallationRoutineCheckpoint installationRoutineCheckpoint;

	public string BlockId => blockId;

	public string InstanceId => instanceId;

	public int Version => version;

	public InstallationRoutineCheckpoint InstallationRoutineCheckpoint
	{
		get
		{
			return installationRoutineCheckpoint;
		}
		set
		{
			installationRoutineCheckpoint = value;
		}
	}

	private void Awake()
	{
		if (!Application.isPlaying && HasDuplicateInstanceId())
		{
			ResetInstanceId();
		}
	}

	private void ResetInstanceId()
	{
		instanceId = Guid.NewGuid().ToString();
	}

	private bool HasDuplicateInstanceId()
	{
		BuildingBlock[] array = UnityEngine.Object.FindObjectsByType<BuildingBlock>(FindObjectsSortMode.InstanceID);
		foreach (BuildingBlock buildingBlock in array)
		{
			if (buildingBlock != this && buildingBlock.InstanceId == InstanceId)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		OVRTelemetry.Start(163063912, 0, -1L).AddBlockInfo(this).SendIf(Application.isPlaying);
	}
}
