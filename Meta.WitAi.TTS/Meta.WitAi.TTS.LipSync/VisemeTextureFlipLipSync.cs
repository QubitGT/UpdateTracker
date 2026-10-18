using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.LipSync;

public class VisemeTextureFlipLipSync : BaseTextureFlipLipSync
{
	[FormerlySerializedAs("renderer")]
	[SerializeField]
	private Renderer visemeRenderer;

	[SerializeField]
	private VisemeLipSyncAnimator _lipSyncAnimator;

	public override Renderer Renderer => visemeRenderer;

	protected override void Awake()
	{
		base.Awake();
		if (!_lipSyncAnimator)
		{
			_lipSyncAnimator = GetComponent<VisemeLipSyncAnimator>();
		}
		if (!visemeRenderer)
		{
			visemeRenderer = GetComponent<Renderer>();
		}
	}

	protected virtual void OnEnable()
	{
		if (!visemeRenderer)
		{
			VLog.E("No renderer has been set on " + base.name + ". Viseme texture flipping will not be visible.");
			base.enabled = false;
		}
		else
		{
			_lipSyncAnimator.OnVisemeStarted?.AddListener(OnVisemeStarted);
		}
	}

	protected virtual void OnDisable()
	{
		_lipSyncAnimator.OnVisemeStarted?.RemoveListener(OnVisemeStarted);
	}
}
