using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ArcadeMachine : NetworkComponent
{
	[SerializeField]
	private ArcadeGame arcadeGame;

	[SerializeField]
	private ArcadeMachineJoystick[] sticks;

	[SerializeField]
	private Renderer screen;

	[SerializeField]
	private bool networkSynchronized = true;

	[SerializeField]
	private CallLimiter soundCallLimit;

	private int buttonsStateValue;

	private AudioSource audioSource;

	private int audioSourcePriority;

	private ArcadeGame arcadeGameInstance;

	private Player[] playersPerJoystick = new Player[4];

	private float[] playerIdleTimeouts = new float[4];

	protected override void Awake()
	{
		base.Awake();
		audioSource = GetComponent<AudioSource>();
	}

	protected override void Start()
	{
		base.Start();
		if (arcadeGame != null && arcadeGame.Scale.x > 0f && arcadeGame.Scale.y > 0f)
		{
			arcadeGameInstance = Object.Instantiate(arcadeGame, screen.transform);
			arcadeGameInstance.transform.localScale = new Vector3(1f / arcadeGameInstance.Scale.x, 1f / arcadeGameInstance.Scale.y, 1f);
			screen.forceRenderingOff = true;
			arcadeGameInstance.SetMachine(this);
		}
	}

	public void PlaySound(int soundId, int priority)
	{
		if (!audioSource.isPlaying || audioSourcePriority >= priority)
		{
			audioSource.GTStop();
			audioSourcePriority = priority;
			audioSource.clip = arcadeGameInstance.audioClips[soundId];
			audioSource.GTPlay();
			if (networkSynchronized && base.IsMine)
			{
				base.GetView.RPC("ArcadeGameInstance_OnPlaySound_RPC", RpcTarget.Others, soundId);
			}
		}
	}

	public bool IsPlayerLocallyControlled(int player)
	{
		return sticks[player].heldByLocalPlayer;
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < sticks.Length; i++)
		{
			sticks[i].Init(this, i);
		}
	}

	internal override void OnDisable()
	{
		base.OnDisable();
	}

	[PunRPC]
	private void ArcadeGameInstance_OnPlaySound_RPC(int id, PhotonMessageInfo info)
	{
		if (info.Sender.IsMasterClient && id <= arcadeGameInstance.audioClips.Length && id >= 0 && soundCallLimit.CheckCallTime(Time.time))
		{
			audioSource.GTStop();
			audioSource.clip = arcadeGameInstance.audioClips[id];
			audioSource.GTPlay();
		}
	}

	public void OnJoystickStateChange(int player, ArcadeButtons buttons)
	{
		if ((object)arcadeGameInstance != null)
		{
			arcadeGameInstance.OnInputStateChange(player, buttons);
		}
	}

	public bool IsControllerInUse(int player)
	{
		if (base.IsMine)
		{
			if (playersPerJoystick[player] != null)
			{
				return Time.time < playerIdleTimeouts[player];
			}
			return false;
		}
		return (buttonsStateValue & (1 << player * 8)) != 0;
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public void ReadPlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
		arcadeGameInstance.ReadPlayerDataPUN(player, stream, info);
	}

	public void WritePlayerDataPUN(int player, PhotonStream stream, PhotonMessageInfo info)
	{
		arcadeGameInstance.WritePlayerDataPUN(player, stream, info);
	}
}
