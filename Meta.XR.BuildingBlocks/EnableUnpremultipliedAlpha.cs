using UnityEngine;

public class EnableUnpremultipliedAlpha : MonoBehaviour
{
	private void Start()
	{
		OVRManager.eyeFovPremultipliedAlphaModeEnabled = false;
	}
}
