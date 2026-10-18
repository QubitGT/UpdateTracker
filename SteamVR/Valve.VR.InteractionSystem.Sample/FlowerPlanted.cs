using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample;

public class FlowerPlanted : MonoBehaviour
{
	private void Start()
	{
		Plant();
	}

	public void Plant()
	{
		StartCoroutine(DoPlant());
	}

	private IEnumerator DoPlant()
	{
		Vector3 position;
		if (Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo))
		{
			position = hitInfo.point + Vector3.up * 0.05f;
		}
		else
		{
			position = base.transform.position;
			position.y = Player.instance.transform.position.y;
		}
		GameObject planting = base.gameObject;
		planting.transform.position = position;
		planting.transform.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
		Color value = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
		value.a = 0.75f;
		planting.GetComponentInChildren<MeshRenderer>().material.SetColor("_BaseColor", value);
		Rigidbody rigidbody = planting.GetComponent<Rigidbody>();
		if (rigidbody != null)
		{
			rigidbody.isKinematic = true;
		}
		Vector3 initialScale = Vector3.one * 0.01f;
		Vector3 targetScale = Vector3.one * (1f + Random.value * 0.25f);
		float startTime = Time.time;
		float overTime = 0.5f;
		float endTime = startTime + overTime;
		while (Time.time < endTime)
		{
			planting.transform.localScale = Vector3.Slerp(initialScale, targetScale, (Time.time - startTime) / overTime);
			yield return null;
		}
		if (rigidbody != null)
		{
			rigidbody.isKinematic = false;
		}
	}
}
