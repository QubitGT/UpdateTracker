#define ENABLE_PROFILER
#define DEBUG
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Fusion.LagCompensation;
using Fusion.Statistics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Fusion;

[DisallowMultipleComponent]
[AddComponentMenu("Fusion/Lag Compensation/Hitbox Manager")]
[DefaultExecutionOrder(2000)]
public sealed class HitboxManager : SimulationBehaviour, IAfterTick, IPublicFacingInterface, IBeforeSimulation, ISpawned
{
	[ReadOnly]
	[InlineHelp]
	public int BVHDepth;

	[ReadOnly]
	[InlineHelp]
	public int BVHNodes;

	[ReadOnly]
	[InlineHelp]
	public int TotalHitboxes;

	[ReadOnly]
	[InlineHelp]
	public LagCompensationDraw DrawInfo;

	private readonly List<LagCompensatedHit> _raycastHits = new List<LagCompensatedHit>();

	private RaycastQuery _raycastQuery;

	private RaycastAllQuery _raycastAllQuery;

	private SphereOverlapQuery _sphereOverlapQuery;

	private BoxOverlapQuery _boxOverlapQuery;

	private LagCompensationSettings _settings;

	private HitboxBuffer _hitboxBuffer;

	private readonly List<HitboxHit> _lagCompensatedHits = new List<HitboxHit>();

	private LagCompensationStatisticsManager _lagCompStatManager = new LagCompensationStatisticsManager();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Raycast(Vector3 origin, Vector3 direction, float length, PlayerRef player, out LagCompensatedHit hit, int layerMask = -1, HitOptions options = HitOptions.None, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		GetPlayerTickAndAlpha(player, out var tickFrom, out var tickTo, out var alpha);
		_raycastQuery.Player = player;
		_raycastQuery.Origin = origin;
		_raycastQuery.Direction = direction;
		_raycastQuery.Length = length;
		_raycastQuery.Tick = tickFrom;
		_raycastQuery.TickTo = tickTo;
		_raycastQuery.Alpha = alpha;
		_raycastQuery.LayerMask = layerMask;
		_raycastQuery.Options = options;
		_raycastQuery.TriggerInteraction = queryTriggerInteraction;
		_raycastQuery.PreProcessingDelegate = preProcessRoots;
		_raycastHits.Clear();
		if (QueryInternal(_raycastQuery, _raycastHits, clearHits: false) <= 0)
		{
			hit = default(LagCompensatedHit);
			return false;
		}
		hit = GetClosestHit(_raycastHits);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Raycast(Vector3 origin, Vector3 direction, float length, int tick, int? tickTo, float? alpha, out LagCompensatedHit hit, int layerMask = -1, HitOptions options = HitOptions.None, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		_raycastQuery.Player = default(PlayerRef);
		_raycastQuery.Origin = origin;
		_raycastQuery.Direction = direction;
		_raycastQuery.Length = length;
		_raycastQuery.Tick = tick;
		_raycastQuery.TickTo = tickTo;
		_raycastQuery.Alpha = alpha;
		_raycastQuery.LayerMask = layerMask;
		_raycastQuery.Options = options;
		_raycastQuery.TriggerInteraction = queryTriggerInteraction;
		_raycastQuery.PreProcessingDelegate = preProcessRoots;
		_raycastHits.Clear();
		if (QueryInternal(_raycastQuery, _raycastHits, clearHits: false) <= 0)
		{
			hit = default(LagCompensatedHit);
			return false;
		}
		hit = GetClosestHit(_raycastHits);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RaycastAll(Vector3 origin, Vector3 direction, float length, PlayerRef player, List<LagCompensatedHit> hits, int layerMask = -1, bool clearHits = true, HitOptions options = HitOptions.None, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		GetPlayerTickAndAlpha(player, out var tickFrom, out var tickTo, out var alpha);
		_raycastAllQuery.Player = player;
		_raycastAllQuery.Origin = origin;
		_raycastAllQuery.Direction = direction;
		_raycastAllQuery.Length = length;
		_raycastAllQuery.Tick = tickFrom;
		_raycastAllQuery.TickTo = tickTo;
		_raycastAllQuery.Alpha = alpha;
		_raycastAllQuery.LayerMask = layerMask;
		_raycastAllQuery.Options = options;
		_raycastAllQuery.TriggerInteraction = queryTriggerInteraction;
		_raycastAllQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_raycastAllQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RaycastAll(Vector3 origin, Vector3 direction, float length, int tick, int? tickTo, float? alpha, List<LagCompensatedHit> hits, int layerMask = -1, bool clearHits = true, HitOptions options = HitOptions.None, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		_raycastAllQuery.Player = default(PlayerRef);
		_raycastAllQuery.Origin = origin;
		_raycastAllQuery.Direction = direction;
		_raycastAllQuery.Length = length;
		_raycastAllQuery.Tick = tick;
		_raycastAllQuery.TickTo = tickTo;
		_raycastAllQuery.Alpha = alpha;
		_raycastAllQuery.LayerMask = layerMask;
		_raycastAllQuery.Options = options;
		_raycastAllQuery.TriggerInteraction = queryTriggerInteraction;
		_raycastAllQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_raycastAllQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapSphere(Vector3 origin, float radius, PlayerRef player, List<LagCompensatedHit> hits, int layerMask = -1, HitOptions options = HitOptions.None, bool clearHits = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		GetPlayerTickAndAlpha(player, out var tickFrom, out var tickTo, out var alpha);
		_sphereOverlapQuery.Player = player;
		_sphereOverlapQuery.Center = origin;
		_sphereOverlapQuery.Radius = radius;
		_sphereOverlapQuery.Tick = tickFrom;
		_sphereOverlapQuery.TickTo = tickTo;
		_sphereOverlapQuery.Alpha = alpha;
		_sphereOverlapQuery.LayerMask = layerMask;
		_sphereOverlapQuery.Options = options;
		_sphereOverlapQuery.TriggerInteraction = queryTriggerInteraction;
		_sphereOverlapQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_sphereOverlapQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapSphere(Vector3 origin, float radius, int tick, int? tickTo, float? alpha, List<LagCompensatedHit> hits, int layerMask = -1, HitOptions options = HitOptions.None, bool clearHits = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		_sphereOverlapQuery.Player = default(PlayerRef);
		_sphereOverlapQuery.Radius = radius;
		_sphereOverlapQuery.Tick = tick;
		_sphereOverlapQuery.TickTo = tickTo;
		_sphereOverlapQuery.Alpha = alpha;
		_sphereOverlapQuery.LayerMask = layerMask;
		_sphereOverlapQuery.Options = options;
		_sphereOverlapQuery.TriggerInteraction = queryTriggerInteraction;
		_sphereOverlapQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_sphereOverlapQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapBox(Vector3 center, Vector3 extents, Quaternion orientation, PlayerRef player, List<LagCompensatedHit> hits, int layerMask = -1, HitOptions options = HitOptions.None, bool clearHits = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		GetPlayerTickAndAlpha(player, out var tickFrom, out var tickTo, out var alpha);
		_boxOverlapQuery.Player = player;
		_boxOverlapQuery.Center = center;
		_boxOverlapQuery.Extents = extents;
		_boxOverlapQuery.Rotation = orientation;
		_boxOverlapQuery.Tick = tickFrom;
		_boxOverlapQuery.TickTo = tickTo;
		_boxOverlapQuery.Alpha = alpha;
		_boxOverlapQuery.LayerMask = layerMask;
		_boxOverlapQuery.Options = options;
		_boxOverlapQuery.TriggerInteraction = queryTriggerInteraction;
		_boxOverlapQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_boxOverlapQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapBox(Vector3 center, Vector3 extents, Quaternion orientation, int tick, int? tickTo, float? alpha, List<LagCompensatedHit> hits, int layerMask = -1, HitOptions options = HitOptions.None, bool clearHits = true, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal, PreProcessingDelegate preProcessRoots = null)
	{
		_boxOverlapQuery.Player = default(PlayerRef);
		_boxOverlapQuery.Center = center;
		_boxOverlapQuery.Extents = extents;
		_boxOverlapQuery.Rotation = orientation;
		_boxOverlapQuery.Tick = tick;
		_boxOverlapQuery.TickTo = tickTo;
		_boxOverlapQuery.Alpha = alpha;
		_boxOverlapQuery.LayerMask = layerMask;
		_boxOverlapQuery.Options = options;
		_boxOverlapQuery.TriggerInteraction = queryTriggerInteraction;
		_boxOverlapQuery.PreProcessingDelegate = preProcessRoots;
		return QueryInternal(_boxOverlapQuery, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PositionRotation(Hitbox hitbox, int tick, out Vector3 position, out Quaternion rotation, bool subtickAccuracy = false, int? tickTo = null, float? alpha = null)
	{
		HitOptions options = (subtickAccuracy ? HitOptions.SubtickAccuracy : HitOptions.None);
		QueryParams queryParams = default(QueryParams);
		queryParams.Tick = tick;
		queryParams.TickTo = tickTo;
		queryParams.Alpha = alpha;
		queryParams.Options = options;
		QueryParams queryParams2 = queryParams;
		PositionRotationQueryParams positionRotationQueryParams = default(PositionRotationQueryParams);
		positionRotationQueryParams.Hitbox = hitbox;
		positionRotationQueryParams.QueryParams = queryParams2;
		PositionRotationQueryParams param = positionRotationQueryParams;
		PositionRotationInternal(ref param, out position, out rotation);
	}

	public void PositionRotation(Hitbox hitbox, PlayerRef player, out Vector3 position, out Quaternion rotation, bool subTickAccuracy = false)
	{
		GetPlayerTickAndAlpha(player, out var tickFrom, out var tickTo, out var alpha);
		HitOptions options = (subTickAccuracy ? HitOptions.SubtickAccuracy : HitOptions.None);
		QueryParams queryParams = default(QueryParams);
		queryParams.Player = player;
		queryParams.Tick = tickFrom.Value;
		queryParams.TickTo = tickTo;
		queryParams.Alpha = alpha;
		queryParams.Options = options;
		QueryParams queryParams2 = queryParams;
		PositionRotationQueryParams positionRotationQueryParams = default(PositionRotationQueryParams);
		positionRotationQueryParams.Hitbox = hitbox;
		positionRotationQueryParams.QueryParams = queryParams2;
		PositionRotationQueryParams param = positionRotationQueryParams;
		PositionRotationInternal(ref param, out position, out rotation);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static LagCompensatedHit GetClosestHit(List<LagCompensatedHit> hits)
	{
		Assert.Check(hits != null);
		Assert.Check(hits.Count > 0, hits.Count);
		int index = 0;
		float num = hits[0].Distance;
		for (int i = 1; i < hits.Count; i++)
		{
			float distance = hits[i].Distance;
			if (distance < num)
			{
				num = distance;
				index = i;
			}
		}
		return hits[index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Raycast(RaycastQuery query, out LagCompensatedHit hit)
	{
		if (!query.Tick.HasValue)
		{
			GetPlayerTickAndAlpha(query.Player, out query.Tick, out query.TickTo, out query.Alpha);
		}
		_raycastHits.Clear();
		if (QueryInternal(query, _raycastHits, clearHits: false) <= 0)
		{
			hit = default(LagCompensatedHit);
			return false;
		}
		hit = GetClosestHit(_raycastHits);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int RaycastAll(RaycastAllQuery query, List<LagCompensatedHit> hits, bool clearHits = true)
	{
		if (!query.Tick.HasValue)
		{
			GetPlayerTickAndAlpha(query.Player, out query.Tick, out query.TickTo, out query.Alpha);
		}
		return QueryInternal(query, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapSphere(SphereOverlapQuery query, List<LagCompensatedHit> hits, bool clearHits = true)
	{
		if (!query.Tick.HasValue)
		{
			GetPlayerTickAndAlpha(query.Player, out query.Tick, out query.TickTo, out query.Alpha);
		}
		return QueryInternal(query, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int OverlapBox(BoxOverlapQuery query, List<LagCompensatedHit> hits, bool clearHits = true)
	{
		if (!query.Tick.HasValue)
		{
			GetPlayerTickAndAlpha(query.Player, out query.Tick, out query.TickTo, out query.Alpha);
		}
		return QueryInternal(query, hits, clearHits);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void GetPlayerTickAndAlpha(PlayerRef player, out int? tickFrom, out int? tickTo, out float? alpha)
	{
		SimulationInput inputForPlayer = base.Runner.Simulation.GetInputForPlayer(player);
		if (inputForPlayer == null)
		{
			tickFrom = _hitboxBuffer?.Current.Tick ?? ((int)base.Runner.Simulation.Tick);
			tickTo = null;
			alpha = null;
		}
		else if (base.Runner.IsClient)
		{
			tickFrom = inputForPlayer.Header->Tick;
			tickTo = null;
			alpha = null;
		}
		else
		{
			tickFrom = inputForPlayer.Header->InterpFrom;
			tickTo = inputForPlayer.Header->InterpTo;
			alpha = inputForPlayer.Header->InterpAlpha;
		}
		Assert.Check(tickFrom.HasValue);
	}

	public LagCompensationStatisticsSnapshot GetStatisticsSnapshot()
	{
		return _lagCompStatManager.CompletedSnapshot;
	}

	private int QueryInternal(Query query, List<LagCompensatedHit> hits, bool clearHits)
	{
		if (base.Runner.Topology == Topologies.Shared)
		{
			InternalLogStreams.LogError?.Log("Lag Compensation is not supported in Shared Mode.");
			return 0;
		}
		if (clearHits)
		{
			hits.Clear();
		}
		Assert.Check(query.Tick.HasValue);
		_lagCompensatedHits.Clear();
		_hitboxBuffer.PerformQuery(query, _lagCompensatedHits);
		int count = hits.Count;
		for (int i = 0; i < _lagCompensatedHits.Count; i++)
		{
			HitboxHit hitboxHit = _lagCompensatedHits[i];
			hits.Add(LagCompensatedHit.FromHitboxHit(ref hitboxHit));
		}
		if ((query.Options & HitOptions.IncludePhysX) != 0 || (query.Options & HitOptions.IncludeBox2D) != 0)
		{
			query.PerformStaticQuery(base.Runner, hits, query.Options);
		}
		return hits.Count - count;
	}

	private void PositionRotationInternal(ref PositionRotationQueryParams param, out Vector3 position, out Quaternion rotation)
	{
		if (base.Runner.IsClient)
		{
			param.QueryParams.TickTo = null;
			param.QueryParams.Alpha = null;
		}
		else if ((param.QueryParams.Options & HitOptions.SubtickAccuracy) == 0 && param.QueryParams.Alpha.HasValue && param.QueryParams.Alpha.Value > 0.5f)
		{
			param.QueryParams.Tick++;
		}
		_hitboxBuffer.PositionQueryInternal(ref param, out position, out rotation);
	}

	private void Init()
	{
		_settings = base.Runner.Config.LagCompensation;
		Init(GetObjects(base.Runner));
		InitQueries();
		DrawInfo = new LagCompensationDraw(_hitboxBuffer);
	}

	private void InitQueries()
	{
		RaycastQueryParams raycastQueryParams = default(RaycastQueryParams);
		SphereOverlapQueryParams sphereOverlapParams = default(SphereOverlapQueryParams);
		BoxOverlapQueryParams boxOverlapParams = default(BoxOverlapQueryParams);
		int cachedStaticCollidersSize = _settings.CachedStaticCollidersSize;
		_raycastQuery = new RaycastQuery(ref raycastQueryParams);
		_raycastAllQuery = new RaycastAllQuery(ref raycastQueryParams, new RaycastHit[cachedStaticCollidersSize], new RaycastHit2D[cachedStaticCollidersSize]);
		_sphereOverlapQuery = new SphereOverlapQuery(ref sphereOverlapParams, new Collider[cachedStaticCollidersSize], new Collider2D[cachedStaticCollidersSize]);
		_boxOverlapQuery = new BoxOverlapQuery(ref boxOverlapParams, new Collider[cachedStaticCollidersSize], new Collider2D[cachedStaticCollidersSize]);
	}

	private void Init(List<HitboxRoot> initialObjects)
	{
		int num = Mathf.Max(_settings.HitboxBufferLengthInMs, 30);
		float f = (float)num * 0.001f * (float)base.Runner.Simulation.TickRate;
		int hitboxCapacity = ((_settings.HitboxDefaultCapacity < 16) ? 16 : _settings.HitboxDefaultCapacity);
		_hitboxBuffer = new HitboxBuffer(initialObjects, Mathf.CeilToInt(f), hitboxCapacity, _settings.ExpansionFactor);
	}

	private List<HitboxRoot> GetObjects(NetworkRunner runner)
	{
		List<HitboxRoot> list = new List<HitboxRoot>();
		SimulationBehaviour[] allBehaviours = runner.GetAllBehaviours(typeof(HitboxRoot));
		for (int i = 0; i < allBehaviours.Length; i++)
		{
			SimulationBehaviour simulationBehaviour = allBehaviours[i];
			while (BehaviourUtils.IsNotNull(simulationBehaviour))
			{
				if (simulationBehaviour.CanReceiveRenderCallback)
				{
					HitboxRoot hitboxRoot = (HitboxRoot)simulationBehaviour;
					hitboxRoot.Manager = this;
					list.Add(hitboxRoot);
				}
				simulationBehaviour = simulationBehaviour.Next;
			}
		}
		return list;
	}

	private void RegisterHitboxSnapshot(int tick, int dataTick)
	{
		if (base.Runner.IsShutdown)
		{
			return;
		}
		if (_hitboxBuffer == null)
		{
			Init();
		}
		Assert.Check(_hitboxBuffer != null);
		if (base.Runner.IsServer)
		{
			Profiler.BeginSample("Server Hitbox Manager");
			AdvanceAndRegister(tick, base.Runner.Simulation.Tick);
			Profiler.EndSample();
		}
		else
		{
			if (base.Runner.Stage == SimulationStages.Resimulate)
			{
				return;
			}
			Profiler.BeginSample("Client Hitbox Manager");
			AdvanceAndRegister(tick, dataTick);
			Profiler.EndSample();
		}
		if (_hitboxBuffer.BVH != null)
		{
			BVHDepth = _hitboxBuffer.BVH.maxDepth;
			BVHNodes = _hitboxBuffer.BVH.UsedNodesCount;
		}
		TotalHitboxes = _hitboxBuffer.Current.CollidersCount;
		_lagCompStatManager.PendingSnapshot.SetBVHMaxDeep(BVHDepth, overrideValue: true);
		_lagCompStatManager.PendingSnapshot.SetBVHNodeCount(BVHNodes, overrideValue: true);
		_lagCompStatManager.PendingSnapshot.SetHitboxesCount(TotalHitboxes, overrideValue: true);
	}

	private void AdvanceAndRegister(int tick, int dataTick)
	{
		base.Runner.InvokeOnBeforeHitboxRegistration();
		Timer timer = Timer.StartNew();
		_hitboxBuffer.Advance(tick, dataTick);
		timer.Stop();
		_lagCompStatManager.PendingSnapshot.SetAdvanceBufferTime(timer.ElapsedInMilliseconds);
		SimulationBehaviour[] allBehaviours = base.Runner.GetAllBehaviours(typeof(HitboxRoot));
		for (int i = 0; i < allBehaviours.Length; i++)
		{
			SimulationBehaviour simulationBehaviour = allBehaviours[i];
			while (BehaviourUtils.IsNotNull(simulationBehaviour))
			{
				if (simulationBehaviour.CanReceiveRenderCallback)
				{
					HitboxRoot hitboxRoot = (HitboxRoot)simulationBehaviour;
					if (!hitboxRoot.Registered)
					{
						hitboxRoot.Manager = this;
						_hitboxBuffer.Add(hitboxRoot, _lagCompStatManager);
					}
					else if (hitboxRoot.InInterest)
					{
						_hitboxBuffer.Update(hitboxRoot, _lagCompStatManager);
					}
				}
				simulationBehaviour = simulationBehaviour.Next;
			}
		}
		Timer timer2 = Timer.StartNew();
		_hitboxBuffer.PosUpdateRefit();
		timer2.Stop();
		_lagCompStatManager.PendingSnapshot.SetRefitBVHTime(timer2.ElapsedInMilliseconds);
	}

	internal bool Remove(HitboxRoot root)
	{
		return _hitboxBuffer.Remove(root);
	}

	void IAfterTick.AfterTick()
	{
		if (base.Runner.IsServer)
		{
			RegisterHitboxSnapshot(base.Runner.Simulation.Tick, base.Runner.Simulation.RemoteTickPrevious);
			_lagCompStatManager.FinishPendingSnapshot();
		}
	}

	void IBeforeSimulation.BeforeSimulation(int forwardTickCount)
	{
		if (base.Runner.IsClient)
		{
			int tickStride = base.Runner.Simulation.TickStride;
			Tick tick = base.Runner.Simulation.Tick.Next(forwardTickCount * tickStride);
			Tick tick2 = base.Runner.Simulation.Tick.Next(tickStride);
			while (tick2 <= tick)
			{
				RegisterHitboxSnapshot(tick2, base.Runner.Simulation.RemoteTickPrevious);
				tick2 = tick2.Next(tickStride);
			}
			_lagCompStatManager.FinishPendingSnapshot();
		}
	}

	void ISpawned.Spawned()
	{
		Init();
	}
}
