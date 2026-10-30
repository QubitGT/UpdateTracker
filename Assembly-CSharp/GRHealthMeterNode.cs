using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Health Meter Node")]
public class GRHealthMeterNode : MonoBehaviour
{
	public GameObject showFull;

	public GameObject showEmpty;

	private bool isEmpty;

	public void Setup()
	{
		isEmpty = true;
		SetEmpty(empty: false);
	}

	public void SetEmpty(bool empty)
	{
		if (isEmpty != empty)
		{
			isEmpty = empty;
			if (showFull != null)
			{
				showFull.SetActive(!isEmpty);
			}
			if (showEmpty != null)
			{
				showEmpty.SetActive(isEmpty);
			}
		}
	}
}
