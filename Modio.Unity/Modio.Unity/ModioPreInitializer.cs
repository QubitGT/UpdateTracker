using Modio.Extensions;
using UnityEngine;

namespace Modio.Unity;

public class ModioPreInitializer : MonoBehaviour
{
	private void Start()
	{
		if (!ModioClient.IsInitialized)
		{
			ModioClient.Init().ForgetTaskSafely();
		}
	}
}
