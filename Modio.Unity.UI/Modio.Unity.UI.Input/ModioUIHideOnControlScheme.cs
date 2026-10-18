using UnityEngine;

namespace Modio.Unity.UI.Input;

public class ModioUIHideOnControlScheme : MonoBehaviour
{
	[SerializeField]
	private bool _showOnController;

	[SerializeField]
	private bool _showOnKBM;

	[SerializeField]
	private GameObject[] _objectsToHide;

	private void OnEnable()
	{
		ModioUIInput.SwappedControlScheme += OnSwappedToController;
		OnSwappedToController(ModioUIInput.IsUsingGamepad);
	}

	private void OnDisable()
	{
		ModioUIInput.SwappedControlScheme -= OnSwappedToController;
	}

	private void OnSwappedToController(bool isController)
	{
		GameObject[] objectsToHide = _objectsToHide;
		for (int i = 0; i < objectsToHide.Length; i++)
		{
			objectsToHide[i].SetActive(isController ? _showOnController : _showOnKBM);
		}
	}
}
