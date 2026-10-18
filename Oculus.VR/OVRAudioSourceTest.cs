using UnityEngine;

public class OVRAudioSourceTest : MonoBehaviour
{
	public float period = 2f;

	private float nextActionTime;

	private void Start()
	{
		Material material = Object.Instantiate(GetComponent<Renderer>().material);
		material.color = Color.green;
		GetComponent<Renderer>().material = material;
		nextActionTime = Time.time + period;
	}

	private void Update()
	{
		if (Time.time > nextActionTime)
		{
			nextActionTime = Time.time + period;
			Material material = GetComponent<Renderer>().material;
			if (material.color == Color.green)
			{
				material.color = Color.red;
			}
			else
			{
				material.color = Color.green;
			}
			AudioSource component = GetComponent<AudioSource>();
			if (component == null)
			{
				Debug.LogError("Unable to find AudioSource");
			}
			else
			{
				component.Play();
			}
		}
	}
}
