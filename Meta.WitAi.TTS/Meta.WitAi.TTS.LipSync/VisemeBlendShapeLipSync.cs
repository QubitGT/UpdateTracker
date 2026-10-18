using UnityEngine;

namespace Meta.WitAi.TTS.LipSync;

public class VisemeBlendShapeLipSync : BaseVisemeBlendShapeLipSync
{
	public SkinnedMeshRenderer meshRenderer;

	[SerializeField]
	private VisemeLipSyncAnimator _lipsyncAnimator;

	public override SkinnedMeshRenderer SkinnedMeshRenderer => meshRenderer;

	protected override void Awake()
	{
		if (!_lipsyncAnimator)
		{
			_lipsyncAnimator = GetComponent<VisemeLipSyncAnimator>();
		}
		base.Awake();
	}

	protected virtual void OnEnable()
	{
		_lipsyncAnimator.OnVisemeLerp.AddListener(OnVisemeLerp);
	}

	protected void OnDisable()
	{
		_lipsyncAnimator.OnVisemeLerp.RemoveListener(OnVisemeLerp);
	}
}
