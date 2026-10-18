using UnityEngine;

namespace Pathfinding.Examples;

[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_examples_1_1_mecanim_bridge.php")]
public class MecanimBridge : VersionedMonoBehaviour
{
	public float velocitySmoothing = 1f;

	private IAstarAI ai;

	private Animator anim;

	private Transform tr;

	private Vector3 smoothedVelocity;

	private Vector3[] prevFootPos = new Vector3[2];

	private Transform[] footTransforms;

	protected override void Awake()
	{
		base.Awake();
		ai = GetComponent<IAstarAI>();
		anim = GetComponent<Animator>();
		tr = base.transform;
		footTransforms = new Transform[2]
		{
			anim.GetBoneTransform(HumanBodyBones.LeftFoot),
			anim.GetBoneTransform(HumanBodyBones.RightFoot)
		};
	}

	private void Update()
	{
		(ai as AIBase).canMove = false;
	}

	private Vector3 CalculateBlendPoint()
	{
		if (footTransforms[0] == null || footTransforms[1] == null)
		{
			return tr.position;
		}
		Vector3 position = footTransforms[0].position;
		Vector3 position2 = footTransforms[1].position;
		Vector3 vector = (position - prevFootPos[0]) / Time.deltaTime;
		Vector3 vector2 = (position2 - prevFootPos[1]) / Time.deltaTime;
		float num = vector.magnitude + vector2.magnitude;
		float t = ((num > 0f) ? (vector.magnitude / num) : 0.5f);
		prevFootPos[0] = position;
		prevFootPos[1] = position2;
		return Vector3.Lerp(position, position2, t);
	}

	private void OnAnimatorMove()
	{
		ai.MovementUpdate(Time.deltaTime, out var nextPosition, out var nextRotation);
		Vector3 desiredVelocity = ai.desiredVelocity;
		Vector3 direction = desiredVelocity;
		direction.y = 0f;
		anim.SetFloat("InputMagnitude", (ai.reachedEndOfPath || direction.magnitude < 0.1f) ? 0f : 1f);
		Vector3 b = tr.InverseTransformDirection(direction);
		smoothedVelocity = Vector3.Lerp(smoothedVelocity, b, (velocitySmoothing > 0f) ? (Time.deltaTime / velocitySmoothing) : 1f);
		if (smoothedVelocity.magnitude < 0.4f)
		{
			smoothedVelocity = smoothedVelocity.normalized * 0.4f;
		}
		anim.SetFloat("X", smoothedVelocity.x);
		anim.SetFloat("Y", smoothedVelocity.z);
		float num = 360f;
		if (ai is AIPath aIPath)
		{
			num = aIPath.rotationSpeed;
		}
		else if (ai is RichAI richAI)
		{
			num = richAI.rotationSpeed;
		}
		Quaternion quaternion = RotateTowards(direction, Time.deltaTime * num);
		nextPosition = ai.position;
		nextRotation = ai.rotation;
		nextPosition = RotatePointAround(nextPosition, CalculateBlendPoint(), quaternion * Quaternion.Inverse(nextRotation));
		nextRotation = quaternion;
		nextRotation = anim.deltaRotation * nextRotation;
		Vector3 deltaPosition = anim.deltaPosition;
		deltaPosition.y = desiredVelocity.y * Time.deltaTime;
		nextPosition += deltaPosition;
		ai.FinalizeMovement(nextPosition, nextRotation);
	}

	private static Vector3 RotatePointAround(Vector3 point, Vector3 around, Quaternion rotation)
	{
		return rotation * (point - around) + around;
	}

	protected virtual Quaternion RotateTowards(Vector3 direction, float maxDegrees)
	{
		if (direction != Vector3.zero)
		{
			Quaternion to = Quaternion.LookRotation(direction);
			return Quaternion.RotateTowards(tr.rotation, to, maxDegrees);
		}
		return tr.rotation;
	}
}
