using System.Collections;
using UnityEngine;

namespace Drawing.Examples;

public class TimedSpawner : MonoBehaviour
{
	public float interval = 1f;

	public float lifeTime = 5f;

	public GameObject prefab;

	private IEnumerator Start()
	{
		while (true)
		{
			GameObject go = Object.Instantiate(prefab, base.transform.position + Random.insideUnitSphere * 0.01f, Random.rotation);
			StartCoroutine(DestroyAfter(go, lifeTime));
			yield return new WaitForSeconds(interval);
		}
	}

	private IEnumerator DestroyAfter(GameObject go, float delay)
	{
		yield return new WaitForSeconds(delay);
		Object.Destroy(go);
	}
}
