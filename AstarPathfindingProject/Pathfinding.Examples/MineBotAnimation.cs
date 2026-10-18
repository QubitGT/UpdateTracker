using UnityEngine;

namespace Pathfinding.Examples;

[HelpURL("http://arongranberg.com/astar/documentation/stable/class_pathfinding_1_1_examples_1_1_mine_bot_animation.php")]
public class MineBotAnimation : VersionedMonoBehaviour
{
	public Animator anim;

	public GameObject endOfPathEffect;

	private bool isAtDestination;

	private IAstarAI ai;

	private Transform tr;

	protected Vector3 lastTarget;

	protected override void Awake()
	{
		base.Awake();
		ai = GetComponent<IAstarAI>();
		tr = GetComponent<Transform>();
	}

	private void OnTargetReached()
	{
		if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1f)
		{
			Object.Instantiate(endOfPathEffect, tr.position, tr.rotation);
			lastTarget = tr.position;
		}
	}

	protected void Update()
	{
		if (ai.reachedEndOfPath)
		{
			if (!isAtDestination)
			{
				OnTargetReached();
			}
			isAtDestination = true;
		}
		else
		{
			isAtDestination = false;
		}
		Vector3 vector = tr.InverseTransformDirection(ai.velocity);
		vector.y = 0f;
		anim.SetFloat("NormalizedSpeed", vector.magnitude / anim.transform.lossyScale.x);
	}
}
