using System;
using UnityEngine;

namespace Unity.XR.CoreUtils;

[AddComponentMenu("")]
[ExecuteInEditMode]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.core-utils@2.0/api/Unity.XR.CoreUtils.OnDestroyNotifier.html")]
public class OnDestroyNotifier : MonoBehaviour
{
	public Action<OnDestroyNotifier> Destroyed { private get; set; }

	private void OnDestroy()
	{
		Destroyed?.Invoke(this);
	}
}
