using System.Threading.Tasks;
using GorillaNetworking;
using PlayFab;
using UnityEngine;

public class TitleDataAnimatorControl : MonoBehaviour, IBuildValidation
{
	[SerializeField]
	private string titleDataKey;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Animation animation;

	private int stateHash;

	private void OnEnable()
	{
		if (animator != null)
		{
			stateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
			animator.speed = 0f;
			animator.Play(stateHash, 0, 0f);
			SetAnimatorFrame();
		}
		else if (animation != null)
		{
			SetAnimationFrame();
		}
	}

	private async void SetAnimatorFrame()
	{
		while (PlayFabTitleDataCache.Instance == null)
		{
			await Task.Delay(100);
		}
		PlayFabTitleDataCache.Instance.GetTitleData(titleDataKey, delegate(string value)
		{
			if (float.TryParse(value, out var result))
			{
				animator.Play(stateHash, 0, Mathf.Clamp(result, 0f, 1f));
				base.gameObject.SetActive(value: true);
			}
			else
			{
				Crash("Bad value in TitleData :: Key: " + titleDataKey + " :: Value: " + value);
			}
		}, delegate(PlayFabError err)
		{
			Crash($"Playfab Error :: err: {err}");
		});
	}

	private async void SetAnimationFrame()
	{
		while (PlayFabTitleDataCache.Instance == null)
		{
			await Task.Delay(100);
		}
		PlayFabTitleDataCache.Instance.GetTitleData(titleDataKey, async delegate(string value)
		{
			if (float.TryParse(value, out var result))
			{
				animation.Play();
				{
					foreach (AnimationState item in animation)
					{
						if (item.weight == 1f)
						{
							item.speed = 0f;
							item.normalizedTime = Mathf.Clamp(result, 0f, 1f);
							break;
						}
					}
					return;
				}
			}
			Crash("Bad value in TitleData :: Key: " + titleDataKey + " :: Value: " + value);
		}, delegate(PlayFabError err)
		{
			Crash($"Playfab Error :: err: {err}");
		});
	}

	private void Crash(string s)
	{
		Debug.LogError(s);
		base.gameObject.SetActive(value: false);
	}

	bool IBuildValidation.BuildValidationCheck()
	{
		if (animator == null && animation == null)
		{
			Debug.Log("You have to hook up an Animator OR an Animation!");
			return false;
		}
		if (animator != null && animation != null)
		{
			Debug.Log("You have hooked up an Animator AND an Animation! Pick one!");
			return false;
		}
		return true;
	}
}
