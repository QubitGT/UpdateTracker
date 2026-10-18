using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

[CreateAssetMenu(menuName = "Meta/Interaction/SDK/Pose Authoring/HandGrabInteractable Data Collection")]
public class HandGrabInteractableDataCollection : ScriptableObject
{
	[SerializeField]
	[Tooltip("Do not modify this manually unless you are sure! Instead load the HandGrabInteractable and use the tools provided.")]
	private List<HandGrabUtils.HandGrabInteractableData> _interactablesData;

	public List<HandGrabUtils.HandGrabInteractableData> InteractablesData => _interactablesData;

	public void StoreInteractables(List<HandGrabUtils.HandGrabInteractableData> interactablesData)
	{
		_interactablesData = interactablesData;
	}
}
