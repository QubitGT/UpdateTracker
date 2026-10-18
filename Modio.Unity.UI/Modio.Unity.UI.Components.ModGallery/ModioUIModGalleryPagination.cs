using UnityEngine;
using UnityEngine.EventSystems;

namespace Modio.Unity.UI.Components.ModGallery;

public class ModioUIModGalleryPagination : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	internal ModioUIModGallery Gallery;

	internal int Index;

	[SerializeField]
	private GameObject _inactiveGameObject;

	[SerializeField]
	private GameObject _activeGameObject;

	public void OnPointerClick(PointerEventData eventData)
	{
		if ((bool)Gallery)
		{
			Gallery.GoTo(Index);
		}
	}

	public void SetState(bool active)
	{
		if (_inactiveGameObject != null)
		{
			_inactiveGameObject.SetActive(!active);
		}
		if (_activeGameObject != null)
		{
			_activeGameObject.SetActive(active);
		}
	}
}
