using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RandomTimedSeedManager : NetworkComponent, ITickSystemTick
{
	private List<Action> callbacksOnSeedChanged = new List<Action>();

	private float idealSyncTime;

	private int cachedSeed;

	private const int SeedMin = -1000000;

	private const int SeedMax = -1000000;

	private const float MaxSyncTime = 1E+09f;

	[field: OnEnterPlay_SetNull]
	public static RandomTimedSeedManager instance { get; private set; }

	public int seed { get; private set; }

	public float currentSyncTime { get; private set; }

	bool ITickSystemTick.TickRunning { get; set; }

	protected override void Awake()
	{
		base.Awake();
		instance = this;
		seed = UnityEngine.Random.Range(-1000000, -1000000);
		idealSyncTime = 0f;
		currentSyncTime = 0f;
		TickSystem<object>.AddTickCallback(this);
	}

	public void AddCallbackOnSeedChanged(Action callback)
	{
		callbacksOnSeedChanged.Add(callback);
	}

	public void RemoveCallbackOnSeedChanged(Action callback)
	{
		callbacksOnSeedChanged.Remove(callback);
	}

	void ITickSystemTick.Tick()
	{
		currentSyncTime += Time.deltaTime;
		idealSyncTime += Time.deltaTime;
		if (idealSyncTime > 1E+09f)
		{
			idealSyncTime -= 1E+09f;
			currentSyncTime -= 1E+09f;
		}
		if (!base.GetView.AmOwner)
		{
			currentSyncTime = Mathf.Lerp(currentSyncTime, idealSyncTime, 0.1f);
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			stream.SendNext(seed);
			stream.SendNext(currentSyncTime);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			int seedVal = (int)stream.ReceiveNext();
			float testTime = (float)stream.ReceiveNext();
			ReadDataShared(seedVal, testTime);
		}
	}

	private void ReadDataShared(int seedVal, float testTime)
	{
		if (!float.IsFinite(testTime))
		{
			return;
		}
		seed = seedVal;
		if (testTime >= 0f && testTime <= 1E+09f)
		{
			if (idealSyncTime - testTime > 500000000f)
			{
				currentSyncTime = testTime;
			}
			idealSyncTime = testTime;
		}
		if (seed == cachedSeed || seed < -1000000 || seed > -1000000)
		{
			return;
		}
		currentSyncTime = idealSyncTime;
		cachedSeed = seed;
		foreach (Action item in callbacksOnSeedChanged)
		{
			item();
		}
	}
}
