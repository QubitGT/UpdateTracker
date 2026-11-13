using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(GameEntity))]
[RequireComponent(typeof(XSceneRefTarget))]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Gorilla Tag/Game Entity/Basketball Hoop")]
public class BasketballHoop : MonoBehaviour, IGameEntityComponent, IGameEntitySerialize, IGameEntityCustomStateChange
{
	public struct ScoreEntry
	{
		public int actorNumber;

		public int score;
	}

	private const int MAX_ENTRIES = 32;

	private const float MAX_ARMED_TIME = 1f;

	private const float AT_HOME_DISTANCE_SQ = 0.0025000002f;

	private const float STATE_APPLY_TIMEOUT = 1f;

	[SerializeField]
	private GameEntity entity;

	[Tooltip("Balls that can score in this hoop. Grabbing one adds the player to the scoreboard.")]
	[SerializeField]
	private GameEntity[] balls;

	[Tooltip("Seconds before the same ball can score again")]
	[SerializeField]
	private float ballScoreCooldown = 1f;

	[Tooltip("Re-enabled every time a basket is scored")]
	[SerializeField]
	private GameObject fx;

	[Tooltip("Triggers on every client when a basket is scored")]
	[SerializeField]
	private UnityEvent OnBasket;

	[Tooltip("Triggers on every client when the game is reset")]
	[SerializeField]
	private UnityEvent OnGameReset;

	[Tooltip("The new game button can't be pressed while any player is inside this volume")]
	[SerializeField]
	private RigEventVolume gameArea;

	[Tooltip("Shown on (enabled) while the game area is empty")]
	[SerializeField]
	private GorillaPressableButton newGameButton;

	private readonly List<ScoreEntry> _scores = new List<ScoreEntry>();

	private readonly List<(GameEntity ball, float time)> _armedBalls = new List<(GameEntity, float)>();

	private float[] _lastScoreTimes;

	private Vector3[] _ballHomes;

	private Action[] _grabHandlers;

	private Collider _trigger;

	private bool _canStartNewGame;

	private readonly Queue<int> _pendingScorers = new Queue<int>();

	private bool _pendingReset;

	private bool _awaitingState;

	private float _stateRequestTime;

	public IReadOnlyList<ScoreEntry> Scores => _scores;

	public event Action OnScoresChanged;

	private void Reset()
	{
		entity = GetComponent<GameEntity>();
		entity.pickupable = false;
		Rigidbody component = GetComponent<Rigidbody>();
		component.useGravity = false;
		component.isKinematic = true;
		base.gameObject.layer = LayerMask.NameToLayer("Gorilla Trigger");
	}

	private void Awake()
	{
		_trigger = GetComponent<Collider>();
		_lastScoreTimes = new float[balls.Length];
		_ballHomes = new Vector3[balls.Length];
		for (int i = 0; i < balls.Length; i++)
		{
			if ((bool)balls[i])
			{
				_ballHomes[i] = balls[i].transform.position;
			}
		}
	}

	private void OnEnable()
	{
		RoomSystem.LeftRoomEvent += new Action(OnLeftRoom);
		RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(OnPlayerLeftRoom);
		if (_grabHandlers == null)
		{
			_grabHandlers = new Action[balls.Length];
		}
		for (int i = 0; i < balls.Length; i++)
		{
			GameEntity ball = balls[i];
			if (!ball)
			{
				continue;
			}
			ref Action reference = ref _grabHandlers[i];
			if (reference == null)
			{
				reference = delegate
				{
					AddPlayer(ball.heldByActorNumber);
				};
			}
			GameEntity gameEntity = ball;
			gameEntity.OnGrabbed = (Action)Delegate.Combine(gameEntity.OnGrabbed, _grabHandlers[i]);
		}
		if ((bool)gameArea)
		{
			gameArea.OnCountChanged += OnGameAreaCountChanged;
		}
		OnGameAreaCountChanged();
	}

	private void OnDisable()
	{
		RoomSystem.LeftRoomEvent -= new Action(OnLeftRoom);
		RoomSystem.PlayerLeftEvent -= new Action<NetPlayer>(OnPlayerLeftRoom);
		for (int i = 0; i < balls.Length; i++)
		{
			if ((bool)balls[i])
			{
				GameEntity obj = balls[i];
				obj.OnGrabbed = (Action)Delegate.Remove(obj.OnGrabbed, _grabHandlers[i]);
			}
		}
		if ((bool)gameArea)
		{
			gameArea.OnCountChanged -= OnGameAreaCountChanged;
		}
		_armedBalls.Clear();
		ClearPendingRequests();
	}

	private void Update()
	{
		if (_pendingReset || _pendingScorers.Count > 0)
		{
			ApplyNextRequest();
		}
	}

	private void OnLeftRoom()
	{
		entity.SetState(0L);
		_scores.Clear();
		ClearPendingRequests();
		this.OnScoresChanged?.Invoke();
	}

	private void OnPlayerLeftRoom(NetPlayer player)
	{
		int num = FindEntry(player.ActorNumber);
		if (num >= 0)
		{
			_scores.RemoveAt(num);
			this.OnScoresChanged?.Invoke();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!TryGetLocalPlayersBall(other, out var ball, out var rb) || rb.linearVelocity.y >= 0f || rb.worldCenterOfMass.y <= _trigger.bounds.center.y)
		{
			return;
		}
		for (int i = 0; i < _armedBalls.Count; i++)
		{
			if (_armedBalls[i].ball == ball)
			{
				_armedBalls[i] = (ball, Time.time);
				return;
			}
		}
		_armedBalls.Add((ball, Time.time));
	}

	private void OnTriggerExit(Collider other)
	{
		if (!TryGetLocalPlayersBall(other, out var ball, out var rb))
		{
			return;
		}
		int num = _armedBalls.FindIndex(((GameEntity ball, float time) a) => a.ball == ball);
		if (num < 0)
		{
			return;
		}
		float item = _armedBalls[num].time;
		_armedBalls.RemoveAt(num);
		if (!(rb.worldCenterOfMass.y >= _trigger.bounds.min.y) && !(Time.time > item + 1f))
		{
			int num2 = Array.IndexOf(balls, ball);
			if (!(Time.time < _lastScoreTimes[num2] + ballScoreCooldown))
			{
				_lastScoreTimes[num2] = Time.time;
				RequestScore();
			}
		}
	}

	private bool TryGetLocalPlayersBall(Collider other, out GameEntity ball, out Rigidbody rb)
	{
		ball = null;
		rb = other.attachedRigidbody;
		if (!rb || !rb.TryGetComponent<GameEntity>(out ball) || Array.IndexOf(balls, ball) < 0)
		{
			return false;
		}
		if (ball.IsHeld() || NetworkSystem.Instance.LocalPlayer == null)
		{
			return false;
		}
		return ball.lastHeldByActorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber;
	}

	private void RequestScore()
	{
		long state = entity.GetState();
		SendRequest(Pack(GetResetGeneration(state), GetBasketCount(state) + 1, NetworkSystem.Instance.LocalPlayer.ActorNumber));
	}

	public void RequestReset()
	{
		if (_canStartNewGame)
		{
			long state = entity.GetState();
			SendRequest(Pack(GetResetGeneration(state) + 1, 0, -1));
		}
	}

	private void SendRequest(long request)
	{
		if ((bool)entity.manager && NetworkSystem.Instance.LocalPlayer != null)
		{
			if (entity.manager.IsAuthority())
			{
				QueueRequest(request, NetworkSystem.Instance.LocalPlayer.ActorNumber);
			}
			else
			{
				entity.RequestState(request);
			}
		}
	}

	public bool CanChangeState(long newState, int playerId)
	{
		QueueRequest(newState, playerId);
		return false;
	}

	private void QueueRequest(long request, int playerId)
	{
		int resetGeneration = GetResetGeneration(entity.GetState());
		if (GetResetGeneration(request) == ((resetGeneration + 1) & 0xFFFF) && GetScorer(request) == -1)
		{
			_pendingReset = true;
			_pendingScorers.Clear();
		}
		else if (GetResetGeneration(request) == resetGeneration && GetScorer(request) == playerId && !_pendingReset && _pendingScorers.Count < 32)
		{
			_pendingScorers.Enqueue(playerId);
		}
	}

	private void ApplyNextRequest()
	{
		if (!_awaitingState || !(Time.time < _stateRequestTime + 1f))
		{
			long state = entity.GetState();
			int resetGeneration = GetResetGeneration(state);
			long newState;
			if (_pendingReset)
			{
				_pendingReset = false;
				newState = Pack(resetGeneration + 1, 0, -1);
			}
			else
			{
				newState = Pack(resetGeneration, GetBasketCount(state) + 1, _pendingScorers.Dequeue());
			}
			_awaitingState = true;
			_stateRequestTime = Time.time;
			entity.RequestState(newState);
		}
	}

	private void ClearPendingRequests()
	{
		_pendingScorers.Clear();
		_pendingReset = false;
		_awaitingState = false;
	}

	private void OnGameAreaCountChanged()
	{
		_canStartNewGame = !gameArea || gameArea.RigCount == 0;
		if ((bool)newGameButton)
		{
			newGameButton.isOn = _canStartNewGame;
			newGameButton.UpdateColor();
		}
	}

	private static long Pack(int resetGeneration, int basketCount, int scorer)
	{
		return ((long)(resetGeneration & 0xFFFF) << 48) | ((long)(basketCount & 0xFFFF) << 32) | (uint)scorer;
	}

	private static int GetResetGeneration(long state)
	{
		return (int)((state >> 48) & 0xFFFF);
	}

	private static int GetBasketCount(long state)
	{
		return (int)((state >> 32) & 0xFFFF);
	}

	private static int GetScorer(long state)
	{
		return (int)state;
	}

	private int FindEntry(int actorNumber)
	{
		for (int i = 0; i < _scores.Count; i++)
		{
			if (_scores[i].actorNumber == actorNumber)
			{
				return i;
			}
		}
		return -1;
	}

	private int AddPlayer(int actorNumber)
	{
		int num = FindEntry(actorNumber);
		if (num >= 0 || actorNumber == -1 || _scores.Count >= 32)
		{
			return num;
		}
		_scores.Add(new ScoreEntry
		{
			actorNumber = actorNumber
		});
		this.OnScoresChanged?.Invoke();
		return _scores.Count - 1;
	}

	private void AddScore(int actorNumber, int amount)
	{
		int num = AddPlayer(actorNumber);
		if (num >= 0)
		{
			ScoreEntry value = _scores[num];
			value.score += amount;
			_scores[num] = value;
			this.OnScoresChanged?.Invoke();
		}
	}

	private void ResetGame(bool live)
	{
		_scores.Clear();
		_armedBalls.Clear();
		for (int i = 0; i < balls.Length; i++)
		{
			GameEntity gameEntity = balls[i];
			if ((bool)gameEntity)
			{
				GameEntityDelayedReturn component;
				if (gameEntity.IsHeld())
				{
					AddPlayer(gameEntity.heldByActorNumber);
				}
				else if (live && gameEntity.snappedByActorNumber == -1 && gameEntity.attachedToEntityId == GameEntityId.Invalid && (gameEntity.transform.position - _ballHomes[i]).sqrMagnitude > 0.0025000002f && gameEntity.TryGetComponent<GameEntityDelayedReturn>(out component))
				{
					component.ReturnNow();
				}
			}
		}
		this.OnScoresChanged?.Invoke();
	}

	public void OnEntityInit()
	{
		this.OnScoresChanged?.Invoke();
	}

	public void OnEntityDestroy()
	{
	}

	public void OnEntityStateChange(long prevState, long newState)
	{
		_awaitingState = false;
		bool flag = (bool)entity.manager && entity.manager.IsZoneActive();
		if (GetResetGeneration(newState) != GetResetGeneration(prevState))
		{
			ResetGame(flag);
			if (flag)
			{
				OnGameReset?.Invoke();
			}
			return;
		}
		int num = (GetBasketCount(newState) - GetBasketCount(prevState)) & 0xFFFF;
		if (num != 0 && flag)
		{
			AddScore(GetScorer(newState), num);
			if ((bool)fx)
			{
				fx.SetActive(value: false);
				fx.SetActive(value: true);
			}
			OnBasket?.Invoke();
		}
	}

	public void OnGameEntitySerialize(BinaryWriter writer)
	{
		writer.Write((byte)_scores.Count);
		foreach (ScoreEntry score in _scores)
		{
			writer.Write(score.actorNumber);
			writer.Write(score.score);
		}
	}

	public void OnGameEntityDeserialize(BinaryReader reader)
	{
		_scores.Clear();
		int num = reader.ReadByte();
		for (int i = 0; i < num; i++)
		{
			ScoreEntry scoreEntry = default(ScoreEntry);
			scoreEntry.actorNumber = reader.ReadInt32();
			scoreEntry.score = reader.ReadInt32();
			ScoreEntry item = scoreEntry;
			if (_scores.Count < 32 && item.actorNumber != -1 && FindEntry(item.actorNumber) < 0)
			{
				_scores.Add(item);
			}
		}
		this.OnScoresChanged?.Invoke();
	}
}
