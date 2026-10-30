using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Animation Event Controller")]
public class AnimationEventController : MonoBehaviour
{
	public GameObject fxAttack;

	public void TriggerAttackVFX()
	{
		fxAttack.SetActive(value: false);
		fxAttack.SetActive(value: true);
	}
}
