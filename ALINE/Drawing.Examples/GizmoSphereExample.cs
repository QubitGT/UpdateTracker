using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Drawing.Examples;

public class GizmoSphereExample : MonoBehaviourGizmos
{
	private struct Contact
	{
		public float impulse;

		public float smoothImpulse;

		public Vector3 lastPoint;

		public Vector3 lastNormal;
	}

	public Color gizmoColor = new Color(1f, 0.34509805f, 1f / 3f);

	public Color gizmoColor2 = new Color(0.30980393f, 0.8f, 79f / 85f);

	private Dictionary<Collider, Contact> contactForces = new Dictionary<Collider, Contact>();

	public override void DrawGizmos()
	{
		using (Draw.InLocalSpace(base.transform))
		{
			Draw.WireSphere(Vector3.zero, 0.5f, gizmoColor);
			foreach (Contact value in contactForces.Values)
			{
				Draw.Circle(value.lastPoint, value.lastNormal, 0.1f * value.impulse, gizmoColor2);
				Draw.SolidCircle(value.lastPoint, value.lastNormal, 0.1f * value.impulse, gizmoColor2);
			}
		}
	}

	private void FixedUpdate()
	{
		foreach (Collider item in contactForces.Keys.ToList())
		{
			Contact value = contactForces[item];
			if (value.impulse > 0.1f)
			{
				value.impulse = Mathf.Lerp(value.impulse, 0f, 10f * Time.fixedDeltaTime);
				value.smoothImpulse = Mathf.Lerp(value.impulse, value.smoothImpulse, 20f * Time.fixedDeltaTime);
				contactForces[item] = value;
			}
			else
			{
				contactForces.Remove(item);
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		ContactPoint[] contacts = collision.contacts;
		int num = 0;
		if (num < contacts.Length)
		{
			ContactPoint contactPoint = contacts[num];
			if (!contactForces.ContainsKey(collision.collider))
			{
				contactForces.Add(collision.collider, new Contact
				{
					impulse = 2f
				});
			}
			Contact value = contactForces[collision.collider];
			value.impulse = Mathf.Max(value.impulse, 1f);
			value.lastPoint = base.transform.InverseTransformPoint(contactPoint.point);
			value.lastNormal = base.transform.InverseTransformVector(contactPoint.normal);
			contactForces[collision.collider] = value;
		}
	}
}
