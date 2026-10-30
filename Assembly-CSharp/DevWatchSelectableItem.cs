using System;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DevWatchSelectableItem : MonoBehaviour
{
	public Button Button;

	public TextMeshProUGUI ItemName;

	public PhotonView SelectedObject;

	public Action<string, PhotonView> OnSelected;

	public void Init(PhotonView obj)
	{
		SelectedObject = obj;
		ItemName.text = obj.name;
		Button.onClick.AddListener(delegate
		{
			OnSelected(ItemName.text, SelectedObject);
		});
	}
}
