using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample;

public class TargetHitEffect : MonoBehaviour
{
	public Collider targetCollider;

	public GameObject spawnObjectOnCollision;

	public bool colorSpawnedObject = true;

	public bool destroyOnTargetCollision = true;

	private void OnCollisionEnter(Collision collision)
	{
		if (!(collision.collider == targetCollider))
		{
			return;
		}
		ContactPoint contactPoint = collision.contacts[0];
		float num = 1f;
		Ray ray = new Ray(contactPoint.point - -contactPoint.normal * num, -contactPoint.normal);
		if (collision.collider.Raycast(ray, out var hitInfo, 2f) && colorSpawnedObject)
		{
			Color pixelBilinear = ((Texture2D)collision.gameObject.GetComponent<Renderer>().material.mainTexture).GetPixelBilinear(hitInfo.textureCoord.x, hitInfo.textureCoord.y);
			pixelBilinear = ((pixelBilinear.r > 0.7f && pixelBilinear.g > 0.7f && pixelBilinear.b < 0.7f) ? Color.yellow : ((Mathf.Max(pixelBilinear.r, pixelBilinear.g, pixelBilinear.b) == pixelBilinear.r) ? Color.red : ((Mathf.Max(pixelBilinear.r, pixelBilinear.g, pixelBilinear.b) != pixelBilinear.g) ? Color.yellow : Color.green)));
			pixelBilinear *= 15f;
			GameObject obj = Object.Instantiate(spawnObjectOnCollision);
			obj.transform.position = contactPoint.point;
			obj.transform.forward = ray.direction;
			Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.material.color = pixelBilinear;
				if (renderer.material.HasProperty("_EmissionColor"))
				{
					renderer.material.SetColor("_EmissionColor", pixelBilinear);
				}
			}
		}
		Debug.DrawRay(ray.origin, ray.direction, Color.cyan, 5f, depthTest: true);
		if (destroyOnTargetCollision)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
