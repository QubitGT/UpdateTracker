using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckSplitNotification : LckBaseNotification
{
	[field: SerializeField]
	[field: Header("UI References")]
	public GameObject AndroidUI { get; private set; }

	[field: SerializeField]
	public GameObject DesktopUI { get; private set; }

	private void Start()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidUI.SetActive(value: true);
			DesktopUI.SetActive(value: false);
		}
		else
		{
			DesktopUI.SetActive(value: true);
			AndroidUI.SetActive(value: false);
		}
	}
}
