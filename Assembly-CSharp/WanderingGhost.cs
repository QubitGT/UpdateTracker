using System;
using System.Collections.Generic;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

public class WanderingGhost : NetworkComponent
{
	[Serializable]
	public struct Waypoint
	{
		[Tooltip("The ghost will be visible when its reached to this waypoint")]
		public bool _visible;

		public Transform _transform;

		public Waypoint(bool visible, Transform tr)
		{
			_visible = visible;
			_transform = tr;
		}
	}

	private enum GhostState
	{
		Patrol,
		Idle
	}

	public float patrolSpeed = 3f;

	public float idleStayDuration = 5f;

	public float sphereColliderRadius = 2f;

	public ThrowableSetDressing[] allFlowers;

	public Vector3 flowerDisabledPosition;

	public float flowerSpawnRadius;

	public float flowerSpawnDuration;

	public LayerMask flowerGroundMask;

	public MeshRenderer mrenderer;

	public Material visibleMaterial;

	public Material scryableMaterial;

	public GameObject waypointsContainer;

	private ZoneBasedObject[] waypointRegions;

	private readonly Dictionary<ZoneBasedObject, Waypoint[]> regionWaypoints = new Dictionary<ZoneBasedObject, Waypoint[]>();

	private ZoneBasedObject lastWaypointRegion;

	private List<Waypoint> waypoints = new List<Waypoint>();

	private Waypoint currentWaypoint;

	public string debugForceWaypointRegion;

	private bool needsStartSnap;

	[SerializeField]
	private AudioSource audioSource;

	public AudioClip[] appearAudio;

	public float idleVolume;

	public AudioClip patrolAudio;

	public float patrolVolume;

	private GhostState currentState;

	private float idlePassedTime;

	public UnityAction<GameObject> TriggerHauntedObjects;

	private Vector3 hoverVelocity;

	public float hoverRectifyForce;

	public float hoverRandomForce;

	public float hoverDrag;

	private Transform hoverTransform;

	private const float WaypointRetryInterval = 0.5f;

	private float nextWaypointRetryTime;

	private const float WaypointArriveDistanceSqr = 0.040000003f;

	private static readonly int PlayerLayerMask = UnityLayer.GorillaHand.ToLayerMask() | UnityLayer.GorillaBodyCollider.ToLayerMask();

	private readonly Collider[] hitColliders = new Collider[1];

	protected override void Start()
	{
		base.Start();
		waypointRegions = waypointsContainer.GetComponentsInChildren<ZoneBasedObject>();
		ZoneBasedObject[] array = waypointRegions;
		foreach (ZoneBasedObject zoneBasedObject in array)
		{
			Transform transform = zoneBasedObject.transform;
			Waypoint[] array2 = new Waypoint[transform.childCount];
			for (int j = 0; j < array2.Length; j++)
			{
				Transform child = transform.GetChild(j);
				array2[j] = new Waypoint(child.name.Contains("_v_"), child);
			}
			regionWaypoints[zoneBasedObject] = array2;
		}
		idlePassedTime = 0f;
		ThrowableSetDressing[] array3 = allFlowers;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].anchor.position = flowerDisabledPosition;
		}
		Invoke("DelayedStart", 0.5f);
	}

	private void DelayedStart()
	{
		if (NetworkSystem.Instance.IsMasterClient || !NetworkSystem.Instance.InRoom)
		{
			needsStartSnap = true;
			TrySnapToStartWaypoint();
		}
		else
		{
			needsStartSnap = false;
			currentWaypoint = default(Waypoint);
		}
		ChangeState(GhostState.Patrol);
	}

	private void TrySnapToStartWaypoint()
	{
		PickNextWaypoint();
		if (!(currentWaypoint._transform == null))
		{
			base.transform.position = currentWaypoint._transform.position;
			PickNextWaypoint();
			needsStartSnap = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		hoverTransform = mrenderer.transform;
	}

	private void LateUpdate()
	{
		UpdateState();
		if (mrenderer.isVisible)
		{
			float deltaTime = Time.deltaTime;
			Vector3 localPosition = hoverTransform.localPosition;
			hoverVelocity -= localPosition * (hoverRectifyForce * deltaTime);
			hoverVelocity += UnityEngine.Random.insideUnitSphere * (hoverRandomForce * deltaTime);
			hoverVelocity = Vector3.MoveTowards(hoverVelocity, Vector3.zero, hoverDrag * deltaTime);
			hoverTransform.localPosition = localPosition + hoverVelocity * deltaTime;
		}
	}

	private void PickNextWaypoint()
	{
		if (waypoints.Count == 0 || lastWaypointRegion == null || !lastWaypointRegion.IsLocalPlayerInZone())
		{
			ZoneBasedObject zoneBasedObject = ZoneBasedObject.SelectRandomEligible(waypointRegions, debugForceWaypointRegion);
			if (zoneBasedObject == null)
			{
				zoneBasedObject = lastWaypointRegion;
			}
			if (zoneBasedObject == null)
			{
				currentWaypoint = default(Waypoint);
				return;
			}
			if (zoneBasedObject != lastWaypointRegion || waypoints.Count == 0)
			{
				lastWaypointRegion = zoneBasedObject;
				waypoints.Clear();
				if (regionWaypoints.TryGetValue(zoneBasedObject, out var value))
				{
					waypoints.AddRange(value);
				}
			}
		}
		if (waypoints.Count == 0)
		{
			currentWaypoint = default(Waypoint);
			return;
		}
		int index = UnityEngine.Random.Range(0, waypoints.Count);
		currentWaypoint = waypoints[index];
		waypoints.RemoveAt(index);
	}

	private bool Patrol()
	{
		float deltaTime = Time.deltaTime;
		Vector3 position = currentWaypoint._transform.position;
		base.transform.GetPositionAndRotation(out var position2, out var rotation);
		position2 = Vector3.MoveTowards(position2, position, patrolSpeed * deltaTime);
		Vector3 forward = position - position2;
		float sqrMagnitude = forward.sqrMagnitude;
		if (sqrMagnitude > 0.0001f)
		{
			rotation = Quaternion.RotateTowards(rotation, Quaternion.LookRotation(forward), 360f * deltaTime);
		}
		base.transform.SetPositionAndRotation(position2, rotation);
		return sqrMagnitude < 0.040000003f;
	}

	private bool MaybeHideGhost()
	{
		return Physics.OverlapSphereNonAlloc(base.transform.position, sphereColliderRadius, hitColliders, PlayerLayerMask) > 0;
	}

	private void ChangeState(GhostState newState)
	{
		currentState = newState;
		mrenderer.sharedMaterial = ((newState == GhostState.Idle) ? visibleMaterial : scryableMaterial);
		switch (newState)
		{
		case GhostState.Patrol:
			audioSource.GTStop();
			audioSource.volume = patrolVolume;
			audioSource.clip = patrolAudio;
			audioSource.GTPlay();
			break;
		case GhostState.Idle:
			idlePassedTime = 0f;
			audioSource.GTStop();
			audioSource.volume = idleVolume;
			audioSource.GTPlayOneShot(appearAudio.GetRandomItem());
			if (NetworkSystem.Instance.IsMasterClient)
			{
				SpawnFlowerNearby();
			}
			break;
		}
	}

	private void UpdateState()
	{
		if (!NetworkSystem.Instance.IsMasterClient)
		{
			return;
		}
		switch (currentState)
		{
		case GhostState.Patrol:
			if (currentWaypoint._transform == null)
			{
				if (!(Time.time < nextWaypointRetryTime))
				{
					nextWaypointRetryTime = Time.time + 0.5f;
					if (needsStartSnap)
					{
						TrySnapToStartWaypoint();
					}
					else
					{
						PickNextWaypoint();
					}
				}
			}
			else if (Patrol())
			{
				if (currentWaypoint._visible)
				{
					ChangeState(GhostState.Idle);
				}
				else
				{
					PickNextWaypoint();
				}
			}
			break;
		case GhostState.Idle:
			idlePassedTime += Time.deltaTime;
			if (idlePassedTime >= idleStayDuration || MaybeHideGhost())
			{
				PickNextWaypoint();
				ChangeState(GhostState.Patrol);
			}
			break;
		}
	}

	private void HauntObjects()
	{
		Collider[] array = new Collider[20];
		int num = Physics.OverlapSphereNonAlloc(base.transform.position, sphereColliderRadius, array);
		for (int i = 0; i < num; i++)
		{
			if (array[i].CompareTag("HauntedObject"))
			{
				TriggerHauntedObjects?.Invoke(array[i].gameObject);
			}
		}
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			stream.SendNext((byte)currentState);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			GhostState state = (GhostState)Convert.ToByte(stream.ReceiveNext());
			ReadDataShared(state);
		}
	}

	private void ReadDataShared(GhostState state)
	{
		GhostState num = currentState;
		currentState = state;
		if (num != currentState)
		{
			ChangeState(currentState);
		}
	}

	public override void OnOwnerChange(Player newOwner, Player previousOwner)
	{
		base.OnOwnerChange(newOwner, previousOwner);
		if (newOwner == PhotonNetwork.LocalPlayer)
		{
			ChangeState(currentState);
		}
	}

	private void SpawnFlowerNearby()
	{
		Vector3 position = base.transform.position + Vector3.down * 0.25f;
		if (Physics.Raycast(new Ray(base.transform.position + UnityEngine.Random.insideUnitCircle.x0y() * flowerSpawnRadius, Vector3.down), out var hitInfo, 3f, flowerGroundMask))
		{
			position = hitInfo.point;
		}
		ThrowableSetDressing throwableSetDressing = null;
		int num = 0;
		ThrowableSetDressing[] array = allFlowers;
		foreach (ThrowableSetDressing throwableSetDressing2 in array)
		{
			if (!throwableSetDressing2.InHand())
			{
				num++;
				if (UnityEngine.Random.Range(0, num) == 0)
				{
					throwableSetDressing = throwableSetDressing2;
				}
			}
		}
		if (throwableSetDressing != null)
		{
			if (!throwableSetDressing.IsLocalOwnedWorldShareable)
			{
				throwableSetDressing.WorldShareableRequestOwnership();
			}
			throwableSetDressing.SetWillTeleport();
			throwableSetDressing.transform.position = position;
			throwableSetDressing.StartRespawnTimer(flowerSpawnDuration);
		}
	}
}
