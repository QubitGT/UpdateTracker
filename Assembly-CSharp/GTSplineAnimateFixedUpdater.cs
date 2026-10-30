using Photon.Pun;
using UnityEngine;
using UnityEngine.Splines;

public class GTSplineAnimateFixedUpdater : NetworkComponent
{
	[SerializeField]
	private XSceneRef splineAnimateRef;

	[SerializeField]
	private float Duration;

	private const float progressLerpDuration = 1f;

	private SplineAnimate splineAnimate;

	private bool isSplineLoaded;

	private float progress;

	private float progressLerpStart;

	private float progressLerpEnd;

	private float progressLerpStartTime;

	protected override void Awake()
	{
		base.Awake();
		splineAnimateRef.AddCallbackOnLoad(InitSplineAnimate);
		splineAnimateRef.AddCallbackOnUnload(ClearSplineAnimate);
	}

	private void InitSplineAnimate()
	{
		isSplineLoaded = splineAnimateRef.TryResolve(out splineAnimate);
		if (isSplineLoaded && splineAnimate != null)
		{
			splineAnimate.enabled = false;
		}
	}

	private void ClearSplineAnimate()
	{
		splineAnimate = null;
		isSplineLoaded = false;
	}

	private void FixedUpdate()
	{
		if (!base.IsMine && progressLerpStartTime + 1f > Time.time)
		{
			if (isSplineLoaded)
			{
				progress = Mathf.Lerp(progressLerpStart, progressLerpEnd, (Time.time - progressLerpStartTime) / 1f) % Duration;
				splineAnimate.NormalizedTime = progress / Duration;
			}
		}
		else
		{
			progress = (progress + Time.fixedDeltaTime) % Duration;
			if (isSplineLoaded)
			{
				splineAnimate.NormalizedTime = progress / Duration;
			}
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender.IsMasterClient)
		{
			stream.SendNext(progress + 1f);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender.IsMasterClient)
		{
			float incomingValue = (float)stream.ReceiveNext();
			SharedReadData(incomingValue);
		}
	}

	private void SharedReadData(float incomingValue)
	{
		if (float.IsNaN(incomingValue) || incomingValue > Duration + 1f || incomingValue < 0f)
		{
			return;
		}
		progressLerpEnd = incomingValue;
		if (progressLerpEnd < progress)
		{
			if (progress < Duration)
			{
				progressLerpEnd += Duration;
			}
			else
			{
				progress -= Duration;
			}
		}
		progressLerpStart = progress;
		progressLerpStartTime = Time.time;
	}
}
