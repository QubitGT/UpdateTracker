using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem;

public class ChaperoneInfo : MonoBehaviour
{
	public static SteamVR_Events.Event Initialized = new SteamVR_Events.Event();

	private static ChaperoneInfo _instance;

	public bool initialized { get; private set; }

	public float playAreaSizeX { get; private set; }

	public float playAreaSizeZ { get; private set; }

	public bool roomscale { get; private set; }

	public static ChaperoneInfo instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GameObject("[ChaperoneInfo]").AddComponent<ChaperoneInfo>();
				_instance.initialized = false;
				_instance.playAreaSizeX = 1f;
				_instance.playAreaSizeZ = 1f;
				_instance.roomscale = false;
				Object.DontDestroyOnLoad(_instance.gameObject);
			}
			return _instance;
		}
	}

	public static SteamVR_Events.Action InitializedAction(UnityAction action)
	{
		return new SteamVR_Events.ActionNoArgs(Initialized, action);
	}

	private IEnumerator Start()
	{
		CVRChaperone chaperone = OpenVR.Chaperone;
		if (chaperone == null)
		{
			Debug.LogWarning("<b>[SteamVR Interaction]</b> Failed to get IVRChaperone interface.");
			initialized = true;
			yield break;
		}
		float pSizeX;
		float pSizeZ;
		while (true)
		{
			pSizeX = 0f;
			pSizeZ = 0f;
			if (chaperone.GetPlayAreaSize(ref pSizeX, ref pSizeZ))
			{
				break;
			}
			yield return null;
		}
		initialized = true;
		playAreaSizeX = pSizeX;
		playAreaSizeZ = pSizeZ;
		roomscale = Mathf.Max(pSizeX, pSizeZ) > 1.01f;
		Debug.LogFormat("<b>[SteamVR Interaction]</b> ChaperoneInfo initialized. {2} play area {0:0.00}m x {1:0.00}m", pSizeX, pSizeZ, roomscale ? "Roomscale" : "Standing");
		Initialized.Send();
	}
}
