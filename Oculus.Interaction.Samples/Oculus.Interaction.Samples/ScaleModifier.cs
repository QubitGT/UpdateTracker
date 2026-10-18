using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ScaleModifier : MonoBehaviour
{
	public void SetScaleX(float x)
	{
		base.transform.localScale = new Vector3(x, base.transform.localScale.y, base.transform.localScale.z);
	}

	public void SetScaleY(float y)
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, y, base.transform.localScale.z);
	}

	public void SetScaleZ(float z)
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, z);
	}
}
