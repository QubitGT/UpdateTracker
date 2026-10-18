using System.Diagnostics;
using UnityEngine;

namespace Photon.Pun.UtilityScripts;

public class OnEscapeQuit : MonoBehaviour
{
	[Conditional("UNITY_ANDROID")]
	[Conditional("UNITY_IOS")]
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
