using System;

public static class OperationResultExtensions
{
	public static bool IsSuccess(this OVRSpatialAnchor.OperationResult res)
	{
		return res == OVRSpatialAnchor.OperationResult.Success;
	}

	public static bool IsError(this OVRSpatialAnchor.OperationResult res)
	{
		return res < OVRSpatialAnchor.OperationResult.Success;
	}

	[Obsolete("There are no OperationResults that are considered warnings so this method will always return False.")]
	public static bool IsWarning(this OVRSpatialAnchor.OperationResult res)
	{
		return res > OVRSpatialAnchor.OperationResult.Success;
	}
}
