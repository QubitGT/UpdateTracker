using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public class UpdateCanvasSortingOrder : MonoBehaviour
{
	public void SetCanvasSortingOrder(int sortingOrder)
	{
		Canvas[] componentsInChildren = base.transform.parent.gameObject.GetComponentsInChildren<Canvas>();
		if (componentsInChildren != null)
		{
			Canvas[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].sortingOrder = sortingOrder;
			}
		}
	}
}
