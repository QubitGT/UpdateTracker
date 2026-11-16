using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BananaMailDisplay : MonoBehaviour
{
	[SerializeField]
	private UnityEvent HighlightOn;

	[SerializeField]
	private UnityEvent HighlightOff;

	private void OnEnable()
	{
		BananaMail_BananaMailStatusChange();
		BananaMail.BananaMailStatusChange += BananaMail_BananaMailStatusChange;
	}

	private void BananaMail_BananaMailStatusChange()
	{
		StopAllCoroutines();
		if (BananaMail.Status == BananaMail.BananaMailStatus.ready)
		{
			StartCoroutine(cr_Blink(BananaMail.Unread));
		}
	}

	private IEnumerator cr_Blink(int unread)
	{
		while (true)
		{
			for (int i = 0; i < unread; i++)
			{
				HighlightOn?.Invoke();
				yield return new WaitForSeconds(0.3f);
				HighlightOff?.Invoke();
				yield return new WaitForSeconds(0.2f);
			}
			yield return new WaitForSeconds(2f);
		}
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		BananaMail.BananaMailStatusChange -= BananaMail_BananaMailStatusChange;
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
		BananaMail.BananaMailStatusChange -= BananaMail_BananaMailStatusChange;
	}
}
