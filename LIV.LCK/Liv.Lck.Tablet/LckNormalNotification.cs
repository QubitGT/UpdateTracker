using TMPro;
using UnityEngine;

namespace Liv.Lck.Tablet;

public class LckNormalNotification : LckBaseNotification
{
	[field: SerializeField]
	[field: Header("UI References")]
	public GameObject UI { get; private set; }

	[field: SerializeField]
	public TMP_Text Text { get; private set; }
}
