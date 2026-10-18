using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.HandGrab.Recorder;

public class RigidbodyDetector : MonoBehaviour
{
	private HashSet<Rigidbody> _ignoredBodies = new HashSet<Rigidbody>();

	public List<Rigidbody> IntersectingBodies { get; private set; } = new List<Rigidbody>();


	public void IgnoreBody(Rigidbody body)
	{
		if (!_ignoredBodies.Contains(body))
		{
			_ignoredBodies.Add(body);
		}
		if (IntersectingBodies.Contains(body))
		{
			IntersectingBodies.Remove(body);
		}
	}

	public void UnIgnoreBody(Rigidbody body)
	{
		if (_ignoredBodies.Contains(body))
		{
			_ignoredBodies.Remove(body);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && !_ignoredBodies.Contains(attachedRigidbody) && !IntersectingBodies.Contains(attachedRigidbody))
		{
			IntersectingBodies.Add(attachedRigidbody);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		Rigidbody attachedRigidbody = collider.attachedRigidbody;
		if (!(attachedRigidbody == null) && IntersectingBodies.Contains(attachedRigidbody))
		{
			IntersectingBodies.Remove(attachedRigidbody);
		}
	}
}
