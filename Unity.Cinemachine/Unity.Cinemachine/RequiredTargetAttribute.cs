using System;

namespace Unity.Cinemachine;

public sealed class RequiredTargetAttribute : Attribute
{
	public enum RequiredTargets
	{
		None,
		Tracking,
		LookAt,
		GroupLookAt
	}

	public RequiredTargets RequiredTarget { get; private set; }

	public RequiredTargetAttribute(RequiredTargets requiredTarget)
	{
		RequiredTarget = requiredTarget;
	}
}
