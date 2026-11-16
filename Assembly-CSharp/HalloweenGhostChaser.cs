using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;

public class HalloweenGhostChaser : AnnoyingNetworkComponent
{
	public enum ChaseState
	{
		Dormant = 1,
		InitialRise = 2,
		Gong = 4,
		Chasing = 8,
		Grabbing = 0x10
	}

	public float heightAboveNavmesh = 0.5f;

	public Transform followTarget;

	public Transform childGhost;

	public float velocityStep = 1f;

	public float currentSpeed;

	public float velocityIncreaseTime = 20f;

	public float riseDistance = 2f;

	public float summonDistance = 5f;

	public float timeEncircled;

	public float lastSummonCheck;

	public float timeGongStarted;

	public float summoningDuration = 30f;

	public float summoningCheckCountdown = 5f;

	public float gongDuration = 5f;

	public int summonCount = 5;

	public bool wasSurroundedLastCheck;

	public AudioSource laugh;

	public List<NetPlayer> possibleTarget;

	public AudioClip defaultLaugh;

	public AudioClip deepLaugh;

	public AudioClip gong;

	public Vector3 noisyOffset;

	public Vector3 leftArmGrabbingLocal;

	public Vector3 rightArmGrabbingLocal;

	public Vector3 leftHandGrabbingLocal;

	public Vector3 rightHandGrabbingLocal;

	public Vector3 leftHandStartingLocal;

	public Vector3 rightHandStartingLocal;

	public Vector3 ghostOffsetGrabbingLocal;

	public Vector3 ghostStartingEulerRotation;

	public Vector3 ghostGrabbingEulerRotation;

	public float maxTimeToNextHeadAngle;

	public float lastHeadAngleTime;

	public float nextHeadAngleTime;

	public float nextTimeToChasePlayer;

	public float maxNextTimeToChasePlayer;

	public float timeRiseStarted;

	public float totalTimeToRise;

	public float catchDistance;

	public float grabTime;

	public float grabDuration;

	public float grabSpeed = 1f;

	public float minGrabCooldown;

	public float lastSpeedIncreased;

	public Vector3[] headEulerAngles;

	public Transform skullTransform;

	public Transform leftArm;

	public Transform rightArm;

	public Transform leftHand;

	public Transform rightHand;

	public Transform[] spawnTransforms;

	public Transform[] spawnTransformOffsets;

	public NetPlayer targetPlayer;

	public GameObject ghostBody;

	public ChaseState currentState;

	public ChaseState lastState;

	public int spawnIndex;

	public NetPlayer grabbedPlayer;

	public Material ghostMaterial;

	public Color defaultColor;

	public Color summonedColor;

	public bool isSummoned;

	private bool targetIsOnNavMesh;

	private const float navMeshSampleRange = 5f;

	[Tooltip("The body is shown when the local player comes within this distance of the chaser")]
	public float bodyShowDistance = 80f;

	[Tooltip("The body is hidden again when the local player is further than this. Keep it above bodyShowDistance.")]
	public float bodyHideDistance = 90f;

	private const float BodyRangeCheckInterval = 0.25f;

	private float nextBodyRangeCheck;

	private bool bodyWanted;

	private bool bodyInRange;

	[Tooltip("Haptic vibration when chased by lucy")]
	public float hapticStrength = 1f;

	public float hapticDuration = 1.5f;

	private NavMeshPath path;

	private bool needsNewPath = true;

	private readonly Vector3[] pathCorners = new Vector3[64];

	public List<Vector3> points;

	public int currentTargetIdx;

	private float nextPathTimestamp;

	private bool allowedToRun = true;

	protected override void Awake()
	{
		base.Awake();
		spawnIndex = 0;
		targetPlayer = null;
		currentState = ChaseState.Dormant;
		grabTime = 0f - minGrabCooldown;
		possibleTarget = new List<NetPlayer>();
	}

	private new void Start()
	{
		NetworkSystem.Instance.RegisterSceneNetworkItem(base.gameObject);
		RoomSystem.JoinedRoomEvent += new Action(OnJoinedRoom);
	}

	private void OnDestroy()
	{
		RoomSystem.JoinedRoomEvent -= new Action(OnJoinedRoom);
	}

	private void InitializeGhost()
	{
		if (NetworkSystem.Instance.InRoom && base.IsMine)
		{
			lastHeadAngleTime = 0f;
			nextHeadAngleTime = lastHeadAngleTime + UnityEngine.Random.value * maxTimeToNextHeadAngle;
			nextTimeToChasePlayer = Time.time + UnityEngine.Random.Range(minGrabCooldown, maxNextTimeToChasePlayer);
			ghostBody.transform.localPosition = Vector3.zero;
			base.transform.eulerAngles = Vector3.zero;
			lastSpeedIncreased = 0f;
			currentSpeed = 0f;
		}
	}

	private void LateUpdate()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			currentState = ChaseState.Dormant;
		}
		else if (base.IsMine)
		{
			switch (currentState)
			{
			case ChaseState.Dormant:
			{
				if (Time.time >= nextTimeToChasePlayer)
				{
					currentState = ChaseState.InitialRise;
				}
				if (!(Time.time >= lastSummonCheck + summoningDuration))
				{
					break;
				}
				lastSummonCheck = Time.time;
				possibleTarget.Clear();
				int num = 0;
				float num2 = summonDistance * summonDistance;
				for (int i = 0; i < spawnTransforms.Length; i++)
				{
					possibleTarget.Clear();
					int num3 = 0;
					Vector3 position = spawnTransforms[i].position;
					for (int j = 0; j < VRRigCache.ActiveRigContainers.Count; j++)
					{
						if ((VRRigCache.ActiveRigContainers[j].transform.position - position).sqrMagnitude < num2)
						{
							possibleTarget.Add(VRRigCache.ActiveRigContainers[j].Creator);
							num3++;
							if (num3 >= summonCount)
							{
								break;
							}
						}
					}
					if (num3 >= summonCount)
					{
						if (!wasSurroundedLastCheck)
						{
							wasSurroundedLastCheck = true;
							break;
						}
						wasSurroundedLastCheck = false;
						isSummoned = true;
						currentState = ChaseState.Gong;
						break;
					}
					num++;
				}
				if (num == spawnTransforms.Length)
				{
					wasSurroundedLastCheck = false;
					possibleTarget.Clear();
				}
				break;
			}
			case ChaseState.Gong:
				if (Time.time > timeGongStarted + gongDuration)
				{
					currentState = ChaseState.InitialRise;
				}
				break;
			case ChaseState.InitialRise:
				if (Time.time > timeRiseStarted + totalTimeToRise)
				{
					currentState = ChaseState.Chasing;
				}
				break;
			case ChaseState.Chasing:
				if (followTarget == null || targetPlayer == null)
				{
					ChooseRandomTarget();
				}
				if (!(followTarget == null) && (followTarget.position - ghostBody.transform.position).sqrMagnitude < catchDistance * catchDistance)
				{
					currentState = ChaseState.Grabbing;
				}
				break;
			case ChaseState.Grabbing:
				if (Time.time > grabTime + grabDuration)
				{
					currentState = ChaseState.Dormant;
				}
				break;
			}
		}
		if (!allowedToRun)
		{
			currentState = ChaseState.Dormant;
		}
		if (lastState != currentState)
		{
			OnChangeState(currentState);
			lastState = currentState;
		}
		UpdateBodyRange(force: false);
		UpdateState();
	}

	private void SetBodyWanted(bool wanted)
	{
		bodyWanted = wanted;
		UpdateBodyRange(force: true);
	}

	private void UpdateBodyRange(bool force)
	{
		if (!bodyWanted)
		{
			bodyInRange = false;
			if (ghostBody.activeSelf)
			{
				ghostBody.SetActive(value: false);
			}
		}
		else if (force || !(Time.time < nextBodyRangeCheck))
		{
			nextBodyRangeCheck = Time.time + 0.25f;
			VRRig vRRig = ((VRRigCache.Instance != null && VRRigCache.Instance.localRig != null) ? VRRigCache.Instance.localRig.Rig : null);
			if (vRRig == null)
			{
				bodyInRange = true;
			}
			else
			{
				float num = (bodyInRange ? bodyHideDistance : bodyShowDistance);
				bodyInRange = (vRRig.transform.position - base.transform.position).sqrMagnitude < num * num;
			}
			if (ghostBody.activeSelf != bodyInRange)
			{
				ghostBody.SetActive(bodyInRange);
			}
		}
	}

	public void UpdateState()
	{
		switch (currentState)
		{
		case ChaseState.InitialRise:
			if (NetworkSystem.Instance.InRoom)
			{
				if (base.IsMine)
				{
					RiseHost();
				}
				if (ghostBody.activeSelf)
				{
					MoveHead();
				}
			}
			break;
		case ChaseState.Chasing:
			if (NetworkSystem.Instance.InRoom)
			{
				if (base.IsMine)
				{
					ChaseHost();
				}
				if (ghostBody.activeSelf)
				{
					MoveBodyShared();
					MoveHead();
				}
			}
			break;
		case ChaseState.Grabbing:
			if (NetworkSystem.Instance.InRoom)
			{
				if (targetPlayer == NetworkSystem.Instance.LocalPlayer)
				{
					RiseGrabbedLocalPlayer();
				}
				GrabBodyShared();
				if (ghostBody.activeSelf)
				{
					MoveHead();
				}
			}
			break;
		}
	}

	private void OnChangeState(ChaseState newState)
	{
		switch (newState)
		{
		case ChaseState.Dormant:
			SetBodyWanted(wanted: false);
			isSummoned = false;
			SetGhostColor(defaultColor);
			if (base.IsMine)
			{
				targetPlayer = null;
				InitializeGhost();
			}
			else
			{
				nextTimeToChasePlayer = Time.time + UnityEngine.Random.Range(minGrabCooldown, maxNextTimeToChasePlayer);
			}
			SetInitialRotations();
			break;
		case ChaseState.Gong:
			SetBodyWanted(wanted: true);
			if (base.IsMine)
			{
				ChooseRandomTarget();
				SetInitialSpawnPoint();
				base.transform.position = spawnTransforms[spawnIndex].position;
			}
			timeGongStarted = Time.time;
			laugh.volume = 1f;
			if (laugh.isActiveAndEnabled)
			{
				laugh.GTPlayOneShot(gong);
			}
			isSummoned = true;
			break;
		case ChaseState.InitialRise:
			timeRiseStarted = Time.time;
			SetBodyWanted(wanted: true);
			if (base.IsMine)
			{
				if (!isSummoned)
				{
					currentSpeed = 0f;
					ChooseRandomTarget();
					SetInitialSpawnPoint();
				}
				else
				{
					currentSpeed = 3f;
				}
			}
			if (isSummoned)
			{
				laugh.volume = 0.25f;
				if (laugh.isActiveAndEnabled)
				{
					laugh.GTPlayOneShot(deepLaugh);
				}
				SetGhostColor(summonedColor);
			}
			else
			{
				laugh.volume = 0.25f;
				if (laugh.isActiveAndEnabled)
				{
					laugh.GTPlay();
				}
				SetGhostColor(defaultColor);
			}
			SetInitialRotations();
			break;
		case ChaseState.Grabbing:
		{
			SetBodyWanted(wanted: true);
			grabTime = Time.time;
			if (isSummoned)
			{
				laugh.volume = 0.25f;
				if (laugh.isActiveAndEnabled)
				{
					laugh.GTPlayOneShot(deepLaugh);
				}
			}
			else
			{
				laugh.volume = 0.25f;
				if (laugh.isActiveAndEnabled)
				{
					laugh.GTPlay();
				}
			}
			leftArm.localEulerAngles = leftArmGrabbingLocal;
			rightArm.localEulerAngles = rightArmGrabbingLocal;
			leftHand.localEulerAngles = leftHandGrabbingLocal;
			rightHand.localEulerAngles = rightHandGrabbingLocal;
			ghostBody.transform.localPosition = ghostOffsetGrabbingLocal;
			ghostBody.transform.localEulerAngles = ghostGrabbingEulerRotation;
			VRRig vRRig = GorillaGameManager.StaticFindRigForPlayer(targetPlayer);
			if (vRRig != null)
			{
				followTarget = vRRig.transform;
			}
			break;
		}
		case ChaseState.Chasing:
			SetBodyWanted(wanted: true);
			ResetPath();
			break;
		}
	}

	private void SetGhostColor(Color color)
	{
		if (ghostMaterial.color != color)
		{
			ghostMaterial.color = color;
		}
	}

	private void SetInitialSpawnPoint()
	{
		float num = 1000f;
		spawnIndex = 0;
		if (followTarget == null)
		{
			return;
		}
		for (int i = 0; i < spawnTransforms.Length; i++)
		{
			float magnitude = (followTarget.position - spawnTransformOffsets[i].position).magnitude;
			if (magnitude < num)
			{
				num = magnitude;
				spawnIndex = i;
			}
		}
	}

	private void ChooseRandomTarget()
	{
		int num = -1;
		if (possibleTarget.Count >= summonCount)
		{
			int index = UnityEngine.Random.Range(0, possibleTarget.Count);
			NetPlayer netPlayer = possibleTarget[index];
			for (int i = 0; i < VRRigCache.ActiveRigContainers.Count; i++)
			{
				NetPlayer creator = VRRigCache.ActiveRigContainers[i].Creator;
				if (creator != null && creator == netPlayer)
				{
					num = i;
					break;
				}
			}
			currentSpeed = 3f;
		}
		if (num == -1)
		{
			num = UnityEngine.Random.Range(0, VRRigCache.ActiveRigContainers.Count);
		}
		possibleTarget.Clear();
		if (num < VRRigCache.ActiveRigContainers.Count)
		{
			VRRig rig = VRRigCache.ActiveRigContainers[num].Rig;
			targetPlayer = rig.creator;
			followTarget = rig.head.rigTarget;
			targetIsOnNavMesh = NavMesh.SamplePosition(followTarget.position, out var _, 5f, 1);
		}
		else
		{
			targetPlayer = null;
			followTarget = null;
		}
	}

	private void SetInitialRotations()
	{
		leftArm.localEulerAngles = Vector3.zero;
		rightArm.localEulerAngles = Vector3.zero;
		leftHand.localEulerAngles = leftHandStartingLocal;
		rightHand.localEulerAngles = rightHandStartingLocal;
		ghostBody.transform.localPosition = Vector3.zero;
		ghostBody.transform.localEulerAngles = ghostStartingEulerRotation;
	}

	private void MoveHead()
	{
		if (Time.time > nextHeadAngleTime)
		{
			skullTransform.localEulerAngles = headEulerAngles[UnityEngine.Random.Range(0, headEulerAngles.Length)];
			lastHeadAngleTime = Time.time;
			nextHeadAngleTime = lastHeadAngleTime + Mathf.Max(UnityEngine.Random.value * maxTimeToNextHeadAngle, 0.05f);
		}
	}

	private void RiseHost()
	{
		if (Time.time < timeRiseStarted + totalTimeToRise)
		{
			if (spawnIndex == -1)
			{
				spawnIndex = 0;
			}
			base.transform.position = spawnTransforms[spawnIndex].position + Vector3.up * (Time.time - timeRiseStarted) / totalTimeToRise * riseDistance;
			base.transform.rotation = spawnTransforms[spawnIndex].rotation;
		}
	}

	private void RiseGrabbedLocalPlayer()
	{
		if (Time.time > grabTime + minGrabCooldown)
		{
			grabTime = Time.time;
			GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, GorillaTagger.Instance.tagCooldown);
			GorillaTagger.Instance.StartVibration(forLeftController: true, hapticStrength, hapticDuration);
			GorillaTagger.Instance.StartVibration(forLeftController: false, hapticStrength, hapticDuration);
		}
		if (Time.time < grabTime + grabDuration)
		{
			Rigidbody rigidbody = GorillaTagger.Instance.rigidbody;
			if (!rigidbody.isKinematic)
			{
				rigidbody.linearVelocity = Vector3.up * grabSpeed;
			}
			EquipmentInteractor.instance.ForceStopClimbing();
		}
	}

	public void UpdateFollowPath(Vector3 destination, float currentSpeed)
	{
		if (needsNewPath)
		{
			GetNewPath(destination);
		}
		points[points.Count - 1] = destination;
		Vector3 vector = points[currentTargetIdx];
		base.transform.position = Vector3.MoveTowards(base.transform.position, vector, currentSpeed * Time.deltaTime);
		Vector3 eulerAngles = Quaternion.LookRotation(vector - base.transform.position).eulerAngles;
		if (Mathf.Abs(eulerAngles.x) > 45f)
		{
			eulerAngles.x = 0f;
		}
		base.transform.rotation = Quaternion.Euler(eulerAngles);
		if (currentTargetIdx + 1 < points.Count && (base.transform.position - vector).sqrMagnitude < 0.1f)
		{
			if (nextPathTimestamp <= Time.time)
			{
				GetNewPath(destination);
			}
			else
			{
				currentTargetIdx++;
			}
		}
	}

	private void GetNewPath(Vector3 destination)
	{
		if (path == null)
		{
			path = new NavMeshPath();
		}
		if (points == null)
		{
			points = new List<Vector3>();
		}
		needsNewPath = false;
		NavMesh.SamplePosition(base.transform.position, out var hit, 5f, 1);
		targetIsOnNavMesh = NavMesh.SamplePosition(destination, out var hit2, 5f, 1);
		NavMesh.CalculatePath(hit.position, hit2.position, -1, path);
		points.Clear();
		int cornersNonAlloc = path.GetCornersNonAlloc(pathCorners);
		Vector3 vector = Vector3.up * heightAboveNavmesh;
		for (int i = 0; i < cornersNonAlloc; i++)
		{
			points.Add(pathCorners[i] + vector);
		}
		points.Add(destination);
		currentTargetIdx = 0;
		nextPathTimestamp = Time.time + 2f;
	}

	public void ResetPath()
	{
		needsNewPath = true;
	}

	private void ChaseHost()
	{
		if (followTarget != null)
		{
			if (Time.time > lastSpeedIncreased + velocityIncreaseTime)
			{
				lastSpeedIncreased = Time.time;
				currentSpeed += velocityStep;
			}
			if (targetIsOnNavMesh)
			{
				UpdateFollowPath(followTarget.position, currentSpeed);
				return;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, followTarget.position, currentSpeed * Time.deltaTime);
			base.transform.rotation = Quaternion.LookRotation(followTarget.position - base.transform.position, Vector3.up);
		}
	}

	private void MoveBodyShared()
	{
		noisyOffset = new Vector3(Mathf.PerlinNoise(Time.time, 0f) - 0.5f, Mathf.PerlinNoise(Time.time, 10f) - 0.5f, Mathf.PerlinNoise(Time.time, 20f) - 0.5f);
		childGhost.localPosition = noisyOffset;
		leftArm.localEulerAngles = noisyOffset * 20f;
		rightArm.localEulerAngles = noisyOffset * -20f;
	}

	private void GrabBodyShared()
	{
		if (followTarget != null)
		{
			base.transform.rotation = followTarget.rotation;
			base.transform.position = followTarget.position;
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (NetworkSystem.Instance.GetPlayer(info.Sender) == NetworkSystem.Instance.MasterClient)
		{
			if (targetPlayer == null)
			{
				stream.SendNext(-1);
			}
			else
			{
				stream.SendNext(targetPlayer.ActorNumber);
			}
			stream.SendNext(currentState);
			stream.SendNext(spawnIndex);
			stream.SendNext(currentSpeed);
			stream.SendNext(isSummoned);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (NetworkSystem.Instance.GetPlayer(info.Sender) == NetworkSystem.Instance.MasterClient)
		{
			int playerID = (int)stream.ReceiveNext();
			targetPlayer = NetworkSystem.Instance.GetPlayer(playerID);
			currentState = (ChaseState)stream.ReceiveNext();
			spawnIndex = (int)stream.ReceiveNext();
			float f = (float)stream.ReceiveNext();
			isSummoned = (bool)stream.ReceiveNext();
			if (float.IsFinite(f))
			{
				currentSpeed = f;
			}
		}
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		base.OnOwnerChange(newOwner, previousOwner);
		if (newOwner == PhotonNetwork.LocalPlayer)
		{
			OnChangeState(currentState);
		}
	}

	public void OnJoinedRoom()
	{
		if (NetworkSystem.Instance.IsMasterClient)
		{
			InitializeGhost();
		}
		else
		{
			nextTimeToChasePlayer = Time.time + UnityEngine.Random.Range(minGrabCooldown, maxNextTimeToChasePlayer);
		}
	}

	public override void Allowed(bool v)
	{
		allowedToRun = v;
	}
}
