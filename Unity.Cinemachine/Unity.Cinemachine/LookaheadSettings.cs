using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct LookaheadSettings
{
	public bool Enabled;

	[Tooltip("Predict the position this many seconds into the future.  Note that this setting is sensitive to noisy animation, and can amplify the noise, resulting in undesirable jitter.  If the camera jitters unacceptably when the target is in motion, turn down this setting, or animate the target more smoothly.")]
	[Range(0f, 1f)]
	public float Time;

	[Tooltip("Controls the smoothness of the lookahead algorithm.  Larger values smooth out jittery predictions and also increase prediction lag")]
	[Range(0f, 30f)]
	public float Smoothing;

	[Tooltip("If checked, movement along the Y axis will be ignored for lookahead calculations")]
	public bool IgnoreY;
}
