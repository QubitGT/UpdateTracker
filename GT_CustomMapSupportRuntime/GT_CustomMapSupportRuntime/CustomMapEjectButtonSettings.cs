using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class CustomMapEjectButtonSettings : MonoBehaviour
{
	public enum EjectType
	{
		EjectFromVirtualStump,
		ReturnToVirtualStump
	}

	public EjectType ejectType;
}
