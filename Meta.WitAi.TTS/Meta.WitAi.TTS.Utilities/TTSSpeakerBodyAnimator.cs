using System;
using Meta.WitAi.Attributes;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;

namespace Meta.WitAi.TTS.Utilities;

public class TTSSpeakerBodyAnimator : MonoBehaviour
{
	[SerializeField]
	[ObjectType(typeof(ISpeaker), new Type[] { })]
	private UnityEngine.Object _speaker;

	public Animator Animator;

	[DropDown("GetAnimatorKeys", false, false, true, true, null, false)]
	public string AnimatorSpeakKey = "SPEAKING";

	private bool _speaking;

	private bool _pausing;

	public ISpeaker Speaker => _speaker as ISpeaker;

	protected virtual void Awake()
	{
		if (Speaker == null)
		{
			_speaker = base.gameObject.GetComponentInChildren(typeof(ISpeaker));
		}
		if (Animator == null)
		{
			Animator = base.gameObject.GetComponentInChildren<Animator>();
		}
	}

	private void Update()
	{
		RefreshPausing();
		RefreshSpeaking();
	}

	public void RefreshSpeaking()
	{
		bool flag = Speaker != null && Speaker.IsSpeaking;
		if (_speaking != flag)
		{
			_speaking = flag;
			if (Animator != null)
			{
				Animator.SetBool(AnimatorSpeakKey, _speaking);
			}
		}
	}

	public void RefreshPausing()
	{
		bool flag = Speaker != null && Speaker.IsPaused;
		if (_pausing != flag)
		{
			_pausing = flag;
			if (Animator != null)
			{
				Animator.speed = (_pausing ? 0f : 1f);
			}
		}
	}
}
