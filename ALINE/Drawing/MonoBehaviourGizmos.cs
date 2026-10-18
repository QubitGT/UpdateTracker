using UnityEngine;

namespace Drawing;

public abstract class MonoBehaviourGizmos : MonoBehaviour, IDrawGizmos
{
	public MonoBehaviourGizmos()
	{
	}

	private void OnDrawGizmosSelected()
	{
	}

	public virtual void DrawGizmos()
	{
	}
}
