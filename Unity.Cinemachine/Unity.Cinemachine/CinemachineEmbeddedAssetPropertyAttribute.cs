using UnityEngine;

namespace Unity.Cinemachine;

public sealed class CinemachineEmbeddedAssetPropertyAttribute : PropertyAttribute
{
	public bool WarnIfNull;

	public CinemachineEmbeddedAssetPropertyAttribute(bool warnIfNull = false)
	{
		WarnIfNull = warnIfNull;
	}
}
