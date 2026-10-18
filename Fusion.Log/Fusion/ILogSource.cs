using UnityEngine;

namespace Fusion;

public interface ILogSource
{
	Object GetUnityObject()
	{
		return this as Object;
	}
}
