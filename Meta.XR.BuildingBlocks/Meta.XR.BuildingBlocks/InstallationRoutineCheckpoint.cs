using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.BuildingBlocks;

[Serializable]
public class InstallationRoutineCheckpoint
{
	[SerializeField]
	[HideInInspector]
	private string _installationRoutineId;

	[SerializeField]
	[HideInInspector]
	private List<VariantCheckpoint> _installationVariants;

	public string InstallationRoutineId => _installationRoutineId;

	public List<VariantCheckpoint> InstallationVariants => _installationVariants;

	public InstallationRoutineCheckpoint(string installationRoutineId, List<VariantCheckpoint> installationVariants)
	{
		_installationRoutineId = installationRoutineId;
		_installationVariants = installationVariants;
	}
}
